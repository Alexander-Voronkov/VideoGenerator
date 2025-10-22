namespace VideoGenerator.Services.Interfaces;

public interface IVideoGenerationService
{
    Task<string[]> CreateVideo(
        string objectName,
        string title,
        CancellationToken token = default);
}
