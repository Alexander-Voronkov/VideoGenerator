using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VideoGenerator.Services.Interfaces;
using static VideoGenerator.Extensions.Extensions;

namespace TikTokSplitter;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var tokenHandler = host.Services.GetRequiredService<ICancellationTokenHandlerService>();
        await host.RunAsync(tokenHandler.Token);
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