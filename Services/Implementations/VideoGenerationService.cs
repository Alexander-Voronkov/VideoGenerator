using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VideoGenerator.Services.Interfaces;
using Xabe.FFmpeg;

namespace VideoGenerator.Services.Implementations;

public class VideoGenerationService : IVideoGenerationService
{
    private readonly IVideoProcessingService _videoService;
    private readonly IAssConvertService _assConvertService;

    public VideoGenerationService(IVideoProcessingService videoService, IAssConvertService assConvertService)
    {
        _videoService = videoService;
        _assConvertService = assConvertService;
    }

    public async Task CreateVideo(string audioPath, string subtitlePath, string backgroundVideoPath, string outputPath)
    {
        // Ensure absolute paths
        audioPath = Path.GetFullPath(audioPath);
        subtitlePath = Path.GetFullPath(subtitlePath);
        backgroundVideoPath = Path.GetFullPath(backgroundVideoPath);
        outputPath = Path.GetFullPath(outputPath);

        // Get audio duration
        var mediaInfo = await FFmpeg.GetMediaInfo(audioPath);
        var audioDuration = mediaInfo.Duration;

        // Pick a random start point in background video
        var bgInfo = await FFmpeg.GetMediaInfo(backgroundVideoPath);
        var bgDuration = bgInfo.Duration;
        var maxStart = bgDuration - audioDuration;
        var randomStart = TimeSpan.FromSeconds(new Random().NextDouble() * maxStart.TotalSeconds);

        // Trim background clip to match audio length
        var trimmedBackground = Path.Combine(Path.GetTempPath(), $"trimmed_bg_{Guid.NewGuid()}.mp4");
        await _videoService.SplitAtAsync(backgroundVideoPath, trimmedBackground, randomStart, audioDuration);
        var styledSubtitle = Path.Combine(Path.GetTempPath(), $"styled_{Guid.NewGuid()}.ass");
        ConvertSrtToStyledAss(subtitlePath, styledSubtitle);

        var backgroundWithSound = Path.Combine(Path.GetTempPath(), $"bg_sound{Guid.NewGuid()}.mp4");
        await _videoService.AttachAudioAsync(audioPath, trimmedBackground, backgroundWithSound);

        await _videoService.AddSubtitlesAsync(backgroundWithSound, outputPath, assPath: styledSubtitle);

        // Clean temp files
        try
        {
            File.Delete(trimmedBackground);
            File.Delete(styledSubtitle);
        }
        catch { /* ignore */ }
    }

    private void ConvertSrtToStyledAss(string srtPath, string assPath)
    {
        var lines = File.ReadAllLines(srtPath);

        File.WriteAllText(assPath, _assConvertService.ConvertFromCrt(lines));
    }
}
