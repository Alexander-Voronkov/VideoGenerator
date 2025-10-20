using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoGenerator.Configs;
using VideoGenerator.Entities;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Interfaces;
using Xabe.FFmpeg;

namespace VideoGenerator.Services.Implementations;

public class VideoGenerationService : IVideoGenerationService
{
    private readonly IVideoProcessingService _videoService;
    private readonly IMinioBlobService _minioBlobService;
	private readonly MinioBlobConfig _minioBlobConfig;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
	private readonly ILogger _logger;

	private const string BackgroundMusicBucket = "background-music";
	private const string SplittedVideosBucket = "splitted-brainrot";
	private const string AssSubtitlesBucket = "ass-subtitles";
	private const string TtsSubtitlesBucket = "tts-subtitles";
	private const string VideosBucket = "generated-videos";


	public VideoGenerationService(
        IVideoProcessingService videoService,
        IMinioBlobService minioBlobService,
		IOptions<MinioBlobConfig> minioBlobConfig,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
		ILogger<VideoGenerationService> logger)
    {
        _videoService = videoService;
        _minioBlobService = minioBlobService;
        _minioBlobConfig = minioBlobConfig.Value;
		_dbContextFactory = dbContextFactory;
		_logger = logger;
	}

	public async Task CreateVideo(
        string objectName, 
        CancellationToken token = default)
	{
		// Get audio duration
		var tempAudioPath = Path.Combine(Path.GetTempPath(), $"tts-subtitles-{objectName}.mp3");

		await using (var str = File.Open(tempAudioPath, FileMode.OpenOrCreate))
		{
			await _minioBlobService.DownloadAsync(TtsSubtitlesBucket, objectName, str, token);
		}

		var mediaInfo = await FFmpeg.GetMediaInfo(tempAudioPath);

		var selectedVideos = new List<SplitHistory>();

		var dbContext = _dbContextFactory.CreateDbContext();

		for (double i = 0; i < mediaInfo.Duration.TotalMinutes;)
		{
			var video = await dbContext.Set<SplitHistory>()
				.OrderBy(x => x.LastTookPartAt == null)
				.ThenBy(x => x.LastTookPartAt)
				.ThenBy(x => x.Duration)
				.FirstOrDefaultAsync(x => x.Duration.TotalMinutes <= mediaInfo.Duration.TotalMinutes - i 
                    || !dbContext.Set<SplitHistory>().Any(q => q.Duration.TotalMinutes <= mediaInfo.Duration.TotalMinutes - i), token);

			await dbContext.Set<SplitHistory>()
				.Where(x => x.Id == video.Id)
				.ExecuteUpdateAsync(x => x.SetProperty(q => q.LastTookPartAt, DateTime.UtcNow), token);

			if (video != null)
			{
				selectedVideos.Add(video);
                i += video.Duration.TotalMinutes;
			}
		}

		dbContext.Dispose();

		var tempBackgroundVideoPath = Path.Combine(Path.GetTempPath(), $"merged_{objectName}.mp4");
        var tempTrimmedBackgroundVideoPath = Path.Combine(Path.GetTempPath(), $"merged_splitted_{objectName}.mp4");
        var tempLoopedMusic = Path.Combine(Path.GetTempPath(), $"music_{objectName}.mp3");
        var tempVideoWithMusic = Path.Combine(Path.GetTempPath(), $"merged_with_music_{objectName}.mp4");
        var tempVideoWithNarration = Path.Combine(Path.GetTempPath(), $"merged_with_sound_{objectName}.mp4");
        var tempVideoWithSubtitles = Path.Combine(Path.GetTempPath(), $"merged_with_subtitles_{objectName}.mp4");

		if (!File.Exists(tempBackgroundVideoPath))
		{
			_logger.LogInformation("VideoGeneration: Started merging");
			if (selectedVideos.Count == 1)
			{
				await using (var stream = File.Open(tempBackgroundVideoPath, FileMode.OpenOrCreate))
				{
					await _minioBlobService.DownloadAsync(
						SplittedVideosBucket,
						Path.GetFileName(selectedVideos.Single().BlobPath),
						stream,
						token);
				}
			}
			else
			{
				List<string> tempSelectedVideos = [];

				foreach (var selectedVideo in selectedVideos)
				{
					var path = Path.Combine(Path.GetTempPath(), $"tempsplitted_{Path.GetFileName(selectedVideo.BlobPath)}");

					await using (var stream = File.Open(path, FileMode.OpenOrCreate))
					{
						await _minioBlobService.DownloadAsync(
							SplittedVideosBucket,
							Path.GetFileName(selectedVideo.BlobPath),
							stream,
							token);
					}

					tempSelectedVideos.Add(path);
				}

				await _videoService.MergeVideosAsync(
					tempSelectedVideos.ToArray(),
					tempBackgroundVideoPath,
					token);

				foreach (var tempPath in tempSelectedVideos)
				{
					TryDelete(tempPath);
				}
			}
			_logger.LogInformation("VideoGeneration: End merging");
		}

        var tempMergedVideoMediaInfo = await FFmpeg.GetMediaInfo(tempBackgroundVideoPath);

		if (!File.Exists(tempTrimmedBackgroundVideoPath))
		{
			_logger.LogInformation("Started splitting");
			await _videoService.SplitAtAsync(tempBackgroundVideoPath, tempTrimmedBackgroundVideoPath, TimeSpan.Zero, mediaInfo.Duration, token);
		}

		if (!File.Exists(tempLoopedMusic))
		{ 
			_logger.LogInformation($"{nameof(VideoGenerationService)}: start searching for available background music.");

			var availableMusic = await _minioBlobService.ListAsync(BackgroundMusicBucket, token);
			var tempBackgroundMusicPath = Path.Combine(Path.GetTempPath(), $"background-music-temp-{objectName}.mp3");

			await using (var str = File.Open(tempBackgroundMusicPath, FileMode.OpenOrCreate))
			{
				await _minioBlobService.DownloadAsync(BackgroundMusicBucket, availableMusic.OrderBy(x => Random.Shared.Next()).First(), str, token);
			}

			_logger.LogInformation($"{nameof(VideoGenerationService)}: end searching for available background music.");

			_logger.LogInformation("VideoGeneration: Started looping music for video");

			await _videoService.LoopForAsync(tempBackgroundMusicPath, tempLoopedMusic, mediaInfo.Duration, token);

			TryDelete(tempBackgroundMusicPath);
			_logger.LogInformation("VideoGeneration: End looping music for video");
		}

		if (!File.Exists(tempVideoWithMusic))
		{
			_logger.LogInformation("VideoGeneration: Started attach music");

			await _videoService.AttachAudioAsync(
				tempLoopedMusic, 
				tempTrimmedBackgroundVideoPath, 
				tempVideoWithMusic, 
				volume: 0.01F, 
				overrideOriginalAudio: true, 
				token: token);

			_logger.LogInformation("VideoGeneration: End attach music");
		}

		if (!File.Exists(tempVideoWithNarration))
		{
			_logger.LogInformation("VideoGeneration: Started attach audio");
			await _videoService.AttachAudioAsync(tempAudioPath, tempVideoWithMusic, tempVideoWithNarration, token: token);
			_logger.LogInformation("VideoGeneration: End attach audio");
		}

		if (!File.Exists(tempVideoWithSubtitles))
		{
			_logger.LogInformation("VideoGeneration: Started add subtitles");
			var tempSubtitlePath = Path.Combine(Path.GetTempPath(), $"subtitles_{Path.GetFileNameWithoutExtension(objectName)}.ass");
			await using (var str = File.Open(tempSubtitlePath, FileMode.OpenOrCreate))
			{
				await _minioBlobService.DownloadAsync(AssSubtitlesBucket, objectName, str, token);
			}

			await _videoService.AddSubtitlesAsync(tempVideoWithNarration, tempVideoWithSubtitles, assPath: tempSubtitlePath, token: token);

			TryDelete(tempSubtitlePath);
			_logger.LogInformation("VideoGeneration: End add subtitles");
		}

		_logger.LogInformation("Started uploading video");

		var videoExists = await _minioBlobService.ExistsAsync(VideosBucket, objectName, token);

		if (!videoExists)
		{
			await using (var stream = File.OpenRead(tempVideoWithSubtitles)) {
				await _minioBlobService.UploadAsync(
				VideosBucket,
				objectName,
				stream,
				"video/mp4",
				token);
			}
		}
		
		TryDelete(tempBackgroundVideoPath);
		TryDelete(tempTrimmedBackgroundVideoPath);
		TryDelete(tempVideoWithNarration);
		TryDelete(tempVideoWithSubtitles);
		TryDelete(tempLoopedMusic);
		TryDelete(tempVideoWithMusic);
		TryDelete(tempAudioPath);

		_logger.LogInformation("End uploading video");
	}

	private void TryDelete(string path)
	{
		try
		{
			File.Delete(path);
		}
		catch (Exception ex)
		{
			_logger.LogError("Failed to clear temp file: {Message}", ex.Message);
		}
	}
}
