using Microsoft.Extensions.Logging;
using TikTokSplitter.Extensions;
using TL;
using VideoGenerator.Enums;
using VideoGenerator.Infrastructure;
using VideoGenerator.Infrastructure.Entities;
using VideoGenerator.Services.Interfaces;
using WTelegram;

namespace VideoGenerator.Services.Implementations;

public class TelegramClient : ITelegramClient
{
    public const long MAX_SIZE = 100_000_000;

    static readonly Dictionary<long, User> Users = new();
    static readonly Dictionary<long, ChatBase> Chats = new();
     
    private readonly ILogger _logger;
    private readonly Client _client;
    private readonly ApplicationDbContext _context;

    public TelegramClient(
        ILogger<ITelegramClient> logger, 
        Client client, 
        ApplicationDbContext context)
    {
        _logger = logger;
        _client = client;
        _context = context;
    }

    public async Task ProcessUpdate(UpdatesBase updates)
    {
        updates.CollectUsersChats(Users, Chats);
        foreach (var update in updates.UpdateList)
        {
            if(update is UpdateNewMessage unw 
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

        var entity = _context.Messages.FirstOrDefault(x => x.TelegramMessageId == msgId);
        if (entity == null)
        {
            // In case that we are receiving messages from test channels
            // TODO: REMOVE
            var group = _context.Groups.FirstOrDefault(x => x.GroupId == message.peer_id);
            if (group == null)
            {
                group = new Group
                {
                    GroupId = message.peer_id,
                    TopicId = null
                };
                _context.Groups.Add(group);
            }

            entity = new TelegramMessage
            {
                TelegramMessageId = msgId,
                GroupId = message.peer_id.ID,
                Text = message.message,
            };
            _context.Messages.Add(entity);
            await _context.SaveChangesAsync();
        }

        if (IsFilmNaChas(message.media))
        {
            return;
        }

        if (message.media is MessageMediaPhoto { photo: Photo photo })
        {
            _logger.LogInformation("Downloading for message {ID}", msgId);
            using var stream = new MemoryStream();
            var type = await _client.DownloadFileAsync(photo, stream);

            if (type is Storage_FileType.unknown or Storage_FileType.partial)
            {
                stream.Close();
                return;
            }

            await SaveAttachment(stream.ToArray(), type.ToMime(), AttachmentContentType.Photo, msgId);
            stream.Close();
        }
        else if (message.media is MessageMediaDocument { document: Document document })
        {
            _logger.LogCritical("Downloading for message {ID}", msgId);
            using var stream = new MemoryStream();
            var type = await _client.DownloadFileAsync(document, stream);

            await SaveAttachment(stream.ToArray(), type, AttachmentContentType.Document, msgId);
            stream.Close();
        }
    }

    private async Task SaveAttachment(byte[] content, string mime, AttachmentContentType type, long messageID)
    {
        var attachment = new Attachment
        {
            Content = content,
            TelegramMessageId = messageID,
            MimeType = mime,
            Type = type
        };

        _context.Attachments.Add(attachment);

        await _context.SaveChangesAsync();
    }

    private bool IsFilmNaChas(MessageMedia media)
    {
        return media switch
        {
            (MessageMediaPhoto { photo: Photo photo }) => photo.LargestPhotoSize.FileSize > MAX_SIZE,
            (MessageMediaDocument { document: Document document }) => document.size > MAX_SIZE,
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
