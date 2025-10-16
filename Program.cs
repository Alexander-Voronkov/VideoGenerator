using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using static VideoGenerator.Extensions.Extensions;

namespace TikTokSplitter;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostcontext, configBuilder) =>
            {
                configBuilder.AddJsonFile("appsettings.json");
                configBuilder.AddJsonFile("logging.json");
            })
			.ConfigureServices(ConfigureServices);
}