using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using TL;
using VideoGenerator.Configurations;
using VideoGenerator.Infrastructure;
using VideoGenerator.Infrastructure.Entities;
using VideoGenerator.Services.Interfaces;
using WTelegram;

namespace VideoGenerator.Workers;
public class TelegramScraperWorker : BackgroundService
{
    private readonly Client _client;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TelegramScraperWorker> _logger;
    private readonly Configuration _config;
    private readonly ITelegramClient _telegramClient;

    public TelegramScraperWorker(
        Client client,
        ApplicationDbContext context,
        ILogger<TelegramScraperWorker> logger,
        IOptions<Configuration> config,
        ITelegramClient telegramClient)
    {
        _client = client;
        _context = context;
        _logger = logger;
        _config = config.Value;
        _telegramClient = telegramClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunScrapperAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Publish failed: {message} {@exception}", 
                    ex.Message, 
                    ex);
            }

            await Task.Delay(_config.TELEGRAM_UPDATE_DELAY, stoppingToken);
        }
    }

    private async Task PublishAsync(CancellationToken token)
    {
        var message = _context
            .QueueMessages
            .Include(x => x.SourceGroup)
            .Include(x => x.Attachments)
            .FirstOrDefault();

        if (message != null)
        {
            var topic = message.SourceGroup.TopicId;
            var channels = _context.Groups
                .Where(x => x.IsTarget && x.TopicId == topic)
                .Select(x => x.GroupId)
                .ToList();

            if (channels.Any())
            {
                _logger.LogInformation("{messageID} Publish started",
                    message.QueueMessageId);
            }
            else
            {
                _logger.LogWarning("Publish failed: Target channels for topic {topicID} not found", topic);
            }

            foreach (var channel in channels)
            {
                await _telegramClient.SendMessage(channel, message, token);
                await Task.Delay(Random.Shared.Next(10000, 20000), token);
            }

            _context.QueueMessages.Remove(message);
            await _context.SaveChangesAsync(token);
            _logger.LogInformation("{messageID} Publish finished", message.QueueMessageId);
        }
        else
        {
            _logger.LogWarning("Publish failed: Message queue is empty");
        }
    }

    private async Task RunScrapperAsync(CancellationToken stoppingToken = default)
    {
        var loginInfo = _config.TELEGRAM_API_PHONE;
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
                    IsAvailable = _config.AVAILABLE_LANGUAGES
                        .Contains(x.TwoLetterISOLanguageName)
                })
                .Skip(1)
                .ToArray();

            await _context.Languages.AddRangeAsync(languages, token);

            await _context.SaveChangesAsync(token);
        }

        var topics = _context.Topics
            .AsNoTracking()
            .ToArray();

        var missingTopics = _config.GENERAL_TOPICS
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
                .Where(x => _config.SOURCE_GROUPS
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

            groups = result.ToArray();
        }

        var targetGroups = groups.Where(x => x.IsTarget);
        foreach (var topic in topics)
        {
            foreach (var language in languages.Where(x => x.IsAvailable))
            {
                if(!targetGroups.Any(x => x.LanguageId == language.LanguageId && x.TopicId == topic.TopicId))
                {
                    await _telegramClient.CreateGroup(language, topic);
                    await Task.Delay(Random.Shared.Next(10000, 40000), token);
                }
            }
        }
    }
}
