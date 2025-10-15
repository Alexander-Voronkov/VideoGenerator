using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Workers;

public class VideoSplitterWorker : BackgroundService
{
    private readonly IMinioBlobService _minioBlobService;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly ILogger _logger;

    private const string RawSourceVideoBucket = "brainrot";
    private const string SplittedVideosBucket = "splitted-brainrot";

    public VideoSplitterWorker(
        IMinioBlobService minioBlobService, 
        IVideoProcessingService videoProcessingService,
        ILogger<VideoSplitterWorker> logger)
    {
        _minioBlobService = minioBlobService;
        _videoProcessingService = videoProcessingService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        await Task.Delay(1, stoppingToken);

        while(!stoppingToken.IsCancellationRequested)
        {
            var videos = _minioBlobService.List(RawSourceVideoBucket, stoppingToken);

            foreach (var video in videos)
            {
                var url = await _minioBlobService.GetPresignedUrlAsync(RawSourceVideoBucket, video, token: stoppingToken);
                var resultVideos = await _videoProcessingService.SplitEqualAsync(TimeSpan.FromMinutes(1), url, "./", stoppingToken);

                foreach (var resultVideo in resultVideos)
                {
                    await _minioBlobService.UploadAsync(SplittedVideosBucket, resultVideo, File.OpenRead(resultVideo), "video/mp4", stoppingToken);
                }

                _logger.LogInformation("Video {0} is split successfully into {1} parts equally.", video, resultVideos.Length);
            }

            await Task.Delay(3000, stoppingToken);
        }
    }
}
