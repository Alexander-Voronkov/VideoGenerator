using System.Drawing;
using Xabe.FFmpeg;

namespace VideoGenerator.Services.Interfaces;

public interface IVideoProcessingService
{
    Task<TimeSpan> SplitAsync(
        int videoCount,
        string inputFilePath,
        string outputFilePath,
        TimeSpan videoLength,
        CancellationToken token = default);
    Task<(string[] Videos, TimeSpan Duration)> SplitEqualAsync(
        TimeSpan videoLength,
        string inputFilePath,
        string outputFilePath,
        CancellationToken token = default);
    Task<TimeSpan> SplitAtAsync(
        string inputFilePath,
        string outputFilePath,
        TimeSpan startPoint,
        TimeSpan videoLength,
        CancellationToken token = default);
    Task<TimeSpan> AttachAudioAsync(
        string audioPath,
        string inputFilePath,
        string outputFilePath,
        CancellationToken token = default);
    Task<TimeSpan> DetachAudioAsync(
        string inputFilePath,
        string outputAudioPath,
        CancellationToken token = default);
    Task<TimeSpan> PlaceWatermarkAsync(
        string inputFilePath,
        string watermarkPath,
        string outputFilePath,
        Position position = Position.Bottom,
        CancellationToken token = default);
    Task<TimeSpan> WriteTextAsync(string text,
        string inputFilePath,
        string outputFilePath,
        string fontName = "Arial",
        KnownColor textColor = KnownColor.Black,
        Position textPosition = Position.Bottom,
        int topPadding = 0,
        int leftPadding = 0,
        int? fontSize = null,
        TimeSpan? startTime = null,
        TimeSpan? endTime = null,
        CancellationToken token = default);
    Task<TimeSpan> MergeVideosAsync(
        string[] inputFilePaths,
        string outputFilePath,
        CancellationToken token = default);
    Task<TimeSpan> GetSnapshot(
        string inputFilePath,
        string ouputFilePath,
        TimeSpan timing,
        CancellationToken token = default);
    Task<TimeSpan> AddSubtitlesAsync(
        string inputVideoPath,
        string outputVideoPath,
        string subtitlesPath = null,
        string assPath = null,
        CancellationToken token = default);
}
