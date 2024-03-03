namespace VideoGenerator.Services.Interfaces;

public interface ITelegramClient
{
    public Task GetLatestMessages();
}
