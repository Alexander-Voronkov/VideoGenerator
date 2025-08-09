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

    public VideoGenerationService(IVideoProcessingService videoService)
    {
        _videoService = videoService;
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

        //var styledSubtitle = "C:\\Users\\Zoranais\\source\\repos\\VideoGaynerator\\testaudio.ass";

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

    private static void ConvertSrtToStyledAss(string srtPath, string assPath)
    {
        var style = "[Script Info]\n" +
                    "ScriptType: v4.00+\n" +
                    "PlayResX: 384\n" +
                    "PlayResY: 288\n" +
                    "ScaledBorderAndShadow: yes\n" +
                    "\n" +
                    "[V4+ Styles]\n" +
                    "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n" +
                    "Style: Default,Arial,16,yellow,green,white,red,0,0,0,0,100,100,0,0,1,1,0,2,10,10,10,0\n" +
                    "\n" +
                    "[Events]\n" +
                    "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n";

        var lines = File.ReadAllLines(srtPath);
        var eventLines = SrtToAssEvents(lines);
        File.WriteAllText(assPath, style + string.Join("\n", eventLines));
    }

    private static string[] SrtToAssEvents(string[] srtLines)
    {
        var events = new List<string>();
        int i = 0;

        while (i < srtLines.Length)
        {
            // Skip index line
            if (string.IsNullOrWhiteSpace(srtLines[i])) { i++; continue; }
            i++;

            // Time line
            var timeParts = srtLines[i++].Split(new[] { " --> " }, StringSplitOptions.None);
            var start = timeParts[0].Replace(",", ".").Substring(0, timeParts[0].Length - 1);
            var end = timeParts[1].Replace(",", ".").Substring(0, timeParts[0].Length - 1);

            // Subtitle text
            var textBuilder = new StringBuilder();
            while (i < srtLines.Length && !string.IsNullOrWhiteSpace(srtLines[i]))
            {
                textBuilder.Append(srtLines[i++] + "\\N");
            }

            // Remove trailing \N
            var text = textBuilder.ToString().TrimEnd('\\', 'N');

            events.Add($"Dialogue: 0,{start},{end},Default,,0,0,0,,{text}");
        }
        return events.ToArray();
    }
}
