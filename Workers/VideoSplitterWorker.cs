using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoGenerator.Configs;
using VideoGenerator.Entities;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Workers;

public class VideoSplitterWorker : BackgroundService
{
    private readonly IMinioBlobService _minioBlobService;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly ILogger _logger;
    private readonly IOptions<MinioBlobConfig> _minioConfig;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    private const string RawSourceVideoBucket = "brainrot";
    private const string SplittedVideosBucket = "splitted-brainrot";

    private const int JobIntervalInMinutes = 10;

    private readonly TimeSpan SplittedVideoDuration = TimeSpan.FromMinutes(1);

	public VideoSplitterWorker(
        IMinioBlobService minioBlobService, 
        IVideoProcessingService videoProcessingService,
        ILogger<VideoSplitterWorker> logger,
        IOptions<MinioBlobConfig> minioConfig,
		IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _minioBlobService = minioBlobService;
        _videoProcessingService = videoProcessingService;
        _logger = logger;
        _minioConfig = minioConfig;
		_dbContextFactory = dbContextFactory;
	}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        await Task.Delay(1, stoppingToken);

		await _minioBlobService.MakeBucketPublicAsync(RawSourceVideoBucket, stoppingToken);
		await _minioBlobService.MakeBucketPublicAsync(SplittedVideosBucket, stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
				var videos = await _minioBlobService.ListAsync(RawSourceVideoBucket, stoppingToken);

                var dbContext = _dbContextFactory.CreateDbContext();

				var rawSplitHistory = await dbContext.Set<SplitHistory>()
                    .Select(x => x.ParentBlobPath)
                    .ToArrayAsync(stoppingToken);

				var splitHistory = rawSplitHistory.ToHashSet();

                dbContext.Dispose();

				foreach (var video in videos.Where(v => !splitHistory.Contains(v)))
				{
                    var url = $"http://{_minioConfig.Value.Host}/{RawSourceVideoBucket}/{video}";
                    var (resultVideos, duration) = await _videoProcessingService.SplitEqualAsync(SplittedVideoDuration, url, ".", stoppingToken);

					dbContext = _dbContextFactory.CreateDbContext();

					foreach (var resultVideo in resultVideos)
					{
						await _minioBlobService.UploadAsync(SplittedVideosBucket, resultVideo, File.OpenRead(resultVideo), "video/mp4", stoppingToken);

						dbContext.Set<SplitHistory>().Add(new SplitHistory
						{
							ParentBlobPath = $"{RawSourceVideoBucket}/{video}",
                            Duration = SplittedVideoDuration,
							BlobPath = $"{SplittedVideosBucket}/{resultVideo}",
                            LastTookPartAt = null,
						});
					}

					await dbContext.SaveChangesAsync(stoppingToken);

					CleanupTemporaryMp4Files();

					_logger.LogInformation("Video {0} is split successfully into {1} parts equally.", video, resultVideos.Length);
				}

                dbContext?.Dispose();
			}
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"An error occurred while trying to execute {nameof(VideoSplitterWorker)} background service: {ex.Message}");
			}
            

            await Task.Delay(TimeSpan.FromMinutes(JobIntervalInMinutes), stoppingToken);
        }
    }

    private static void CleanupTemporaryMp4Files()
    {
		string[] mp4Files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mp4");
		foreach (var file in mp4Files)
		{
			File.Delete(file);
		}
	}
}
