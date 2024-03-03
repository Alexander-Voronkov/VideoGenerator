using Microsoft.Extensions.Logging;
using TikTokSplitter.Services.Interfaces;

namespace TikTokSplitter.Services.Implementations;

public class VideoDownloaderService : IVideoDownloaderService
{
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    public VideoDownloaderService(ILogger<VideoDownloaderService> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task Download(string videoUrl, string outputPath)
    {
        var bytes = await _client.GetByteArrayAsync(videoUrl);

        Directory.CreateDirectory(outputPath);

        using (var fs = new FileStream(outputPath, FileMode.OpenOrCreate))
        {
            fs.Write(bytes);
        }

        _logger.LogInformation("Youtube video is saved");
    }
}
