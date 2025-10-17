using DetectLanguage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
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
            .AddScoped<IMinioBlobService, MinioBlobService>()
            .AddScoped<IOpenAiService, OpenAIService>()
            .AddScoped<ISubtitleGeneratorService, SubtitleGeneratorService>()
            .AddScoped<IVideoGenerationService, VideoGenerationService>()
            .AddScoped<IVideoProcessingService, VideoProcessingService>()
            .AddScoped<IAssConvertService, AssConvertService>()
            .AddScoped<ITextToSpeechService, ElevenLabsTtsService>()
            .Configure<SubtitlesConfig>(hostContext.Configuration.GetSection("SubtitlesConfig"))
            .Configure<MinioBlobConfig>(hostContext.Configuration.GetSection("MinioConfig"))
            .Configure<OpenAiConfig>(hostContext.Configuration.GetSection("OpenAiConfig"))
            .Configure<ElevenLabsConfig>(hostContext.Configuration.GetSection("ElevenLabsConfig"))

            // add hosted services

            .AddHostedService<VideoSplitterWorker>();
        //.AddHostedService<VideoMakerWorker>();
    }
}