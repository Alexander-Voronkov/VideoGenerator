using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TikTokSplitter.Exceptions;
using TikTokSplitter.Services.Interfaces;

namespace TikTokSplitter.Services.Implementations;

public class SubtitleGeneratorService : ISubtitleGeneratorService
{
    private readonly ILogger _logger;

    public SubtitleGeneratorService(ILogger<SubtitleGeneratorService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate subtitles in srt format based on video with the help of whisper
    /// </summary>
    /// <param name="inputVideoPath">Input video path</param>
    /// <returns></returns>
    public async Task GenerateSubtitles(string inputVideoPath, string outputSubtitlesPath, CancellationToken token = default)
    {
        var subtitleProcess = Process.Start(new ProcessStartInfo()
        {
            FileName = "whisper",
            Arguments = $"{inputVideoPath} --model small --output_dir {Path.GetDirectoryName(outputSubtitlesPath)} --output_format srt",
            CreateNoWindow = true,
        });

        await subtitleProcess.WaitForExitAsync(token);

        if (subtitleProcess.ExitCode != 0)
        {
            throw new SubtitleGenerationError($"An error occured while trying to transcribe the video: {await subtitleProcess.StandardOutput.ReadToEndAsync()}");
        }
    }
}
