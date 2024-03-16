using TL;
using VideoGenerator.Infrastructure.Entities;

namespace VideoGenerator.Services.Interfaces;

public interface ITelegramClient
{
    Task ProcessUpdate(UpdatesBase updates);

    Task SendMessage(long channelId, QueueMessage message, CancellationToken cancelationToken);

    Task CreateGroup(Language language, Topic topic);
}
