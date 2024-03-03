using Microsoft.Extensions.Hosting;

namespace VideoGenerator.Workers;

public class PostCreatorWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1);
    }
}