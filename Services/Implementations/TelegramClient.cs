using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Threading;
using TL;
using VideoGenerator.Configurations;
using VideoGenerator.Enums;
using VideoGenerator.Extensions;
using VideoGenerator.Infrastructure;
using VideoGenerator.Infrastructure.Entities;
using VideoGenerator.Services.Interfaces;
using WTelegram;

namespace VideoGenerator.Services.Implementations;

public class TelegramClient : ITelegramClient
{
    static readonly Dictionary<long, User> Users = new();
    static readonly Dictionary<long, ChatBase> Chats = new();
    private readonly ILogger _logger;
    private readonly Client _client;
    private readonly ApplicationDbContext _context;
    private readonly IOptions<Configuration> _config;
    private readonly ITranslationService _translationService;

    public TelegramClient(
        ILogger<ITelegramClient> logger,
        IOptions<Configuration> config,
        Client client,
        ApplicationDbContext context,
        ITranslationService translationService)
    {
        _logger = logger;
        _client = client;
        _context = context;
        _config = config;
        _translationService = translationService;
    }

    public async Task ProcessUpdate(UpdatesBase updates)
    {
        foreach (var update in updates.UpdateList)
        {
            if (update is UpdateNewMessage unw
                && unw.message.Peer is PeerChannel channel
                && IsChannelInPool(channel))
            {
                switch (unw.message)
                {
                    case Message message:
                        await ProcessMessage(message);
                        break;
                }
            }
        }
    }

