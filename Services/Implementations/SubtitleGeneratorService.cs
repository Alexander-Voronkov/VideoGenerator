using Microsoft.Extensions.Logging;
using System.Diagnostics;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

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
            Arguments = $"{inputVideoPath} --model small --output_dir {Path.GetDirectoryName(outputSubtitlesPath)} --word_timestamps True --max_line_width 25 --max_line_count 1 --output_format srt",
            CreateNoWindow = true,
        });

        await subtitleProcess.WaitForExitAsync(token);

        if (subtitleProcess.ExitCode != 0)
        {
            throw new Exception($"An error occured while trying to transcribe the video: {await subtitleProcess.StandardOutput.ReadToEndAsync()}");
        }
    }
}
