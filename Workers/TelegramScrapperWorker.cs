using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoGenerator.Services.Interfaces;
using WTelegram;

namespace VideoGenerator.Workers;
public class TelegramScrapperWorker : BackgroundService
{
    private readonly ILogger<TelegramScrapperWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly ITelegramClient _telegramClient;
    private readonly Client _client;

    public TelegramScrapperWorker(
        ILogger<TelegramScrapperWorker> logger,
        IConfiguration configuration,
        ITelegramClient telegramClient,
        Client client)
    {
        _logger = logger;
        _configuration = configuration;
        _telegramClient = telegramClient;
        _client = client;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var loginInfo = _configuration["PHONE"];
        while (_client.User == null)
            switch (_client.Login(loginInfo).Result) // returns which config is needed to continue login
            {
                case "verification_code": Console.Write("Code: "); loginInfo = Console.ReadLine(); break;
                case "name": loginInfo = "John Doe"; break;    // if sign-up is required (first/last_name)
                case "password": loginInfo = "secret!"; break; // if user has enabled 2FA
                default: loginInfo = null; break;
            }
        _client.OnUpdate += _telegramClient.ProcessUpdate;
        var me = _client.User;
        var chats = _client.Messages_GetAllChats().Result;
        if (_logger != null)
        {
            var channels = chats.chats
                .Where(x => x.Value.IsChannel == true)
                .Select(x => x.Value.Title)
                .ToList();

            _logger.LogInformation("Telegram client {ID}:{Username} logged", me.ID, me.MainUsername);
            _logger.LogInformation("Available channels: {@Channels}", channels);
        }

        return Task.CompletedTask;
    }
}
