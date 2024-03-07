using Microsoft.EntityFrameworkCore;
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
        await Seed();

        var loginInfo = _config.Value.TELEGRAM_API_PHONE;
        while (_client.User is null)
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

    private async Task Seed(CancellationToken token = default)
    {
        var languages = _context.Languages
            .AsNoTracking()
            .ToArray();

        if (!languages.Any())
        {
            languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures)
                .Select(x => new Language
                {
                    LanguageCode = x.IetfLanguageTag,
                })
                .Skip(1)
                .ToArray();

            await _context.Languages.AddRangeAsync(languages, token);

            await _context.SaveChangesAsync(token);
        }

        var topics = _context.Topics
            .AsNoTracking()
            .ToArray();

        if (!topics.Any())
        {
            topics = Constants.GeneralTopics
                .Select(t =>
                {
                    var cultures = _languageDetectorService.Detect(t).Result
                        .Select(x => x.TwoLetterISOLanguageName)
                        .ToArray();
                    return new Topic
                    {
                        TopicName = t,
                        LanguageId = languages
                            .FirstOrDefault(l => cultures.Contains(l.LanguageCode)).LanguageId
                    };
                }).ToArray();

            await _context.Topics.AddRangeAsync(topics, token);

            await _context.SaveChangesAsync(token);
        }
    }
}
