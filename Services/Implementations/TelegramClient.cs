using Microsoft.Extensions.Logging;
using TL;
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

    public TelegramClient(ILogger<ITelegramClient> logger, Client client)
    {
        _logger = logger;
        _client = client;
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

        if (IsFilmNaChas(message.media))
        {
            return;
        }

        if (message.media is MessageMediaPhoto { photo: Photo photo })
        {
            _logger.LogCritical("Downloading for message {ID}", message.grouped_id);
            using var stream = new MemoryStream();
            var type = await _client.DownloadFileAsync(photo, stream);

            if (type is Storage_FileType.unknown or Storage_FileType.partial)
            {
                stream.Close();
                return;
            }

            var content = stream.ToArray();
            stream.Close();
        }
        else if (message.media is MessageMediaDocument { document: Document document })
        {
            _logger.LogCritical("Downloading for message {ID}", message.grouped_id);
            using var stream = new MemoryStream();
            var type = await _client.DownloadFileAsync(document, stream);

            var content = stream.ToArray();
            stream.Close();
        }
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
        // TODO: Replace with validation when db

        return channel.channel_id == 1215896143;
    }

    

    private static string User(long id) => Users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
    private static string Chat(long id) => Chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
    private static string Peer(Peer peer) => peer is null ? null : peer is PeerUser user ? User(user.user_id)
        : peer is PeerChat or PeerChannel ? Chat(peer.ID) : $"Peer {peer.ID}";
}
