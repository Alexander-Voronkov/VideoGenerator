using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using VideoGenerator.Entities;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Workers;

public class VideoMakerWorker : BackgroundService
{
    private readonly ILogger _logger;
	private readonly IVideoGenerationService _videoService;
	private readonly ISubtitleGeneratorService _subtitleGenerationService;
	private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

	private const int JobIntervalInMinutes = 15;

	private const string GeneratedVideosBucket = "generated-videos";

	private const string Language = "en";

	public VideoMakerWorker(
		ILogger<VideoMakerWorker> logger, 
		IVideoGenerationService videoService, 
		ISubtitleGeneratorService subtitleGeneratorService,
		IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _logger = logger;
        _videoService = videoService;
		_dbContextFactory = dbContextFactory;
		_subtitleGenerationService = subtitleGeneratorService;
    }

    protected override async Task ExecuteAsync(CancellationToken token = default)
    {
        await Task.Delay(1, token);

		while (!token.IsCancellationRequested)
		{
			_logger.LogInformation("{Worker} started.", nameof(VideoMakerWorker));

			var queueItem = await GetRandomQueueItem(token);

			try
			{
				if (queueItem is null)
				{
					throw new Exception("No pending reddit texts found in the queue.");
				}

				_logger.LogInformation("Processing brainrot id: {id}", queueItem.Id);

				var objectName = queueItem.Id + Language;

				await _subtitleGenerationService.GenerateSubtitles(queueItem, token);
				var objectNames = await _videoService.CreateVideo(objectName, queueItem.Title, token);

				await SaveVideosToUploadQueue(objectNames, queueItem, token);
			}
			catch(Exception ex)
			{
				_logger.LogError(exception: ex, message: $"An error occurred while trying to execute {nameof(VideoMakerWorker)} background service : {ex.Message}");

				await RevertQueueItemStatusOnError(queueItem, token);
			}

			await Task.Delay(TimeSpan.FromMinutes(JobIntervalInMinutes), token);
		}
    }

	private async Task<GenerationQueueItem> GetRandomQueueItem(CancellationToken token = default)
	{
		await using (var dbContext = await _dbContextFactory.CreateDbContextAsync(token))
		{
			var queueItem = await dbContext.Set<GenerationQueueItem>()
				.Where(x => x.Status == GenerationStatus.ReadyToProcess)
				.FirstOrDefaultAsync(token);

			queueItem.Status = GenerationStatus.Processing;

			await dbContext.SaveChangesAsync(token);

			return queueItem;
		}
	}

	private async Task SaveVideosToUploadQueue(string[] objectNames, GenerationQueueItem queueItem, CancellationToken token = default)
	{
		await using (var dbContext = await _dbContextFactory.CreateDbContextAsync(token))
		{
			for (int i = 0; i < objectNames.Length; i++)
			{
				dbContext.Set<GeneratedVideo>().Add(new()
				{
					GenerationQueueId = queueItem.Id,
					BlobPath = $"{GeneratedVideosBucket}/{objectNames[i]}",
					UploadingStatus = UploadingStatus.NotUploaded,
					PartNumber = i + 1,
					TotalParts = objectNames.Length
				});
			}

			dbContext.Attach(queueItem);

			queueItem.Status = GenerationStatus.Processed;

			await dbContext.SaveChangesAsync(token);
		}
	}

	private async Task RevertQueueItemStatusOnError(GenerationQueueItem queueItem, CancellationToken token = default)
	{
		await using (var dbContext = await _dbContextFactory.CreateDbContextAsync(token))
		{
			dbContext.Attach(queueItem);
			queueItem.Status = GenerationStatus.ReadyToProcess;
			await dbContext.SaveChangesAsync(token);
		}
	}
}
