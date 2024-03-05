namespace VideoGenerator.Services.Interfaces;

public interface IChatService
{
    Task<string> SendRequest(string topic, string languageCode);
}
