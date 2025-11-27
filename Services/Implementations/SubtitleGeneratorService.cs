using ElevenLabs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using VideoGenerator.Entities;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class SubtitleGeneratorService : ISubtitleGeneratorService
{
    private readonly ILogger _logger;
	private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
	private readonly IMinioBlobService _minioBlobService;
	private readonly ITextToSpeechService _textToSpeechService;
	private readonly IAssConvertService _assConvertService;

	private const string TtsSubtitlesBucket = "tts-subtitles";
	private const string AssSubtitlesBucket = "ass-subtitles";

	private const string Language = "en";

	public SubtitleGeneratorService(
		ILogger<SubtitleGeneratorService> logger,
		IDbContextFactory<ApplicationDbContext> dbContextFactory,
		IMinioBlobService minioBlobService,
		ITextToSpeechService textToSpeechService,
		IAssConvertService assConvertService)
    {
        _logger = logger;
		_dbContextFactory = dbContextFactory;
		_minioBlobService = minioBlobService;
		_textToSpeechService = textToSpeechService;
		_assConvertService = assConvertService;
    }

    public async Task GenerateSubtitles(GenerationQueueItem pendingItem, CancellationToken token = default)
	{
		_logger.LogInformation("{service}: start subtitles generation.", nameof(SubtitleGeneratorService));

		var dbContext = _dbContextFactory.CreateDbContext();

		var objectName = pendingItem.Id + Language;

		var ttsExists = await _minioBlobService.ExistsAsync(TtsSubtitlesBucket, objectName, token);
		var assExists = await _minioBlobService.ExistsAsync(AssSubtitlesBucket, objectName, token);

		var generatedSubtitle = await dbContext.Set<GeneratedSubtitle>()
			.FirstOrDefaultAsync(x => x.TtsBlobPath == $"{TtsSubtitlesBucket}/{objectName}" || x.AssBlobPath == $"{AssSubtitlesBucket}/{objectName}", token);

		if (!ttsExists)
		{
			var text = pendingItem.Title + "\n" + pendingItem.Text;
			var audioResult = await _textToSpeechService.CreateTextToSpeech(text, pendingItem.SexType, Language);
			await using (var str = new MemoryStream(audioResult.Audio))
			{
				await _minioBlobService.UploadAsync(TtsSubtitlesBucket, objectName, str, "audio/mpeg", token);
			}

			if (generatedSubtitle is not null)
			{
				generatedSubtitle.TtsBlobPath = $"{TtsSubtitlesBucket}/{objectName}";
			}
			else
			{
				generatedSubtitle = new()
				{
					TtsBlobPath = $"{TtsSubtitlesBucket}/{objectName}",
					Timestamps = JsonSerializer.Serialize(audioResult.Timestamps)
				};

				dbContext = _dbContextFactory.CreateDbContext();

				dbContext.Set<GeneratedSubtitle>().Add(generatedSubtitle);
				await dbContext.SaveChangesAsync(token);
			}
		}

		if (!assExists)
		{
			var subtitles = _assConvertService.ConvertFromTimestampedTranscript(JsonSerializer.Deserialize<TimestampedTranscriptCharacter[]>(generatedSubtitle.Timestamps), 9, false);
			await using (var str = new MemoryStream(Encoding.UTF8.GetBytes(subtitles)))
			{
				await _minioBlobService.UploadAsync(AssSubtitlesBucket, objectName, str, "text/ssa", token);
			}
			generatedSubtitle.AssBlobPath = $"{AssSubtitlesBucket}/{objectName}";
		}

		_logger.LogInformation("VideoMakerWorker: end subtitles generation.");

		await dbContext.SaveChangesAsync(token);
		dbContext.Dispose();
	}
}
