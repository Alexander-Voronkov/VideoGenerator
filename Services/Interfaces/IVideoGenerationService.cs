namespace VideoGenerator.Services.Interfaces;

public interface IVideoGenerationService
{
    Task<string[]> CreateRedditBrainrotVideo(
        string objectName,
        string title,
        CancellationToken token = default);

    Task<string[]> CreateInterestingFactVideo(
        string objectName,
        string title,
        CancellationToken token = default);
}
