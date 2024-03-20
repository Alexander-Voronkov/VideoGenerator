using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Drawing;
using VideoGenerator.Exceptions;
using VideoGenerator.Extensions;
using VideoGenerator.Services.Interfaces;
using Xabe.FFmpeg;

namespace VideoGenerator.Services.Implementations;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly ILogger _logger;
    private readonly ConcurrentBag<string> _fonts;

    public VideoProcessingService(ILogger<VideoProcessingService> logger)
    {
        _logger = logger;
        _fonts = new ConcurrentBag<string>(
            Directory.GetFiles(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "Fonts"),
                "*.{ttf,fon}",
                SearchOption.AllDirectories));
    }

    /// <summary>
    /// Method for attaching audio from <paramref name="audioPath"/> to the video from <paramref name="inputFilePath"/>.
    /// </summary>
    /// <param name="audioPath">Audio file path to be attached.</param>
    /// <param name="inputFilePath">Video file path where the audio will be attached.</param>
    /// <param name="outputFilePath">Result video file path.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task AttachAudioAsync(
        string audioPath,
        string inputFilePath,
        string outputFilePath,
        CancellationToken token = default)
    {
        var attachedAudioInfo = await FFmpeg.GetMediaInfo(audioPath, token);
        var videoInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);

        if (!attachedAudioInfo.AudioStreams.Any())
        {
            throw new VideoProcessingError("No audio stream found in the input audio file.");
        }

        if (!videoInfo.VideoStreams.Any())
        {
            throw new VideoProcessingError("No video stream found in the input video file.");
        }

        var conversion = await FFmpeg.Conversions.FromSnippet.AddAudio(inputFilePath, audioPath, outputFilePath);
        var result = await conversion.Start(token);

        _logger.LogInformation($"Audio attaching took {result.Duration.TotalSeconds} seconds.");
    }

    /// <summary>
    /// Method for detaching audio from <paramref name="inputFilePath"/> and placing it to the <paramref name="outputAudioPath"/>.
    /// </summary>
    /// <param name="inputFilePath">Path of the file, where we take audio from.</param>
    /// <param name="outputAudioPath">Path to the output file.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task DetachAudioAsync(
        string inputFilePath,
        string outputAudioPath,
        CancellationToken token = default)
    {
        var inputFileInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);

        if (!inputFileInfo.AudioStreams.Any())
        {
            throw new VideoProcessingError("No audio stream found in the input video file.");
        }

        var conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(inputFilePath, outputAudioPath);
        var result = await conversion.Start(token);

        _logger.LogInformation($"Audio detaching took {result.Duration.TotalSeconds} seconds.");
    }

    /// <summary>
    /// Method for taking snapshot of video.
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <param name="timing">Timing of snapshot.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task GetSnapshot(
        string inputFilePath,
        string outputFilePath,
        TimeSpan timing,
        CancellationToken token = default)
    {
        var inputFileInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);

        if (!inputFileInfo.VideoStreams.Any())
        {
            throw new VideoProcessingError("Cannot take snapshot because there are no video streams in the input video.");
        }

        if (inputFileInfo.Duration < timing)
        {
            throw new VideoProcessingError("Cannot take snapshot because there is no such timing in the video.");
        }

        var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(
            inputFilePath,
            outputFilePath,
            timing);
        var result = await conversion.Start(token);

        _logger.LogInformation($"Snapshot taking took {result.Duration.TotalSeconds} seconds.");
    }

    /// <summary>
    /// Method for merging or concatenating some videos
    /// </summary>
    /// <param name="inputFilePaths">Paths to the input files.</param>
    /// <param name="outputFilePath">Path to the ouputfile.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task MergeVideosAsync(
        string[] inputFilePaths,
        string outputFilePath,
        CancellationToken token = default)
    {
        if (inputFilePaths.Length <= 1)
        {
            throw new VideoProcessingError("Not enough input files parameters.");
        }

        var conversion = await FFmpeg.Conversions.FromSnippet.Concatenate(outputFilePath, inputFilePaths);
        var result = await conversion.Start(token);

        _logger.LogInformation($"Merging of videos took {result.Duration.TotalSeconds} seconds.");
    }

    /// <summary>
    /// Method for placing a watermark on the video
    /// </summary>
    /// <param name="inputFilePath">Path to the input video file.</param>
    /// <param name="watermarkPath">Path to the watermark image.</param>
    /// <param name="outputFilePath">Path to the ouput video with the watermark.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task PlaceWatermarkAsync(
        string inputFilePath,
        string watermarkPath,
        string outputFilePath,
        Position position = Position.Bottom,
        CancellationToken token = default)
    {
        var conversion = await FFmpeg.Conversions.FromSnippet.SetWatermark(inputFilePath, outputFilePath, watermarkPath, position);
        var result = await conversion.Start(token);

        _logger.LogInformation($"Placing watermark took {result.Duration.TotalSeconds} seconds.");
    }

    /// <summary>
    /// Method for splitting video
    /// </summary>
    /// <param name="videoCount">Count of video pieces.</param>
    /// <param name="videoLength">Length of single video piece.</param>
    /// <param name="inputFilePath">Path to the input video file to be split.</param>
    /// <param name="outputFolderPath">Path to the output folder, where split video pieces to be saved.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task SplitAsync(
        int videoCount,
        string inputFilePath,
        string outputFolderPath,
        TimeSpan videoLength,
        CancellationToken token = default)
    {
        var inputVideoInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);
        var i = 0;
        var start = TimeSpan.FromSeconds(0);
        var totalDuration = 0;
        var tasks = new List<Task>();

        for (; start < inputVideoInfo.Duration && i < videoCount; start = start.Add(videoLength), i++)
        {
            tasks.Add(Task.Factory.StartNew(async () =>
            {
                var conversion = await FFmpeg.Conversions.FromSnippet.Split(
                inputFilePath,
                string.Concat(
                    outputFolderPath,
                    "/",
                    Path.GetFileNameWithoutExtension(inputFilePath),
                    Guid.NewGuid().ToString(),
                    Path.GetExtension(inputFilePath)),
                start,
                videoLength);
                var result = await conversion.Start(token);
                Interlocked.Add(ref totalDuration, (int)result.Duration.TotalSeconds);
            }, token));
        }

        await Task.WhenAll(tasks);

        _logger.LogInformation($"Splitting of the video took {totalDuration} seconds.");
    }

    /// <summary>
    /// Method for splitting video at some point.
    /// </summary>
    /// <param name="videoLength">Length of single video piece.</param>
    /// <param name="inputFilePath">Path to the input video file to be split.</param>
    /// <param name="outputFilePath">Path to the output file, where the split video to be saved.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task SplitAtAsync(
        string inputFilePath,
        string outputFilePath,
        TimeSpan startPoint,
        TimeSpan videoLength,
        CancellationToken token = default)
    {
        var inputVideoInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);

        if (inputVideoInfo.Duration <= startPoint)
        {
            throw new VideoProcessingError($"Cannot start splitting from {startPoint} as the video duration is {inputVideoInfo.Duration}");
        }

        var conversion = await FFmpeg.Conversions.FromSnippet.Split(inputFilePath, outputFilePath, startPoint, videoLength);
        var result = await conversion.Start(token);

        _logger.LogInformation($"Splitting of the video took {result.Duration.TotalSeconds} seconds.");
    }

    /// <summary>
    /// Method for splitting video to equal pieces not depending on the video length
    /// </summary>
    /// <param name="videoCount">Pieces of videos count.</param>
    /// <param name="inputFilePath">Path to the input video file to be split.</param>
    /// <param name="outputFolderPath">Path to the output folder, where split video pieces to be saved.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task SplitEqualAsync(
        int videoCount,
        string inputFilePath,
        string outputFolderPath,
        CancellationToken token = default)
    {
        var inputVideoInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);
        var tasks = new List<Task>();
        var totalDuration = 0;
        var start = TimeSpan.FromSeconds(0);
        var i = 0;
        var videoLength = TimeSpan.FromSeconds(inputVideoInfo.Duration.TotalSeconds / videoCount);

        for (; start < inputVideoInfo.Duration && i < videoCount; start = start.Add(videoLength), i++)
        {
            tasks.Add(Task.Factory.StartNew(async () =>
            {
                var conversion = await FFmpeg.Conversions.FromSnippet.Split(
                inputFilePath,
                string.Concat(
                    outputFolderPath,
                    "/",
                    Path.GetFileNameWithoutExtension(inputFilePath),
                    Guid.NewGuid().ToString(),
                    Path.GetExtension(inputFilePath)),
                start,
                videoLength);
                var result = await conversion.Start(token);
                Interlocked.Add(ref totalDuration, (int)result.Duration.TotalSeconds);
            }, token));
        }

        await Task.WhenAll(tasks);

        _logger.LogInformation($"Splitting of the video took {totalDuration} seconds.");
    }

    /// <summary>
    /// Method for writing text on video in some position
    /// </summary>
    /// <param name="text">Text to be written.</param>
    /// <param name="inputFilePath">Input video file path.</param>
    /// <param name="outputFilePath">Output video path with the text.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task WriteTextAsync(
        string text,
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
        CancellationToken token = default)
    {
        var inputFileInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);

        if (!inputFileInfo.VideoStreams.Any())
        {
            throw new VideoProcessingError("Cannot write text on a file without video stream.");
        }

        var font = _fonts.FirstOrDefault(x => x.Contains(fontName, StringComparison.InvariantCultureIgnoreCase))
            ?? _fonts.ElementAt(Random.Shared.Next(0, _fonts.Count)) ?? fontName;
        var stream = inputFileInfo.VideoStreams.FirstOrDefault();
        var color = Color.FromKnownColor(textColor).ToHexColor();
        var position = textPosition.ToFFMpegPosition(leftPadding, topPadding);
        var videoStream = inputFileInfo.VideoStreams.First();
        fontSize ??= videoStream.Height / 15;
        startTime ??= TimeSpan.FromSeconds(0);
        endTime ??= inputFileInfo.Duration;

        var result = await FFmpeg.Conversions
            .New()
            .AddStream(inputFileInfo.VideoStreams)
            .AddStream(inputFileInfo.AudioStreams)
            .AddParameter($@"-vf ""drawtext=text='{text}':font='{font}':fontsize={fontSize}:{position}:enable='between(t,{startTime?.ToFFmpeg()},{endTime?.ToFFmpeg()})'""")
            .SetOutput(outputFilePath)
            .Start(token);

        _logger.LogInformation($"Writing text on video took {result.Duration.TotalSeconds} seconds.");
    }

    /// <summary>
    /// Method for writing subtitles on videos
    /// </summary>
    /// <param name="inputVideoPath">Path to the input video</param>
    /// <param name="outputVideoPath">Path to the output video with subtitles</param>
    /// <param name="subtitlesPath">Path to the subtitles in .srt format</param>
    /// <returns></returns>
    public async Task AddSubtitlesAsync(
        string inputVideoPath,
        string outputVideoPath,
        string subtitlesPath,
        CancellationToken token = default)
    {
        var conversion = await FFmpeg.Conversions.FromSnippet.AddSubtitle(
            inputVideoPath,
            outputVideoPath,
            subtitlesPath);

        var result = await conversion.Start(token);

        _logger.LogInformation($"Adding subtitles took {result.Duration.TotalSeconds}");
    }
}