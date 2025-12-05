using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoGenerator.Configs;
using VideoGenerator.Entities;
using VideoGenerator.Enums;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Workers;

public class UploadSchedulerWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly UploadConfig _options;

    public UploadSchedulerWorker(ILogger<UploadSchedulerWorker> logger, IDbContextFactory<ApplicationDbContext> contextFactory, IOptions<UploadConfig> options)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _options = options.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken token = default)
    {
        var partsDelay = TimeSpan.FromMinutes(_options.MinutesBetweenParts);
        var videosDelay = TimeSpan.FromMinutes(_options.MinutesBetweenVideos);
        
        await Task.Delay(1, token);

        while (!token.IsCancellationRequested)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync(token);

                var itemsToProcess = await context.Set<PublishBatch>()
                    .Where(x => x.ApproveStatus == ApproveStatus.Approved && !x.IsScheduled)
                    .ToListAsync(token);

                foreach (var item in itemsToProcess)
                {
                    var videos = await context.Set<GeneratedVideo>()
                        .Where(x => x.GenerationQueueId == item.GenerationQueueItemId)
                        .OrderBy(x => x.PartNumber)
                        .ToListAsync(token);

                    if (videos.Count == 0)
                    {
                        _logger.LogWarning("No videos found for batch {BatchId}", item.Id);
                        continue;
                    }
                    
                    var allPlatformTypes = await context.Set<PublishAccount>()
                        .Select(x => x.Type)
                        .Distinct()
                        .ToListAsync(token);

                    if (allPlatformTypes.Count == 0)
                    {
                        _logger.LogWarning("No publish accounts found. Skipping batch {BatchId}", item.Id);
                        continue;
                    }
                    
                    foreach (var platformType in allPlatformTypes)
                    {
                        var account = await context.Set<PublishAccount>()
                            .Where(x => x.Type == platformType && x.ContentType == item.ContentType)
                            .OrderBy(x => x.LastPublishAt)
                            .FirstOrDefaultAsync(token);

                        if (account == null)
                        {
                            _logger.LogWarning("No account found for platform {Platform} with {Type} content. Skipping.", 
                                platformType, 
                                item.ContentType.ToString());
                            continue;
                        }

                        var initialScheduleTime = account.LastPublishAt + videosDelay;
                        var scheduleTime = initialScheduleTime < DateTime.UtcNow 
                            ? DateTime.UtcNow 
                            : initialScheduleTime;

                        var scheduledUploads = new List<ScheduledUpload>();
                        foreach (var video in videos)
                        {
                            scheduledUploads.Add(new ScheduledUpload
                            {
                                GeneratedVideoId = video.Id,
                                ScheduledAt = scheduleTime,
                                PublishQueueItemId = null
                            });
                            scheduleTime += partsDelay;
                        }

                        var queueItem = new PublishQueueItem
                        {
                            PublishAccountId = account.Id,
                            PublishBatchId = item.Id,
                            ScheduledUploads = scheduledUploads
                        };

                        context.Set<PublishQueueItem>().Add(queueItem);
                        
                        account.LastPublishAt = scheduleTime - partsDelay;
                        
                        _logger.LogInformation("Scheduled batch {BatchId} for {Platform} account {AccountName}", 
                            item.Id, platformType, account.Name);
                    }

                    item.IsScheduled = true;
                    await context.SaveChangesAsync(token);
                    
                    _logger.LogInformation("Successfully scheduled batch {BatchId} across {PlatformCount} platforms", 
                        item.Id, allPlatformTypes.Count);
                }
            } 
            catch(Exception ex)
            {
                _logger.LogError(exception: ex, message: $"An error occurred while trying to execute {nameof(UploadSchedulerWorker)} background service : {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), token);
        }
    }
}