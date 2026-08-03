using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace VideoGenerator.Logging;
public static class SerilogExtensions
{
	public static LoggerConfiguration Telegram(this LoggerSinkConfiguration sinkConfiguration, LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum, string chatId = null,
		string token = null, string outputTemplate = null)
	{
		var telegramSink = new TelegramSink(chatId, token, outputTemplate);

		var batchingOptions = new PeriodicBatchingSinkOptions
		{
			BatchSizeLimit = 1,
			Period = TimeSpan.FromSeconds(30),
			EagerlyEmitFirstEvent = true,
			QueueLimit = 10000
		};

		var batchingSink = new PeriodicBatchingSink(telegramSink, batchingOptions);

		return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel);
	}
}
