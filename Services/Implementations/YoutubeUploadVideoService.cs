using TikTokSplitter.Services.Interfaces;

namespace TikTokSplitter.Services.Implementations;

public class YoutubeUploadVideoService : IYoutubeUploadVideoService
{
    public Task UploadVideo(string path)
    {
        return Task.CompletedTask;
    }
}
