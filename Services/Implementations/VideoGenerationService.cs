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

		for (double i = 0; i < mediaInfo.Duration.TotalMinutes;)
		{
			var video = await dbContext.Set<SplitHistory>()
				.OrderByDescending(x => x.LastTookPartAt)
				.OrderByDescending(x => x.Duration)
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

		var tempMergedVideoPath = Path.Combine(Path.GetTempPath(), $"merged_{objectName}");
        var tempMergedVideoPath1 = Path.Combine(Path.GetTempPath(), $"merged_splitted_{objectName}");
        var tempMergedVideoPath2 = Path.Combine(Path.GetTempPath(), $"merged_with_sound_{objectName}");
        var tempMergedVideoPath3 = Path.Combine(Path.GetTempPath(), $"merged_with_subtitles_{objectName}");

		if (!File.Exists(tempMergedVideoPath))
		{
			_logger.LogInformation("Started merging");
			if (selectedVideos.Count == 1)
			{
				tempMergedVideoPath = $"http://{_minioBlobConfig.Host}/{selectedVideos.Single().BlobPath}";
			}
			else
			{
				await _videoService.MergeVideosAsync(
					selectedVideos.Select(x => $"http://{_minioBlobConfig.Host}/{x.BlobPath}").ToArray(),
					tempMergedVideoPath,
					token);
			}
		}

        var tempMergedVideoMediaInfo = await FFmpeg.GetMediaInfo(tempMergedVideoPath);

		if (!File.Exists(tempMergedVideoPath1))
		{
			_logger.LogInformation("Started splitting");
			await _videoService.SplitAtAsync(tempMergedVideoPath, tempMergedVideoPath1, TimeSpan.Zero, mediaInfo.Duration, token);
		}

		if (!File.Exists(tempMergedVideoPath2))
		{
			_logger.LogInformation("Started attaching audio");
			await _videoService.AttachAudioAsync(audioPath, tempMergedVideoPath1, tempMergedVideoPath2, token);
		}

		if (!File.Exists(tempMergedVideoPath3))
		{
			_logger.LogInformation("Started adding subtitles");
			using var http = new HttpClient();
			var data = await http.GetByteArrayAsync(subtitlePath);
			var tempSubtitlePath = Path.Combine(Path.GetTempPath(), $"subtitles_{Path.GetFileNameWithoutExtension(objectName)}.ass");
			await File.WriteAllBytesAsync(tempSubtitlePath, data, token);

			await _videoService.AddSubtitlesAsync(tempMergedVideoPath2, tempMergedVideoPath3, assPath: tempSubtitlePath, token: token);

			File.Delete(tempSubtitlePath);
		}

		_logger.LogInformation("Started uploading video");

		var videoExists = await _minioBlobService.ExistsAsync(bucketName, objectName, token);

		if (!videoExists)
		{
			await using var stream = File.OpenRead(tempMergedVideoPath3);
			await _minioBlobService.UploadAsync(
				bucketName,
				objectName,
				stream,
				"video/mp4",
				token);
		}
		
		TryDelete(tempMergedVideoPath);
		TryDelete(tempMergedVideoPath1);
		TryDelete(tempMergedVideoPath2);
		TryDelete(tempMergedVideoPath3);
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
