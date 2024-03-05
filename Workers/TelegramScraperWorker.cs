using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using VideoGenerator.AllConstants;
using VideoGenerator.Configurations;
using VideoGenerator.Infrastructure;
using VideoGenerator.Infrastructure.Entities;
using VideoGenerator.Services.Interfaces;
using WTelegram;

namespace VideoGenerator.Workers;
public class TelegramScraperWorker : BackgroundService
{
    private readonly ILogger<TelegramScraperWorker> _logger;
    private readonly IOptions<Configuration> _config;
    private readonly ITelegramClient _telegramClient;
    private readonly ILanguageDetectorService _languageDetectorService;
    private readonly Client _client;
    private readonly ApplicationDbContext _context;

    public TelegramScraperWorker(
        ILogger<TelegramScraperWorker> logger,
        IOptions<Configuration> config,
        ITelegramClient telegramClient,
        Client client,
        ApplicationDbContext context,
        ILanguageDetectorService languageDetectorService)
    {
        _logger = logger;
        _config = config;
        _telegramClient = telegramClient;
        _client = client;
        _context = context;
        _languageDetectorService = languageDetectorService;
    }

    protected override async Task ExecuteAsync(CancellationToken token = default)
    {
        Language[] languages = _context.Languages.ToArray();

        if (!_context.Languages.Any())
        {
            languages = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Select(x => new Language
                {
                    LanguageCode = x.IetfLanguageTag,
                }).ToArray();

            await _context.Languages.AddRangeAsync(languages, token);

            await _context.SaveChangesAsync(token);
        }

        await _context.Topics.AddRangeAsync(Constants.GeneralTopics
            .Select(t =>
            {
                var culture = _languageDetectorService.Detect(t).Result;
                var topic = new Topic
                {
                    TopicName = t,
                    Language = languages
                    .FirstOrDefault(l => l.LanguageCode
                        .Equals(
                            culture.TwoLetterISOLanguageName,
                            StringComparison.InvariantCultureIgnoreCase))
                };
                return topic;
            }), token);

        await _context.SaveChangesAsync(token);

        var loginInfo = _config.Value.TELEGRAM_API_PHONE;
        while (_client.User == null)
        {
            switch (_client.Login(loginInfo).Result) // returns which config is needed to continue login
            {
                case "verification_code": Console.Write("Code: "); loginInfo = Console.ReadLine(); break;
                case "name": loginInfo = "John Doe"; break;    // if sign-up is required (first/last_name)
                case "password": loginInfo = "secret!"; break; // if user has enabled 2FA
                default: loginInfo = null; break;
            }
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
    }
}
