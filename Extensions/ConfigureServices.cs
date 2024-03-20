using DetectLanguage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI_API;
using Serilog;
using System.Net.Http.Headers;
using System.Text;
using TMDbLib.Client;
using VideoGenerator.Configurations;
using VideoGenerator.Infrastructure;
using VideoGenerator.Infrastructure.Interceptors;
using VideoGenerator.Services.Implementations;
using VideoGenerator.Services.Interfaces;
using VideoGenerator.Workers;
using WTelegram;

namespace VideoGenerator.Extensions;

public static partial class Extensions
{
    public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        //configuration
        services.Configure<Configuration>(hostContext.Configuration)

        // logging
        .AddSerilog(logger =>
        {
            Console.OutputEncoding = Encoding.UTF8;
            logger.ReadFrom.Configuration(hostContext.Configuration);
        })

        //singleton services
        .AddSingleton<ICancellationTokenHandlerService, CancellationTokenHandlerService>()
        .AddSingleton<ISaveChangesInterceptor, AuditableEntityInterceptor>()
        .AddSingleton<ITelegramClient, TelegramClient>()
        .AddSingleton<ILanguageDetectorService, LanguageDetectorService>()
        .AddSingleton<Client>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<Configuration>>().Value;
            return new(config.TELEGRAM_API_ID, config.TELEGRAM_API_HASH);
        })
        .AddSingleton<OpenAIAPI>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<Configuration>>().Value;
            return new(config.CHAT_API_KEY);
        })
        .AddSingleton<DetectLanguageClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<Configuration>>().Value;
            return new(config.DETECT_LANGUAGE_API_KEY);
        })
        .AddSingleton<TMDbClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<Configuration>>().Value;
            return new(config.TMDB_API_KEY);
        })

        // transient services
        .AddTransient(
            typeof(CancellationToken),
            sp => sp.GetRequiredService<ICancellationTokenHandlerService>().Token)

        // infrastructure services
        .AddMemoryCache()
        .AddDbContextPool<ApplicationDbContext>((sp, options) =>
        {
            options.UseLazyLoadingProxies();
            options.EnableSensitiveDataLogging();
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseMemoryCache(sp.GetRequiredService<IMemoryCache>());
            options.UseNpgsql(sp.GetRequiredService<IOptions<Configuration>>().Value.CONNECTION_STRING);
        })

        // add hosted services
        .AddHostedService<VideoMakerWorker>()
        .AddHostedService<TelegramScraperWorker>()
        .AddHostedService<FilmDownloaderWorker>();

        AddHttpClients(hostContext, services);
    }

    private static void AddHttpClients(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddHttpClient<IVideoDownloaderService, VideoDownloaderService>();
        services.AddHttpClient<IYoutubeMetadataScraperService, YoutubeMetadataScraperService>(client =>
        {
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", hostContext.Configuration.GetValue<string>("YOUTUBE_DOWNLOAD_API_KEY"));
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "ytstream-download-youtube-videos.p.rapidapi.com");
        });
        services.AddHttpClient<ITiktokMetadataScraperService, TiktokMetadataScraperService>(client =>
        {
            client.BaseAddress = new Uri("https://tiktok-no-watermark-video1.p.rapidapi.com/video_info/");
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", hostContext.Configuration.GetValue<string>("TIKTOK_DOWNLOAD_API_KEY"));
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "tiktok-no-watermark-video1.p.rapidapi.com");
        });
        services.AddHttpClient<ITiktokUploadVideoService, TiktokUploadVideoService>(client =>
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hostContext.Configuration.GetValue<string>("TIKTOK_DATA_API_KEY"));
        });
        services.AddHttpClient<IYoutubeUploadVideoService, YoutubeUploadVideoService>(client =>
        {
            client.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hostContext.Configuration.GetValue<string>("YOUTUBE_DATA_API_KEY"));
        });
        services.AddHttpClient<ITranslationService, TranslationService>(client =>
        {
            client.BaseAddress = new Uri("https://api-free.deepl.com/v2/");
        });
        services.AddHttpClient("OMDB", client =>
        {
            client.BaseAddress = new Uri($"http://www.omdbapi.com/?apikey={hostContext.Configuration.GetValue<string>("OMDB_API_KEY")}");
        });
        services.AddHttpClient("KINOPOISK", client =>
        {
            client.DefaultRequestHeaders.Add("x-api-key", hostContext.Configuration.GetValue<string>("KINOPOISK_API_KEY"));
            client.BaseAddress = new Uri("https://kinopoiskapiunofficial.tech/api/v2.2/");
        });
        services.AddHttpClient("HDREZKA", client =>
        {
            client.BaseAddress = new Uri("https://rezka.cc/");
        });
    }
}