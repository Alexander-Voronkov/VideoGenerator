using DetectLanguage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text;
using VideoGenerator.Configs;
using VideoGenerator.Configurations;
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
        .AddScoped<IMinioBlobService, MinioBlobService>()
        .AddScoped<IOpenAiService, OpenAIService>()
        .AddScoped<ISubtitleGeneratorService, SubtitleGeneratorService>()
        .AddScoped<IVideoGenerationService, VideoGenerationService>()
        .AddScoped<IVideoProcessingService, VideoProcessingService>()
        .AddScoped<IAssConvertService, AssConvertService>()
        .Configure<SubtitlesConfig>(hostContext.Configuration.GetSection("SubtitlesConfig"))
        .Configure<MinioBlobConfig>(hostContext.Configuration.GetSection("MinioConfig"))
        .Configure<OpenAiConfig>(hostContext.Configuration.GetSection("OpenAiConfig"))

		// add hosted services
		.AddHostedService<VideoMakerWorker>();
    }
}