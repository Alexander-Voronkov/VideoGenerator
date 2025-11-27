using VideoGenerator.Entities;

namespace VideoGenerator.Services.Interfaces;

public interface ISubtitleGeneratorService
{
    Task GenerateSubtitles(GenerationQueueItem pendingItem, CancellationToken token = default);
}
