using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Workers;

public class InterestingFactsVideoMakerWorker : BackgroundService
{
	private readonly ILogger _logger;

	private readonly TimeSpan JobInterval = TimeSpan.FromMinutes(15);

	public InterestingFactsVideoMakerWorker(
		ILogger<InterestingFactsVideoMakerWorker> logger)
	{
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Delay(1, stoppingToken);

		while(!stoppingToken.IsCancellationRequested)
		{
			try
			{
				// interesting as fuck
			}
			catch
			{
				_logger.LogError("RedditSubtitlesGeneratorWorker failed to process job.");	
			}

			await Task.Delay(JobInterval, stoppingToken);
		}
	}
}
