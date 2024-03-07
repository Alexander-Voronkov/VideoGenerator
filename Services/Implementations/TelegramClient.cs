using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    public TelegramClient(
        ILogger<ITelegramClient> logger,
        IOptions<Configuration> config,
        Client client,
        ApplicationDbContext context)
    {
        _logger = logger;
        _client = client;
        _context = context;
        _config = config;
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