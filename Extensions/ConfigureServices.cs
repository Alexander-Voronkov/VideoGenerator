using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Net.Http.Headers;
using TikTokSplitter.Configurations;
using TikTokSplitter.Services.Implementations;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Implementations;
using VideoGenerator.Services.Interfaces;
using VideoGenerator.Workers;

namespace TikTokSplitter.Extensions;

public static class Extensions
{
    public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddSingleton<ICancellationTokenHandlerService, CancellationTokenHandlerService>();
        services.AddTransient(typeof(CancellationToken), sp => sp.GetRequiredService<ICancellationTokenHandlerService>().Token);
        services.AddSerilog(logger => logger.ReadFrom.Configuration(hostContext.Configuration));
        services.Configure<Configuration>(hostContext.Configuration);
        services.AddMemoryCache();
        services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
        {
            options.UseMemoryCache(sp.GetRequiredService<IMemoryCache>());
        });
        services.AddHostedService<VideoMakerWorker>();
        AddHttpClients(hostContext, services);
    }

    private static void AddHttpClients(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddHttpClient<VideoDownloaderService>();
        services.AddHttpClient<YoutubeMetadataScraperService>(client =>
        {
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", hostContext.Configuration.GetValue<string>("YOUTUBE_DOWNLOAD_API_KEY"));
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "ytstream-download-youtube-videos.p.rapidapi.com");
        });
        services.AddHttpClient<TiktokMetadataScraperService>(client =>
        {
            client.BaseAddress = new Uri("https://tiktok-no-watermark-video1.p.rapidapi.com/video_info/");
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", hostContext.Configuration.GetValue<string>("TIKTOK_DOWNLOAD_API_KEY"));
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "tiktok-no-watermark-video1.p.rapidapi.com");
        });
        services.AddHttpClient<TiktokUploadVideoService>(client =>
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hostContext.Configuration.GetValue<string>("TIKTOK_DATA_API_KEY"));
        });
        services.AddHttpClient<YoutubeUploadVideoService>(client =>
        {
            client.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hostContext.Configuration.GetValue<string>("YOUTUBE_DATA_API_KEY"));
        });
    }
}