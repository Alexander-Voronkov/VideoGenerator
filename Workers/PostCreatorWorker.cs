using Microsoft.Extensions.Hosting;

namespace VideoGenerator.Workers;

public class PostCreatorWorker : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}