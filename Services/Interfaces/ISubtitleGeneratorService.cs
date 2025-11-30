using VideoGenerator.Entities;

namespace VideoGenerator.Services.Interfaces;

public interface ISubtitleGeneratorService
{
    Task GenerateSubtitlesForReddit(GenerationQueueItem pendingItem, CancellationToken token = default);
    Task GenerateSubtitlesForInterestingFact(InterestingFactQueueItem pendingItem, CancellationToken token = default);
}
