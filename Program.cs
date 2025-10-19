using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoGenerator.Services.Implementations;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using static VideoGenerator.Extensions.Extensions;

namespace TikTokSplitter;

public static class Program
{
    public static async Task Main(string[] args)
    {
#if DEBUG
		Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif

        var host = CreateHostBuilder(args).Build();
        var logger = host.Services.GetRequiredService<ILogger<VideoGenerationService>>();

        logger.LogInformation("Start application...");

        long prevlog = 0;
		await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, new Progress<ProgressInfo>((p) =>
        {
            prevlog = p.DownloadedBytes;

			if (p.DownloadedBytes - prevlog >= 1000000 || prevlog == 0)
			{
				logger.LogInformation("Downloading ffmpeg: {0} / {1} bytes", p.DownloadedBytes, p.TotalBytes);
			}
        }));

		await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostcontext, configBuilder) =>
            {
                configBuilder.AddJsonFile($"appsettings.{hostcontext.HostingEnvironment.EnvironmentName ?? "Development"}.json");
                configBuilder.AddJsonFile("logging.json");
            })
			.ConfigureServices(ConfigureServices);
}