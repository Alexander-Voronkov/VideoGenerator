using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Sinks.PeriodicBatching;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace VideoGenerator.Logging;
public class TelegramSink : IBatchedLogEventSink
{
	private readonly ITextFormatter _textFormatter;
	private readonly ITelegramBotClient _telegramBotClient;
	private readonly string _chatId;

	private readonly string[] _messagesToSkip = {
		"A task was canceled",
		"An error occurred using the connection to database",
		"The operation was canceled"
	};

	public TelegramSink(string chatId, string token, string template)
	{
		_textFormatter = new MessageTemplateTextFormatter(template);
		_chatId = chatId;
		_telegramBotClient = new TelegramBotClient(token);
	}

	private bool CanInclude(LogEvent logEvent)
	{
		//We skip canceled exception to not to span to telegram

		if (logEvent.Exception is TaskCanceledException)
		{
			return false;
		}

		return true;
	}

	public async Task EmitBatchAsync(IEnumerable<LogEvent> events)
	{
		var message = FormatEvents(events).ToArray();

		if (message.Length == 0)
		{
			return;
		}

		try
		{
			var result = await SendMessages(message);

			if (!result)
			{
				SelfLog.WriteLine("Failed to send message to telegram");
			}
		}
		catch (Exception ex)
		{
			SelfLog.WriteLine("Failed to send message to telegram: {0}", ex.ToString());
		}
	}

	public async Task OnEmptyBatchAsync()
	{
		await Task.CompletedTask;
	}

	private async Task<bool> SendMessages(IEnumerable<string> texts)
	{
		ServicePointManager.Expect100Continue = true;
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

		const short telegramMessageLimit = 4000;

		foreach (var text in texts)
		{
			var mutableText = text;
			if (text.Length > telegramMessageLimit)
			{
				mutableText = text[..telegramMessageLimit] + "```";
			}

			await _telegramBotClient.SendTextMessageAsync(_chatId, mutableText, null, ParseMode.Markdown,
				disableWebPagePreview: true);

		}
		return true;
	}


	private IEnumerable<string> FormatEvents(IEnumerable<LogEvent> events)
	{
		if (events == null)
			throw new ArgumentNullException(nameof(events));

		foreach (var logEvent in events)
		{
			if (!CanInclude(logEvent))
			{
				continue;
			}

			var payload = new StringWriter();

			_textFormatter.Format(logEvent, payload);

			var message = payload.ToString();

			if (_messagesToSkip.All(m => !message.Contains(m)))
			{
				yield return message;
			}
		}
	}
}
