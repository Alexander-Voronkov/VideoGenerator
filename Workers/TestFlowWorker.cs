using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Workers;

public class TestFlowWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IVideoProcessingService _videoService;

    public TestFlowWorker(ILogger<TestFlowWorker> logger, IVideoProcessingService videoService)
    {
        _logger = logger;
        _videoService = videoService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken token = default)
    {
        await Task.Delay(1, token);

        while (!token.IsCancellationRequested)
        {
            try
            {
               var widget = "\"C:\\Users\\oleks\\OneDrive\\Desktop\\cooking10.mp4\"";
               var video = "\"C:\\Users\\oleks\\OneDrive\\Desktop\\subscribe.mp4\"";
               
               var output = "\"C:\\Users\\oleks\\OneDrive\\Desktop\\test11.mp4\"";
               
               await _videoService.AddWidgetAsync(widget, video, output, new AddWidgetConfig { BackgroundColor = "0x000000", Similarity = 0.01f, StartTime = 5}, token);
            } 
            catch(Exception ex)
            {
                _logger.LogError(exception: ex, message: $"An error occurred while trying to execute {nameof(VideoMakerWorker)} background service : {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), token);
        }
    }
}