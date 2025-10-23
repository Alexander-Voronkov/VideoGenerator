using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using System.Diagnostics;
using VideoGenerator.Configs;
using VideoGenerator.Entities;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Interfaces;
using Xabe.FFmpeg;
using File = System.IO.File;

namespace VideoGenerator.Services.Implementations;
public class VideoGenerationService : IVideoGenerationService
{
    private readonly IVideoProcessingService _videoService;
    private readonly IMinioBlobService _minioBlobService;
	private readonly MinioBlobConfig _minioBlobConfig;
	private readonly RedditStoryConfig _redditStoryConfig;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IAssConvertService  _assService;
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
        IOptions<RedditStoryConfig> redditStoryConfig,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
		ILogger<VideoGenerationService> logger,
        IAssConvertService assService)
    {
        _videoService = videoService;
        _minioBlobService = minioBlobService;
        _minioBlobConfig = minioBlobConfig.Value;
		_redditStoryConfig = redditStoryConfig.Value;
		_dbContextFactory = dbContextFactory;
		_assService = assService;
		_logger = logger;
    }

	public async Task<string[]> CreateVideo(
        string objectName,
        string title,
        CancellationToken token = default)
	{
		var (tempAudioPath, mediaInfo) = await DownloadNarration(objectName, token);

		var tempTrimmedBackgroundVideoPath = await PrepareBackgroundVideo(objectName, mediaInfo.Duration, token);
		var tempVideoWithMusic = await PrepareBackgroundMusic(objectName, tempTrimmedBackgroundVideoPath, mediaInfo.Duration, token);
        var tempVideoWithNarration = await AttachNarration(objectName, tempVideoWithMusic, tempAudioPath, token);
        var tempVideoWithSubtitles = await AttachSubtitles(objectName, tempVideoWithNarration, mediaInfo.Duration, token);
		
		var generatedVideoMediaInfo = await FFmpeg.GetMediaInfo(tempVideoWithSubtitles, token);
		var (partsCount, partLength) = CalculatePartsCount(generatedVideoMediaInfo.Duration, _redditStoryConfig.TargetVideoLengthInSeconds);
		
        string[] temp = [tempTrimmedBackgroundVideoPath, tempVideoWithMusic, tempVideoWithNarration, tempVideoWithSubtitles];
        var objectNames = await SplitAndUpload(objectName, tempVideoWithSubtitles, title, partsCount, partLength, token);
		
		TryDelete(temp);
		
		return objectNames.ToArray();
	}

	private async Task<(string audioPath, IMediaInfo mediaInfo)> DownloadNarration(string objectName, CancellationToken token = default)
	{
		var tempAudioPath = Path.Combine(Path.GetTempPath(), $"tts-subtitles-{objectName}.mp3");

		await using var str = File.Open(tempAudioPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
		await _minioBlobService.DownloadAsync(TtsSubtitlesBucket, objectName, str, token);
		
		
		return (tempAudioPath, await FFmpeg.GetMediaInfo(tempAudioPath, token));
	}

	private async Task<string[]> SplitAndUpload(string objectName, string videoPath, string title, int partsCount, double partLength, CancellationToken token = default)
	{
		List<string> objectNames = [];
		_logger.LogInformation("Started uploading video");
		if (partsCount > 1)
		{
			_logger.LogInformation("Splitting video to {Parts} of {Duration} seconds",  partsCount, partLength);
			
			var (videos,_) = await _videoService.SplitEqualAsync(
				TimeSpan.FromSeconds(partLength), 
				videoPath, 
				Path.GetTempPath(), 
				token);

			var i = 1;
			foreach (var part in videos)
			{
				var partObjectName = objectName + "_" + i;
				
				var path = await AttachTitle(partObjectName, part, title, i, partsCount, token );
				await UploadVideo(partObjectName, path, token);
				
				objectNames.Add(partObjectName);
				i++;
				
				TryDelete(part, path);
			}
		}
		else
		{
			objectNames = [objectName];
			await UploadVideo(objectName, videoPath, token);
		}
		
		_logger.LogInformation("End uploading video");
		return objectNames.ToArray();
	}

	private async Task<string> AttachTitle(string objectName, string videoPath, string title, int part, int partsCount,
		CancellationToken token = default)
	{
		var mediaInfo = await FFmpeg.GetMediaInfo(videoPath, token);
		
		var tempPath = Path.Combine(Path.GetTempPath(), $"entitled-{objectName}.mp4");
		var tempTextPath = Path.Combine(Path.GetTempPath(), $"title-{objectName}.ass");
		
		var line = new TimestampedLine($"{title}" + (partsCount > 1 ? $" Part {part}/{partsCount}" : ""), TimeSpan.Zero, mediaInfo.Duration);
		var subs = _assService.GenerateFromTimestampedLines([line], position: 8, fontSize: 60);
		await File.WriteAllTextAsync(tempTextPath, subs, token);
		
		await _videoService.AddSubtitlesAsync(videoPath, tempPath, null, tempTextPath, token);
		
		TryDelete(tempTextPath);
		
		return tempPath;
	}

	private async Task UploadVideo(string objectName, string videoPath, CancellationToken token = default)
	{
		var videoExists = await _minioBlobService.ExistsAsync(VideosBucket, objectName, token);

		if (!videoExists)
		{
			await using var stream = File.OpenRead(videoPath);
			await _minioBlobService.UploadAsync(
				VideosBucket,
				objectName,
				stream,
				"video/mp4",
				token);
		}
	}

	private async Task<string> AttachSubtitles(string objectName, string backgroundVideoPath, TimeSpan duration, CancellationToken token)
	{
		var tempVideoWithSubtitles = Path.Combine(Path.GetTempPath(), $"merged_with_subtitles_{objectName}.mp4");
		if (!File.Exists(tempVideoWithSubtitles))
		{
			_logger.LogInformation("VideoGeneration: Started add subtitles");
			var tempSubtitlePath = Path.Combine(Path.GetTempPath(), $"subtitles_{Path.GetFileNameWithoutExtension(objectName)}.ass");
			await using (var str = File.Open(tempSubtitlePath, FileMode.OpenOrCreate))
			{
				await _minioBlobService.DownloadAsync(AssSubtitlesBucket, objectName, str, token);
			}

			await _videoService.AddSubtitlesAsync(backgroundVideoPath, tempVideoWithSubtitles, assPath: tempSubtitlePath, token: token);

			TryDelete(tempSubtitlePath);
			_logger.LogInformation("VideoGeneration: End add subtitles");
		}
		
		return tempVideoWithSubtitles;
	}

	private async Task<string> AttachNarration(string objectName, string backgroundVideoPath, string narrationPath, CancellationToken token)
	{
		var tempVideoWithNarration = Path.Combine(Path.GetTempPath(), $"merged_with_sound_{objectName}.mp4");
		if (!File.Exists(tempVideoWithNarration))
		{
			_logger.LogInformation("VideoGeneration: Started attach audio");
			await _videoService.AttachAudioAsync(narrationPath, backgroundVideoPath, tempVideoWithNarration, token: token);
			_logger.LogInformation("VideoGeneration: End attach audio");
		}
		
		return tempVideoWithNarration;
	}

	private async Task<string> PrepareBackgroundMusic(string objectName, string backgroundVideoPath,  TimeSpan duration, CancellationToken token)
	{
		var loopedMusicPath = Path.Combine(Path.GetTempPath(), $"music_{objectName}.mp3");
		var videoWithMusic = Path.Combine(Path.GetTempPath(), $"merged_with_music_{objectName}.mp4");
		
		if (!File.Exists(loopedMusicPath))
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

			await _videoService.LoopForAsync(tempBackgroundMusicPath, loopedMusicPath, duration, token);

			TryDelete(tempBackgroundMusicPath);
			_logger.LogInformation("VideoGeneration: End looping music for video");
		}
		
		if (!File.Exists(videoWithMusic))
		{
			_logger.LogInformation("VideoGeneration: Started attach music");

			await _videoService.AttachAudioAsync(
				loopedMusicPath, 
				backgroundVideoPath, 
				videoWithMusic, 
				volume: 0.15F, 
				overrideOriginalAudio: true, 
				token: token);

			_logger.LogInformation("VideoGeneration: End attach music");
		}
		
		return videoWithMusic;
	}

	private async Task<string> PrepareBackgroundVideo(string objectName, TimeSpan duration, CancellationToken token)
	{
		var backgroundVideoPath = Path.Combine(Path.GetTempPath(), $"merged_{objectName}.mp4");
		var tempTrimmedBackgroundVideoPath = Path.Combine(Path.GetTempPath(), $"merged_splitted_{objectName}.mp4");
		
		await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
		var selectedVideos = new List<SplitHistory>();

		for (double i = 0; i < duration.TotalMinutes;)
		{
			var video = await dbContext.Set<SplitHistory>()
				.OrderBy(x => x.LastTookPartAt != null)
				.ThenBy(x => x.LastTookPartAt)
				.ThenBy(x => x.Duration)
				.FirstOrDefaultAsync(x => x.Duration.TotalMinutes <= duration.TotalMinutes - i 
				                          || !dbContext.Set<SplitHistory>().Any(q => q.Duration.TotalMinutes <= duration.TotalMinutes - i), token);

			await dbContext.Set<SplitHistory>()
				.Where(x => x.Id == video.Id)
				.ExecuteUpdateAsync(x => x.SetProperty(q => q.LastTookPartAt, DateTime.UtcNow), token);

			if (video == null)
			{
				continue;
			}
			
			selectedVideos.Add(video);
			i += video.Duration.TotalMinutes;
		}
		
		if (!File.Exists(backgroundVideoPath))
		{
			_logger.LogInformation("VideoGeneration: Started merging");
			if (selectedVideos.Count == 1)
			{
				await using var stream = File.Open(backgroundVideoPath, FileMode.OpenOrCreate);
				
				await _minioBlobService.DownloadAsync(
					SplittedVideosBucket,
					Path.GetFileName(selectedVideos.Single().BlobPath),
					stream,
					token);
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
					backgroundVideoPath,
					token);

				foreach (var tempPath in tempSelectedVideos)
				{
					TryDelete(tempPath);
				}
			}
			_logger.LogInformation("VideoGeneration: End merging");
			
			if (!File.Exists(tempTrimmedBackgroundVideoPath))
			{
				_logger.LogInformation("Started splitting");
				await _videoService.SplitAtAsync(backgroundVideoPath, tempTrimmedBackgroundVideoPath, TimeSpan.Zero, duration, token);
			}
			
			TryDelete(backgroundVideoPath);
		}
		
		return tempTrimmedBackgroundVideoPath;
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

	private void TryDelete(params string[] paths)
	{
		foreach (var path in paths)
		{
			TryDelete(path);
		}
	}

	private static (int partsCount, double partLength) CalculatePartsCount(TimeSpan videoDuration, double targetLengthInSeconds)
	{
		var videoDurationInSeconds = videoDuration.TotalSeconds;
		var partsCount = (int)Math.Ceiling(videoDurationInSeconds / targetLengthInSeconds);
		
		double partLength = videoDurationInSeconds / partsCount;
		
		if (partLength < targetLengthInSeconds)
		{
			partsCount--;
			partLength = videoDurationInSeconds / partsCount;
		}

		return (partsCount, partLength);
	}
}
