namespace VideoGenerator.Services.Interfaces;

public interface ISubtitleGeneratorService
{
    Task GenerateSubtitles(string inputVideoPath, string outputSubtitlesPath, CancellationToken token = default);
}