    public async Task SendMessage(long channelId, QueueMessage message, CancellationToken cancelationToken)
    {
        var channel = _context
            .Groups
            .Include(x => x.Language)
            .FirstOrDefault(x => x.IsTarget && x.GroupId == channelId);

        var input = (await _client.Messages_GetAllChats())
            .chats
            .FirstOrDefault(x => x.Value.IsChannel && x.Key == channelId)
            .Value
            ?.ToInputPeer();

        if(channel != null && input != null)
        {
            var translatedText = (await _translationService.Translate(
                message.Text, 
                new CultureInfo(channel.Language.LanguageCode), 
                cancelationToken))
                .Translations?
                .FirstOrDefault()?
                .WordText;

            var attachments = new List<InputMedia>();
            foreach (var attachment in message.Attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                var file = await _client.UploadFileAsync(stream, Guid.NewGuid().ToString(), null);
                var media = new InputMediaUploadedPhoto
                {
                    file = file,                    
                };

                attachments.Add(media);
            }
            List<Message> messages = new List<Message>();
            if(attachments.Count > 0)
            {
                var sendedMessage = await _client
                    .SendAlbumAsync(
                        input,
                        attachments,
                        translatedText ?? string.Empty);

                messages.AddRange(sendedMessage);
            }
            else
            {
                var sendedMessage = await _client.SendMessageAsync(input, translatedText ?? string.Empty);
                messages.Add(sendedMessage);
            }

            var published = new PublishedMessage
            {
                PublishedMessageId = messages[0].grouped_id > 0
                    ? messages[0].grouped_id
                    : messages[0].ID,
                SourceGroupId = message.SourceGroupId,
                TargetGroupId = channel.GroupId,
                PublishedAt = DateTime.UtcNow,
                SourceMessageId = message.QueueMessageId,
                Text = translatedText ?? string.Empty,
                StolenAt = message.StolenAt
            };

            _context.PublishedMessages.Add(published);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CreateGroup(Language language, Topic topic)
    {
        var group = new Group
        {
            TopicId = topic.TopicId,
            IsTarget = true,
            LanguageId = language.LanguageId,
            GroupName = $"{language.LanguageCode} {topic.TopicName}"
        };

        var updates = await _client.Channels_CreateChannel(group.GroupName, group.GroupName, broadcast: true);
        foreach (var update in updates.UpdateList)
        {
            if (update is UpdateChannel channel)
            {
                group.GroupId = channel.channel_id;
                var channelChannel = (await _client.Messages_GetAllChats())
                    .chats
                    .FirstOrDefault(x => x.Key == channel.channel_id);

                group.GroupLink = channelChannel.Value.MainUsername;

                var path = Path.Combine(Environment.CurrentDirectory, "Resources");
                var files = Directory.GetFiles(path).Where(x => Path.GetFileName(x).StartsWith(topic.TopicName)).ToArray();

                var filePath = files[Random.Shared.Next(0, files.Length - 1)];

                var file = await _client
                    .UploadFileAsync(filePath, 
                    (long transmitted, long totalSize) => _logger.LogInformation("{trans}{oleg}", transmitted, totalSize));

                try
                {
                    await _client.Channels_EditPhoto(channelChannel.Value.ToInputPeer() as InputPeerChannel, file.ToInputChatPhoto());
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to update photo: {ex}; {photo} {@ex}", ex.Message, filePath, ex);
                }

                _context.Groups.Add(group);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task ProcessMessage(Message message)
    {
        _logger.LogInformation("{Author} in {ChannelID}:{Channel}: {Message}",
            Peer(message.from_id) ?? message.post_author,
            message.peer_id,
            Peer(message.peer_id),
            message.message);

        var msgId = message.grouped_id > 0 ? message.grouped_id : message.ID;

        if (!_context.QueueMessages.Any(x => x.QueueMessageId == msgId)
            && !_context.PublishedMessages.Any(x => x.PublishedMessageId == msgId))
        {
            var entity = new QueueMessage
            {
                QueueMessageId = msgId,
                SourceGroupId = message.peer_id.ID,
                Text = message.message,
            };
            _context.QueueMessages.Add(entity);
            await _context.SaveChangesAsync();
        }

        if (IsLargeMedia(message.media))
        {
            return;
        }

        if (message.media is MessageMediaPhoto { photo: Photo photo })
        {
            _logger.LogInformation("Downloading for message {ID}", msgId);
            using (var stream = new MemoryStream())
            {
                var type = await _client.DownloadFileAsync(photo, stream);

                if (type is Storage_FileType.unknown or Storage_FileType.partial)
                {
                    return;
                }

                await SaveAttachment(stream.ToArray(), type.ToMime(), AttachmentContentType.Photo, msgId);
            }
        }
        else if (message.media is MessageMediaDocument { document: Document document })
        {
            _logger.LogCritical("Downloading for message {ID}", msgId);
            using (var stream = new MemoryStream())
            {
                var type = await _client.DownloadFileAsync(document, stream);
                await SaveAttachment(stream.ToArray(), type, AttachmentContentType.Document, msgId);
            }
        }
    }

    private async Task SaveAttachment(byte[] content, string mime, AttachmentContentType type, long messageID)
    {
        var attachment = new Attachment
        {
            Content = content,
            QueueMessageId = messageID,
            MimeType = mime,
            Type = type
        };

        _context.MessageAttachments.Add(attachment);

        await _context.SaveChangesAsync();
    }

    private bool IsLargeMedia(MessageMedia media)
    {
        return media switch
        {
            MessageMediaPhoto { photo: Photo photo } => photo.LargestPhotoSize.FileSize > _config.Value.MAX_TELEGRAM_FILE_SIZE,
            MessageMediaDocument { document: Document document } => document.size > _config.Value.MAX_TELEGRAM_FILE_SIZE,
            _ => true,
        };
    }

    private bool IsChannelInPool(PeerChannel channel)
    {
        return channel.channel_id == 1215896143 // TEST CHAT
            || _context.Groups.Any(x => x.GroupId == channel.ID);
    }

    private static string User(long id) => Users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
    private static string Chat(long id) => Chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
    private static string Peer(Peer peer) => peer is null ? null : peer is PeerUser user ? User(user.user_id)
        : peer is PeerChat or PeerChannel ? Chat(peer.ID) : $"Peer {peer.ID}";
}