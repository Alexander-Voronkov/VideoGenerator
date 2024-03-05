namespace VideoGenerator.Services.Interfaces;

public interface IYoutubeUploadVideoService
{
    Task UploadVideo(string path);
}
