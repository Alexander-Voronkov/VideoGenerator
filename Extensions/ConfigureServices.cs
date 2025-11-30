using DetectLanguage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;
using System.Text;
using VideoGenerator.Configs;
using VideoGenerator.Configurations;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Implementations;
using VideoGenerator.Services.Interfaces;
using VideoGenerator.Workers;

namespace VideoGenerator.Extensions;

public static partial class Extensions
{
    public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        //configuration
        services

            // logging
            .AddSerilog(logger =>
            {
                Console.OutputEncoding = Encoding.UTF8;
                logger.ReadFrom.Configuration(hostContext.Configuration);
            })
            .AddSingleton<DetectLanguageClient>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new(config["DetectLanguageApiKey"]);
            })
            .AddDbContextFactory<ApplicationDbContext>(x =>
            {
                x.UseNpgsql(hostContext.Configuration.GetConnectionString("Default"));
            })
            .AddSingleton<IMinioBlobService, MinioBlobService>()
            .AddSingleton<ISubtitleGeneratorService, SubtitleGeneratorService>()
            .AddSingleton<IVideoGenerationService, VideoGenerationService>()
            .AddSingleton<IVideoProcessingService, VideoProcessingService>()
            .AddSingleton<IAssConvertService, AssConvertService>()
            .AddSingleton<ITextToSpeechService, ElevenLabsTtsService>()
            .AddSingleton<IPexelsService, PexelsService>()
            .AddSingleton<IStockContentService, StockContentService>()
            .Configure<SubtitlesConfig>(hostContext.Configuration.GetSection("SubtitlesConfig"))
            .Configure<PexelsStockContentConfig>(hostContext.Configuration.GetSection("PexelsStockContentConfig"))
            .Configure<MinioBlobConfig>(hostContext.Configuration.GetSection("MinioConfig"))
            .Configure<OpenAiConfig>(hostContext.Configuration.GetSection("OpenAiConfig"))
            .Configure<ElevenLabsConfig>(hostContext.Configuration.GetSection("ElevenLabsConfig"))
            .Configure<RedditStoryConfig>(hostContext.Configuration.GetSection("RedditStoryConfig"));

			// add hosted services

		var enabledWorkers = hostContext.Configuration.GetSection("Workers").Get<string[]>() ?? Array.Empty<string>();

        var workers = typeof(RedditBrainrotMakerWorker).Assembly.GetTypes().Where(t => 
            t.IsSubclassOf(typeof(BackgroundService)) && !
            t.IsAbstract && 
            enabledWorkers.Contains(t.Name));

		foreach (var workerType in workers)
		{
			var method = typeof(ServiceCollectionHostedServiceExtensions)
				.GetMethods()
				.First(m =>
					m.Name == "AddHostedService" &&
					m.IsGenericMethodDefinition &&
					m.GetParameters().Length == 1
				);

			var genericMethod = method.MakeGenericMethod(workerType);

			genericMethod.Invoke(null, new object[] { services });
		}
	}
}