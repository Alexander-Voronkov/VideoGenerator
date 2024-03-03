using System.Drawing;
using Xabe.FFmpeg;

namespace TikTokSplitter.Services.Interfaces;

public interface IVideoProcessingService
{
    Task SplitAsync(
        int videoCount,
        string inputFilePath,
        string outputFilePath,
        TimeSpan videoLength,
        CancellationToken token = default);
    Task SplitEqualAsync(
        int videoCount,
        string inputFilePath,
        string outputFilePath,
        CancellationToken token = default);
    Task SplitAtAsync(
        string inputFilePath,
        string outputFilePath,
        TimeSpan startPoint,
        TimeSpan videoLength,
        CancellationToken token = default);
    Task AttachAudioAsync(
        string audioPath,
        string inputFilePath,
        string outputFilePath,
        CancellationToken token = default);
    Task DetachAudioAsync(
        string inputFilePath,
        string outputAudioPath,
        CancellationToken token = default);
    Task PlaceWatermarkAsync(
        string inputFilePath,
        string watermarkPath,
        string outputFilePath,
        Position position = Position.Bottom,
        CancellationToken token = default);
    Task WriteTextAsync(string text,
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
    Task MergeVideosAsync(
        string[] inputFilePaths,
        string outputFilePath,
        CancellationToken token = default);
    Task GetSnapshot(
        string inputFilePath,
        string ouputFilePath,
        TimeSpan timing,
        CancellationToken token = default);
}
