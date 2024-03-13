namespace VideoGenerator.Services.Interfaces;

public interface ITiktokUploadVideoService
{
    Task UploadVideo(string path);
}
