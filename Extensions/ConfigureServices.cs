using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Net.Http.Headers;
using TikTokSplitter.Configurations;
using TikTokSplitter.Services.Implementations;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Implementations;
using VideoGenerator.Services.Interfaces;
using VideoGenerator.Workers;
using WTelegram;

namespace TikTokSplitter.Extensions;

public static partial class Extensions
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
            options.UseSqlite("Data Source=videogenerator.db;");
        });
        services.AddHostedService<VideoMakerWorker>();
        AddHttpClients(hostContext, services);
        services.AddHostedService<VideoMakerWorker>();
        services.AddHostedService<TelegramScrapperWorker>();

        services.AddSingleton<ITelegramClient, TelegramClient>();
        services.AddSingleton<Client>(x => new Client(int.Parse(hostContext.Configuration["API_ID"]), hostContext.Configuration["API_HASH"]));
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

    public static void StartTelegramClient(this IHost host)
    {
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var tgService = host.Services.GetRequiredService<ITelegramClient>();
        var logger = host.Services.GetService<ILogger<ITelegramClient>>();

        var client = new WTelegram.Client(int.Parse(configuration["API_ID"]), configuration["API_HASH"]);
        var loginInfo = configuration["PHONE"];

        while (client.User == null)
        {
            switch (client.Login(loginInfo).Result) // returns which config is needed to continue login
            {
                case "verification_code": Console.Write("Code: "); loginInfo = Console.ReadLine(); break;
                default: loginInfo = null; break;
            }
        }

        client.OnUpdate += tgService.ProcessUpdate;
        var me = client.User;
        var chats = client.Messages_GetAllChats().Result;
        if (logger != null)
        {
            var channels = chats.chats
                .Where(x => x.Value.IsChannel == true)
                .Select(x => x.Value.Title)
                .ToList();

            logger.LogInformation("Telegram client {ID}:{Username} logged", me.ID, me.MainUsername);
            logger.LogInformation("Available channels: {@Channels}", channels);
        }
    }
}