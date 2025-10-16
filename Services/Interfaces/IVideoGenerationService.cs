namespace VideoGenerator.Services.Interfaces;

public interface IVideoGenerationService
{
    Task CreateVideo(
        string audioPath, 
        string subtitlePath, 
        string bucketName,
        string objectName,
        CancellationToken token = default);
}
