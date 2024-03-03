using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VideoGenerator.Workers;

public class VideoMakerWorker : BackgroundService
{
    private readonly ILogger _logger;

    public VideoMakerWorker(ILogger<VideoMakerWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken token = default)
    {
        await Task.Delay(1);
        try
        {
            while (!token.IsCancellationRequested)
            {

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);
        }
    }
}
