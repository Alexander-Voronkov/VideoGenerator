using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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
        string audioPath, 
        string subtitlePath, 
        string bucketName,
        string objectName, 
        CancellationToken token = default)
	{
		// Get audio duration
		var mediaInfo = await FFmpeg.GetMediaInfo(audioPath);

		var selectedVideos = new List<SplitHistory>();

		var dbContext = _dbContextFactory.CreateDbContext();

		var availableMusic = await _minioBlobService.ListAsync("background-music", token);
		var backgroundMusic = "background-music/" + availableMusic.OrderBy(x => Random.Shared.Next()).First();

		for (double i = 0; i < mediaInfo.Duration.TotalMinutes;)
		{
			var video = await dbContext.Set<SplitHistory>()
				.OrderBy(x => x.LastTookPartAt)
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

		var tempBackgroundVideoPath = Path.Combine(Path.GetTempPath(), $"merged_{objectName}");
        var tempTrimmedBackgroundVideoPath = Path.Combine(Path.GetTempPath(), $"merged_splitted_{objectName}");
        var tempLoopedMusic = Path.Combine(Path.GetTempPath(), $"music_{objectName}");
        var tempVideoWithMusic = Path.Combine(Path.GetTempPath(), $"merged_with_music_{objectName}");
        var tempVideoWithNarration = Path.Combine(Path.GetTempPath(), $"merged_with_sound_{objectName}");
        var tempVideoWithSubtitles = Path.Combine(Path.GetTempPath(), $"merged_with_subtitles_{objectName}");

		if (!File.Exists(tempBackgroundVideoPath))
		{
			_logger.LogInformation("Started merging");
			if (selectedVideos.Count == 1)
			{
				tempBackgroundVideoPath = $"http://{_minioBlobConfig.Host}/{selectedVideos.Single().BlobPath}";
			}
			else
			{
				await _videoService.MergeVideosAsync(
					selectedVideos.Select(x => $"http://{_minioBlobConfig.Host}/{x.BlobPath}").ToArray(),
					tempBackgroundVideoPath,
					token);
			}
		}

        var tempMergedVideoMediaInfo = await FFmpeg.GetMediaInfo(tempBackgroundVideoPath);

		if (!File.Exists(tempTrimmedBackgroundVideoPath))
		{
			_logger.LogInformation("Started splitting");
			await _videoService.SplitAtAsync(tempBackgroundVideoPath, tempTrimmedBackgroundVideoPath, TimeSpan.Zero, mediaInfo.Duration, token);
		}

		if (!File.Exists(tempLoopedMusic))
		{
			_logger.LogInformation("Started looping music for video");
			await _videoService.LoopForAsync($"http://{_minioBlobConfig.Host}/{backgroundMusic}", tempLoopedMusic, mediaInfo.Duration, token);
		}

		if (!File.Exists(tempVideoWithMusic))
		{
			_logger.LogInformation("Started attaching music");
			await _videoService.AttachAudioAsync(
				tempLoopedMusic, 
				tempTrimmedBackgroundVideoPath, 
				tempVideoWithMusic, 
				volume: 0.1F, 
				overrideOriginalAudio: true, 
				token: token);
		}

		if (!File.Exists(tempVideoWithNarration))
		{
			_logger.LogInformation("Started attaching audio");
			await _videoService.AttachAudioAsync(audioPath, tempVideoWithMusic, tempVideoWithNarration, token: token);
		}

		if (!File.Exists(tempVideoWithSubtitles))
		{
			_logger.LogInformation("Started adding subtitles");
			using var http = new HttpClient();
			var data = await http.GetByteArrayAsync(subtitlePath);
			var tempSubtitlePath = Path.Combine(Path.GetTempPath(), $"subtitles_{Path.GetFileNameWithoutExtension(objectName)}.ass");
			await File.WriteAllBytesAsync(tempSubtitlePath, data, token);

			await _videoService.AddSubtitlesAsync(tempVideoWithNarration, tempVideoWithSubtitles, assPath: tempSubtitlePath, token: token);

			File.Delete(tempSubtitlePath);
		}

		_logger.LogInformation("Started uploading video");

		var videoExists = await _minioBlobService.ExistsAsync(bucketName, objectName, token);

		if (!videoExists)
		{
			await using var stream = File.OpenRead(tempVideoWithSubtitles);
			await _minioBlobService.UploadAsync(
				bucketName,
				objectName,
				stream,
				"video/mp4",
				token);
		}
		
		TryDelete(tempBackgroundVideoPath);
		TryDelete(tempTrimmedBackgroundVideoPath);
		TryDelete(tempVideoWithNarration);
		TryDelete(tempVideoWithSubtitles);
		TryDelete(tempLoopedMusic);
		TryDelete(tempVideoWithMusic);
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
