namespace VideoGenerator.Services.Interfaces;

public interface IVideoGenerationService
{
    Task CreateVideo(
        string objectName,
        CancellationToken token = default);
}
