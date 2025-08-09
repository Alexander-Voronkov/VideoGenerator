using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoGenerator.Helpers;

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
        await InstallDependenciesHelper.InstallAllDependencies(token);
        await Task.Delay(1);
		try
        {
            while (!token.IsCancellationRequested)
            {
				_logger.LogInformation("VideoMakerWorker started successfully");
				_logger.LogError("VideoMakerWorker failed successfully");
				await Task.Delay(1000);
			}
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);
        }
    }
}
