using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoGenerator.Entities;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Workers;

public class InterestingFactsVideoMakerWorker : BackgroundService
{
	private readonly ILogger _logger;
	private readonly ISubtitleGeneratorService _subtitleGeneratorService;
	private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
	private readonly IVideoGenerationService _videoService;

    private readonly TimeSpan JobInterval = TimeSpan.FromMinutes(15);
    private const string GeneratedVideosBucket = "generated-videos";
    private const string Language = "en";

    public InterestingFactsVideoMakerWorker(
		ILogger<InterestingFactsVideoMakerWorker> logger,
		ISubtitleGeneratorService subtitleGeneratorService,
		IDbContextFactory<ApplicationDbContext> dbContextFactory,
		IVideoGenerationService videoGenerationService)
	{
		_logger = logger;
		_subtitleGeneratorService = subtitleGeneratorService;
		_dbContextFactory = dbContextFactory;
		_videoService = videoGenerationService;
    }

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Delay(1, stoppingToken);

		while(!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("{Worker} started.", nameof(InterestingFactsVideoMakerWorker));

            var queueItem = await GetRandomQueueItem(stoppingToken);

            try
			{
				if (queueItem is null)
				{
					throw new Exception("No pending interesting fact texts found in the queue.");
				}

				_logger.LogInformation("Processing interesting fact id: {id}", queueItem.Id);

                var objectName = queueItem.Id + Language;

                await _subtitleGeneratorService.GenerateSubtitlesForInterestingFact(queueItem, stoppingToken);
                var objectNames = await _videoService.CreateInterestingFactVideo(queueItem, stoppingToken);

                await SaveVideoToUploadQueue(queueItem, stoppingToken);
            }
			catch
			{
				_logger.LogError("{Worker} failed to process job.", nameof(InterestingFactsVideoMakerWorker));
            }

            _logger.LogInformation("{Worker} finished.", nameof(InterestingFactsVideoMakerWorker));

            await Task.Delay(JobInterval, stoppingToken);
		}
	}

    private async Task<GenerationQueueItem> GetRandomQueueItem(CancellationToken token = default)
    {
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync(token))
        {
            var queueItem = await dbContext.Set<GenerationQueueItem>()
                .Where(x => x.Status == GenerationStatus.ReadyToProcess 
                    && x.Type == Enums.VideoType.InterestingFact)
                .FirstOrDefaultAsync(token);

            queueItem.Status = GenerationStatus.Processing;

            await dbContext.SaveChangesAsync(token);

            return queueItem;
        }
    }

    private async Task SaveVideoToUploadQueue(
        GenerationQueueItem queueItem,
        CancellationToken token = default)
    {
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync(token))
        {
            dbContext.Set<GeneratedVideo>().Add(new()
            {
                GenerationQueueId = queueItem.Id,
                BlobPath = $"{GeneratedVideosBucket}/{(queueItem.Id + "en")}",
                Type = Enums.VideoType.InterestingFact,
                UploadingStatus = UploadingStatus.NotUploaded,
                PartNumber = 1,
                TotalParts = 1
            });

            dbContext.Attach(queueItem);

            queueItem.Status = GenerationStatus.Processed;

            await dbContext.SaveChangesAsync(token);
        }
    }
}
