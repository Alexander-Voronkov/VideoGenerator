using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TikTokSplitter.Helpers;
using VideoGenerator.Services.Interfaces;
using static TikTokSplitter.Extensions.Extensions;

namespace TikTokSplitter;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        // host.StartTelegramClient();
        var tokenHandler = host.Services.GetRequiredService<ICancellationTokenHandlerService>();
        // await InstallDependenciesHelper.InstallAllDependencies(tokenHandler.Token);
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
    //.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    //.ConfigureContainer<ContainerBuilder>(ConfigureAutofacBuilder)
}