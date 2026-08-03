using VideoGenerator.Entities;

namespace VideoGenerator.Services.Interfaces;

public interface IVideoGenerationService
{
    Task<string[]> CreateRedditBrainrotVideo(
        GenerationQueueItem queueItem,
        CancellationToken token = default);

    Task<string> CreateInterestingFactVideo(
        GenerationQueueItem queueItem,
        CancellationToken token = default);
}
