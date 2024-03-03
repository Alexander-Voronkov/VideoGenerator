using TL;

namespace VideoGenerator.Services.Interfaces;

public interface ITelegramClient
{
    public Task ProcessUpdate(UpdatesBase updates);
}
