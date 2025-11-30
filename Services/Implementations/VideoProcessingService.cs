using Microsoft.Extensions.Logging;
using System.Drawing;
using VideoGenerator.Extensions;
using VideoGenerator.Services.Interfaces;
using Xabe.FFmpeg;

namespace VideoGenerator.Services.Implementations;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly ILogger _logger;
    private int lastProgress = 0;

    public VideoProcessingService(ILogger<VideoProcessingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Method for attaching audio from <paramref name="audioPath"/> to the video from <paramref name="inputFilePath"/>.
    /// </summary>
    /// <param name="audioPath">Audio file path to be attached.</param>
    /// <param name="inputFilePath">Video file path where the audio will be attached.</param>
    /// <param name="outputFilePath">Result video file path.</param>
    /// <param name="volume">Volume of attached audio, Defaults to 1</param>
    /// <param name="overrideOriginalAudio">If set to true - original audio would be overridden</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task<TimeSpan> AttachAudioAsync(
        string audioPath,
        string inputFilePath,
        string outputFilePath,
        float volume = 1F,
        bool overrideOriginalAudio = false,
        CancellationToken token = default)
    {
        var attachedAudioInfo = await FFmpeg.GetMediaInfo(audioPath, token);
        var videoInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);

        if (!attachedAudioInfo.AudioStreams.Any())
        {
            throw new Exception("No audio stream found in the input audio file.");
        }

        if (!videoInfo.VideoStreams.Any())
        {
            throw new Exception("No video stream found in the input video file.");
        }

        var conversion = FFmpeg.Conversions.New()
            // Input video
            .AddParameter($"-i \"{inputFilePath}\"", ParameterPosition.PreInput)
            // Input audio
            .AddParameter($"-i \"{audioPath}\"", ParameterPosition.PreInput);
        if (overrideOriginalAudio)
        {
            // Replace original audio
            conversion.AddParameter($"-map 0:v:0 -map 1:a:0 -c:v copy -c:a aac -filter:a volume={volume}");
        }
        else
        {
            // Mix original audio with new audio, apply volume to new audio only
            conversion.AddParameter(
                $"-filter_complex [1:a]volume={volume}[a1];[0:a][a1]amix=inputs=2:duration=longest:dropout_transition=2[aout] -map 0:v -map [aout] -c:v copy -c:a aac");
        }
            // Overwrite without asking
            conversion = conversion
                .AddParameter("-y")
                // Output path
                .SetOutput(outputFilePath).SetOverwriteOutput(true);
        conversion.OnProgress += Conversion_OnProgress;

        var result = await conversion.Start(token);

        _logger.LogInformation("Audio attaching took {Duration}", result.Duration);
        return result.Duration;
    }

    /// <summary>
    /// Method for detaching audio from <paramref name="inputFilePath"/> and placing it to the <paramref name="outputAudioPath"/>.
    /// </summary>
    /// <param name="inputFilePath">Path of the file, where we take audio from.</param>
    /// <param name="outputAudioPath">Path to the output file.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task<TimeSpan> DetachAudioAsync(
        string inputFilePath,
        string outputAudioPath,
        CancellationToken token = default)
    {
        var inputFileInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);

        if (!inputFileInfo.AudioStreams.Any())
        {
            throw new Exception("No audio stream found in the input video file.");
        }

        var conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(inputFilePath, outputAudioPath);
        var result = await conversion.Start(token);

        _logger.LogInformation("Audio detaching took {Duration}.", result.Duration);
        return result.Duration;
    }

    /// <summary>
    /// Method for taking snapshot of video.
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <param name="timing">Timing of snapshot.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task<TimeSpan> GetSnapshot(
        string inputFilePath,
        string outputFilePath,
        TimeSpan timing,
        CancellationToken token = default)
    {
        var inputFileInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);

        if (!inputFileInfo.VideoStreams.Any())
        {
            throw new Exception("Cannot take snapshot because there are no video streams in the input video.");
        }

        if (inputFileInfo.Duration < timing)
        {
            throw new Exception("Cannot take snapshot because there is no such timing in the video.");
        }

        var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(
            inputFilePath,
            outputFilePath,
            timing);
        var result = await conversion.Start(token);

		_logger.LogInformation("Snapshot taking took {Duration}.", result.Duration);

		return result.Duration;
    }

	/// <summary>
	/// Method for merging or concatenating some videos without re-encoding.
	/// Requires that all input videos have the same codec, resolution, FPS, and container.
	/// </summary>
	/// <param name="inputFilePaths">Paths to the input files.</param>
	/// <param name="outputFilePath">Path to the output file.</param>
	/// <param name="token">Cancellation token.</param>
	/// <returns></returns>
	public async Task<TimeSpan> MergeVideosAsync(
		string[] inputFilePaths,
		string outputFilePath,
		CancellationToken token = default)
	{
		if (inputFilePaths.Length <= 1)
		{
			throw new ArgumentException("Not enough input files to concatenate.", nameof(inputFilePaths));
		}

		var tempFileList = Path.Combine(Path.GetTempPath(), $"filelist_{Guid.NewGuid()}.txt");
		await File.WriteAllLinesAsync(tempFileList, inputFilePaths.Select(p => $"file '{Path.GetFullPath(p).Replace("'", "'\\''")}'"), token);

		try
		{
			var conversion = FFmpeg.Conversions.New()
				.AddParameter($"-f concat -safe 0 -i \"{tempFileList}\" -c copy \"{outputFilePath}\"", ParameterPosition.PreInput);

			var result = await conversion.Start(token);

			_logger.LogInformation("Merging of videos took {Duration}.", result.Duration);

			return result.Duration;
		}
		finally
		{
			if (File.Exists(tempFileList))
				File.Delete(tempFileList);
		}
	}

	/// <summary>
	/// Method for placing a watermark on the video
	/// </summary>
	/// <param name="inputFilePath">Path to the input video file.</param>
	/// <param name="watermarkPath">Path to the watermark image.</param>
	/// <param name="outputFilePath">Path to the ouput video with the watermark.</param>
	/// <param name="token">Cancellation token.</param>
	/// <returns></returns>
	public async Task<TimeSpan> PlaceWatermarkAsync(
        string inputFilePath,
        string watermarkPath,
        string outputFilePath,
        Position position = Position.Bottom,
        CancellationToken token = default)
    {
        var conversion = await FFmpeg.Conversions.FromSnippet.SetWatermark(inputFilePath, outputFilePath, watermarkPath, position);
        var result = await conversion.Start(token);
		_logger.LogInformation("Placing watermark took {Duration}.", result.Duration);

		return result.Duration;
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
    public async Task<TimeSpan> SplitAsync(
        int videoCount,
        string inputFilePath,
        string outputFolderPath,
        TimeSpan videoLength,
        CancellationToken token = default)
    {
        var inputVideoInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);
        var i = 0;
        var start = TimeSpan.FromSeconds(0);
        var totalDuration = TimeSpan.Zero;
        var tasks = new List<Task>();
        var lockObj = new object();

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
                lock(lockObj)
                {
                    totalDuration += result.Duration;
                }
            }, token));
        }

        await Task.WhenAll(tasks);
		_logger.LogInformation("Splitting of the video took {totalDuration}.", totalDuration);

		return totalDuration;
    }

    /// <summary>
    /// Method for splitting video at some point.
    /// </summary>
    /// <param name="videoLength">Length of single video piece.</param>
    /// <param name="inputFilePath">Path to the input video file to be split.</param>
    /// <param name="outputFilePath">Path to the output file, where the split video to be saved.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task<TimeSpan> SplitAtAsync(
        string inputFilePath,
        string outputFilePath,
        TimeSpan startPoint,
        TimeSpan videoLength,
        CancellationToken token = default)
    {
        var inputVideoInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);

        if (inputVideoInfo.Duration <= startPoint)
        {
            throw new Exception($"Cannot start splitting from {startPoint} as the video duration is {inputVideoInfo.Duration}");
        }

        var conversion = FFmpeg.Conversions.New()
            .AddParameter($"-ss {startPoint} -i \"{inputFilePath}\" -t {videoLength} -c copy \"{outputFilePath}\"")
            .AddParameter("-r 29.97");
        conversion.OnProgress += Conversion_OnProgress;
        var result = await conversion.Start(token);

		_logger.LogInformation("Splitting of the video took {Duration}.", result.Duration);

		return result.Duration;
    }

    /// <summary>
    /// Method for splitting video to equal pieces not depending on the video length
    /// </summary>
    /// <param name="videoCount">Pieces of videos count.</param>
    /// <param name="inputFilePath">Path to the input video file to be split.</param>
    /// <param name="outputFolderPath">Path to the output folder, where split video pieces to be saved.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task<(string[] Videos, TimeSpan Duration)> SplitEqualAsync(
        TimeSpan videoLength,
        string inputFilePath,
        string outputFolderPath,
        CancellationToken token = default)
    {
        var inputVideoInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);
        var totalDuration = TimeSpan.Zero;

        int videoCount = (int)Math.Round(inputVideoInfo.Duration.TotalSeconds / videoLength.TotalSeconds);

        var existingFiles = Enumerable.Range(0, videoCount)
            .Select(i =>
            {
                var path = Path.Combine(
                    outputFolderPath,
                    $"{Path.GetFileNameWithoutExtension(inputFilePath)}{i}{Path.GetExtension(inputFilePath)}");

                var exists = File.Exists(path);

                return exists ? path : null;
            })
            .Where(x => x is not null)
            .ToHashSet();

		var i = existingFiles.Count;
		var start = videoLength * i;

		var resultVideos = new HashSet<string>(existingFiles);
        
        _logger.LogInformation("Splitting of the video {inputFilePath} started.", inputFilePath);

		for (; start < inputVideoInfo.Duration && i < videoCount; start = start.Add(videoLength), i++)
		{
			_logger.LogInformation("Splitting {I}/{Count}", i, videoCount);

			var outputFile = Path.Combine(
				outputFolderPath,
				$"{Path.GetFileNameWithoutExtension(inputFilePath)}{i}{Path.GetExtension(inputFilePath)}");

			try
			{
				var conversion = await FFmpeg.Conversions.FromSnippet.Split(
					inputFilePath,
					outputFile,
					start,
					videoLength);

				conversion.OnProgress += Conversion_OnProgress;

				var result = await conversion
#if DEBUG
                    .UseHardwareAcceleration("cuda", "h264_cuvid", "h264_nvenc")
#endif
                    .Start(token);

				totalDuration += result.Duration;
				resultVideos.Add(outputFile);

				_logger.LogInformation("Finished splitting {I}/{Count}", i, videoCount);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while splitting video {File} part {Part}", inputFilePath, i);

				if (File.Exists(outputFile))
				{
					try
					{
						File.Delete(outputFile);
					}
					catch (Exception deleteEx)
					{
						_logger.LogWarning(deleteEx, "Failed to delete incomplete output file {File}", outputFile);
					}
				}
			}
		}

		_logger.LogInformation("Splitting of the video took {TotalDuration}", totalDuration);

		return (resultVideos.ToArray(), totalDuration);
    }

    /// <summary>
    /// Method for writing text on video in some position
    /// </summary>
    /// <param name="text">Text to be written.</param>
    /// <param name="inputFilePath">Input video file path.</param>
    /// <param name="outputFilePath">Output video path with the text.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns></returns>
    public async Task<TimeSpan> WriteTextAsync(
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
            throw new Exception("Cannot write text on a file without video stream.");
        }

        var font = "Arial";
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

		_logger.LogInformation("Writing text on video took {Duration}", result.Duration);

		return result.Duration;
    }

    /// <summary>
    /// Method for writing subtitles on videos
    /// </summary>
    /// <param name="inputVideoPath">Path to the input video</param>
    /// <param name="outputVideoPath">Path to the output video with subtitles</param>
    /// <param name="subtitlesPath">Path to the subtitles in .srt format</param>
    /// <returns></returns>
    public async Task<TimeSpan> AddSubtitlesAsync(
        string inputVideoPath,
        string outputVideoPath,
        string subtitlesPath = null,
        string assPath = null,
        CancellationToken token = default)
    {
        var conversion = FFmpeg.Conversions.New()
            .AddParameter($"-i \"{inputVideoPath}\"", ParameterPosition.PreInput);

        if (subtitlesPath == null && assPath == null) {
            throw new InvalidOperationException("No subtitles specified");
        }

        if (subtitlesPath != null)
        {
            conversion = conversion.AddParameter($"-vf \"subtitles=\'{subtitlesPath.Replace("\\", "\\\\").Replace(":", "\\:")}\'\"");
        }

        if (assPath != null)
        {
            conversion = conversion.AddParameter($"-vf \"ass=\'{assPath.Replace("\\", "\\\\").Replace(":", "\\:")}\'\"");
        }

        conversion = conversion
            .AddParameter("-c:a copy")
            .AddParameter("-r 29.97")
            .SetOutput(outputVideoPath)
            .SetOverwriteOutput(true);

        conversion.OnProgress += Conversion_OnProgress;

        var result = await conversion.Start(token);
		_logger.LogInformation("Adding subtitles took {Duration}", result.Duration);

		return result.Duration;
    }

    public async Task<TimeSpan> LoopForAsync(string inputFilePath, string outputFilePath, TimeSpan duration,
        CancellationToken token = default)
    {
        var conversion = FFmpeg.Conversions.New()
            .AddParameter($"-stream_loop -1 -i \"{inputFilePath}\" -t {duration.ToFFmpeg()} -c copy \"{outputFilePath}\"");
        
        conversion.OnProgress += Conversion_OnProgress;
        
        var result = await conversion.Start(token);
        _logger.LogInformation("Looping for {Duration} took {JobDuration} seconds.",  duration, result.Duration);
        
        return result.Duration;
    }

    public async Task<TimeSpan> AddWidgetAsync(string inputFilePath, string widgetFilePath, string outputFilePath, AddWidgetConfig config, CancellationToken token = default)
    {
	    var baseMediaInfo = await FFmpeg.GetMediaInfo(inputFilePath, token);

	    var baseWidth = baseMediaInfo.VideoStreams.First().Width;
	    
	    var widgetInfo = await FFmpeg.GetMediaInfo(widgetFilePath, token);
	    var widgetStream = widgetInfo.VideoStreams.First();
	    
	    var fps = (int)Math.Round(widgetStream.Framerate);
	    var frames = (int)(widgetStream.Duration.TotalSeconds * fps);

	    var startSeconds = config.StartTime;
	    
	    var loopFilter = config.Loop
		    ? $"loop=loop=-1:size={frames},"
		    : "";
	    
	    var widgetDuration = widgetStream.Duration.TotalSeconds;
	    var fadeOutStart = widgetDuration - config.FadeDuration;
	    
	    var fadeInCmd = $"fade=t=in:st=0:d={config.FadeDuration}:alpha=1";
	    var fadeOutCmd = $"fade=t=out:st={fadeOutStart}:d={config.FadeDuration}:alpha=1";

	    var fadeFilters = "";

	    if (config.Loop && config.FadeIn)
	    {
		    fadeFilters = $",{fadeInCmd}";
	    }
	    else if (config.FadeIn && config.FadeOut)
	    {
		    fadeFilters = $",{fadeInCmd},{fadeOutCmd}";
	    }
	    else if (config.FadeIn)
	    {
		    fadeFilters = $",{fadeInCmd}";
	    }
	    else if (config.FadeOut)
	    {
		    fadeFilters = $",{fadeOutCmd}";
	    }
	    
	    string timeShift = $",setpts=PTS-STARTPTS+{startSeconds}/TB";
	    
	    string filter = $@"
		    [1:v]{loopFilter}scale={baseWidth}:-1,
		          colorkey={config.BackgroundColor}:{config.Similarity}:0.2{fadeFilters}{timeShift}[ov];
		    [0:v][ov]overlay=(W-w)/2:100:enable='gte(t,{startSeconds})'[vout]
		";
	    
	    var conversion = FFmpeg.Conversions.New()
		    .AddParameter($"-i \"{inputFilePath}\"", ParameterPosition.PreInput)
		    .AddParameter($"-i \"{widgetFilePath}\"", ParameterPosition.PreInput)
		    .AddParameter($"-filter_complex \"{filter}\"")
		    .AddParameter("-map \"[vout]\"")
		    .AddParameter("-map 0:a?")
		    .SetOutput(outputFilePath);
	    
	    conversion.OnProgress += Conversion_OnProgress;

	    var result = await conversion.Start(token);
	    
	    _logger.LogInformation("Applying widget took {JobDuration} seconds.", result.Duration);
	    
	    return result.Duration;
    }

    private void Conversion_OnProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
    {
        if (lastProgress != 0 && Math.Abs(args.Percent - lastProgress) < 25) return;
        lastProgress = args.Percent;

        _logger.LogInformation($"Generating - Process ({args.ProcessId}) - {args.Percent}%");
    }
}