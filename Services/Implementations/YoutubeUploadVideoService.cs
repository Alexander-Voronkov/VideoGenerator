using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class YoutubeUploadVideoService : IYoutubeUploadVideoService
{
    public Task UploadVideo(string path)
    {
        return Task.CompletedTask;
    }
}
