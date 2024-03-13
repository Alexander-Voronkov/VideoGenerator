using Microsoft.Extensions.Hosting;

namespace VideoGenerator.Workers;

public class FilmDownloaderWorker : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}