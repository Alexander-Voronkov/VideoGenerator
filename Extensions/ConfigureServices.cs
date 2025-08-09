using DetectLanguage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text;
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

		// add hosted services
		.AddHostedService<VideoMakerWorker>();
    }
}