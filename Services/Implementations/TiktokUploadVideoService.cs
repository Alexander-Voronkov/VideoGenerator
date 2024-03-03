using Microsoft.Extensions.Logging;
using TikTokSplitter.Services.Interfaces;

namespace TikTokSplitter.Services.Implementations;

public class TiktokUploadVideoService : ITiktokUploadVIdeoService
{
    private readonly ILogger _logger;
    private readonly HttpClient _client;

    public TiktokUploadVideoService(ILogger<TiktokUploadVideoService> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }

    public Task UploadVideo(string path)
    {
        throw new NotImplementedException();
    }
}
