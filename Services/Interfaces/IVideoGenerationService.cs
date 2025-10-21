namespace VideoGenerator.Services.Interfaces;

public interface IVideoGenerationService
{
    Task<string[]> CreateVideo(
        string objectName,
        CancellationToken token = default);
}
