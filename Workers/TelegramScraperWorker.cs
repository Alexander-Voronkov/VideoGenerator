using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using TL;
using VideoGenerator.Configurations;
using VideoGenerator.Infrastructure;
using VideoGenerator.Infrastructure.Entities;
using VideoGenerator.Services.Implementations;
using VideoGenerator.Services.Interfaces;
using WTelegram;

namespace VideoGenerator.Workers;
public class TelegramScraperWorker : BackgroundService
{
    private readonly Client _client;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TelegramScraperWorker> _logger;
    private readonly IOptions<Configuration> _config;
    private readonly ITelegramClient _telegramClient;
    private readonly ILanguageDetectorService _languageDetectorService;
    private readonly ITranslationService _translationService;

    public TelegramScraperWorker(
        Client client,
        ApplicationDbContext context,
        ILogger<TelegramScraperWorker> logger,
        IOptions<Configuration> config,
        ITelegramClient telegramClient,
        ILanguageDetectorService languageDetectorService,
        ITranslationService translationService)
    {
        _client = client;
        _context = context;
        _logger = logger;
        _config = config;
        _telegramClient = telegramClient;
        _languageDetectorService = languageDetectorService;
        _translationService = translationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunScrapperAsync(stoppingToken);
    }

    private async Task RunScrapperAsync(CancellationToken stoppingToken = default)
    {
        var loginInfo = _config.Value.TELEGRAM_API_PHONE;
        while (_client.User is null)
        {
            switch (await _client.Login(loginInfo)) // returns which config is needed to continue login
            {
                case "verification_code": Console.Write("Code: "); loginInfo = Console.ReadLine(); break;
                case "name": loginInfo = "John Doe"; break;    // if sign-up is required (first/last_name)
                case "password": loginInfo = "secret!"; break; // if user has enabled 2FA
                default: loginInfo = null; break;
            }
        }

        _client.OnUpdate += _telegramClient.ProcessUpdate;
        var me = _client.User;
        var chats = await _client.Messages_GetAllChats();

        var channels = chats.chats
            .Where(x => x.Value.IsChannel)
            .ToList();

        await Seed(channels, stoppingToken);

        _logger.LogInformation("Telegram client {ID}:{Username} logged", me.ID, me.MainUsername);
        _logger.LogInformation("Available channels: {@Channels}", channels);
    }

    private async Task Seed(List<KeyValuePair<long, ChatBase>> channels, CancellationToken token = default)
    {
        var languages = _context.Languages
            .AsNoTracking()
            .ToArray();

        if (languages.Length == 0)
        {
            languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures)
                .Select(x => new Language
                {
                    LanguageCode = x.TwoLetterISOLanguageName,
                })
                .Skip(1)
                .ToArray();

            await _context.Languages.AddRangeAsync(languages, token);

            await _context.SaveChangesAsync(token);
        }

        var topics = _context.Topics
            .AsNoTracking()
            .ToArray();

        var missingTopics = _config.Value.GENERAL_TOPICS
            .Where(t => !topics.Any(x => x.TopicName == t))
            .ToArray();

        if (missingTopics.Length != 0)
        {
            topics = missingTopics
                .Select(t => new Topic
                {
                    TopicName = t,
                    AvailableLanguages = languages
                })
                .ToArray();

            await _context.Topics.AddRangeAsync(topics, token);

            await _context.SaveChangesAsync(token);
        }

        var groups = _context.Groups
            .AsNoTracking()
            .ToArray();

        if (groups.Length == 0)
        {
            var result = channels
                .Where(x => _config.Value.SOURCE_GROUPS
                    .Any(s => s
                        .Split('/')
                        .Last() == x.Value.MainUsername))
                .Select(x => new Group
                {
                    GroupId = x.Value.ID,
                    GroupLink = x.Value.MainUsername,
                    GroupName = x.Value.Title,
                    TopicId = topics[Random.Shared.Next(0, topics.Length)].TopicId,
                    LanguageId = languages[Random.Shared.Next(0, languages.Length)].LanguageId
                });

            await _context.Groups.AddRangeAsync(result, token);

            await _context.SaveChangesAsync(token);
        }
    }
}
