using ElevenLabs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using VideoGenerator.Configs;
using VideoGenerator.Entities;
using VideoGenerator.Infrastructure;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Workers;

public class VideoMakerWorker : BackgroundService
{
    private readonly ILogger _logger;
	private readonly IVideoGenerationService _videoService;
	private readonly ITextToSpeechService _textToSpeechService;
	private readonly IAssConvertService _assConvertService;
	private readonly IMinioBlobService _minioBlobService;
	private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

	private const int JobIntervalInMinutes = 15;

	private const string TtsSubtitlesBucket = "tts-subtitles";
	private const string AssSubtitlesBucket = "ass-subtitles";
	private const string GeneratedVideosBucket = "generated-videos";
	private const string BackgroundMusic = "background-music";

    public VideoMakerWorker(
		ILogger<VideoMakerWorker> logger, 
		IVideoGenerationService videoService, 
		ISubtitleGeneratorService subtitleGeneratorService,
		ITextToSpeechService textToSpeechService,
		IAssConvertService assConvertService,
		IMinioBlobService minioBlobService,
		IOptions<MinioBlobConfig> minioBlobConfig,
		IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _logger = logger;
        _videoService = videoService;
        _textToSpeechService = textToSpeechService;
        _assConvertService = assConvertService;
		_minioBlobService = minioBlobService;
		_dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken token = default)
    {
        await Task.Delay(1, token);
        
		await _minioBlobService.MakeBucketPublicAsync(AssSubtitlesBucket, token);
		await _minioBlobService.MakeBucketPublicAsync(TtsSubtitlesBucket, token);
		await _minioBlobService.MakeBucketPublicAsync(GeneratedVideosBucket, token);
		await _minioBlobService.MakeBucketPublicAsync(BackgroundMusic, token);

		while (!token.IsCancellationRequested)
		{
			_logger.LogInformation("VideoMakerWorker started.");

			try
			{
				const string language = "en";

				var dbContext = _dbContextFactory.CreateDbContext();

				var pendingText = await dbContext.Set<GenerationQueueItem>()
					.Where(x => x.Id == "1nbq3x2")
					//.Where(x => x.Status == GenerationStatus.ReadyToProcess)
					.FirstOrDefaultAsync(token);

				pendingText.Status = GenerationStatus.Processing;
				await dbContext.SaveChangesAsync(token);

				_logger.LogInformation("VideoMakerWorker: start subtitles generation.");

				try
				{
					var objectName = pendingText.Id + language;

					var ttsExists = await _minioBlobService.ExistsAsync(TtsSubtitlesBucket, objectName, token);
					var assExists = await _minioBlobService.ExistsAsync(AssSubtitlesBucket, objectName, token);

					var generatedSubtitle = await dbContext.Set<GeneratedSubtitle>()
						.FirstOrDefaultAsync(x => x.TtsBlobPath == $"{TtsSubtitlesBucket}/{objectName}" || x.AssBlobPath == $"{AssSubtitlesBucket}/{objectName}", token);

					if (!ttsExists)
					{
						var text = pendingText.Title + "\n" + pendingText.Text;
						var audioResult = await _textToSpeechService.CreateTextToSpeech(text, pendingText.SexType, language);
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

					_logger.LogInformation("VideoMakerWorker: start video generation.");

					var objectNames = await _videoService.CreateVideo(objectName, pendingText.Title, token);

					_logger.LogInformation("VideoMakerWorker: end video generation.");

					dbContext = _dbContextFactory.CreateDbContext();

					dbContext.Attach(pendingText);

					for (int i = 0; i < objectNames.Length; i++)
					{
						dbContext.Set<GeneratedVideo>().Add(new()
						{
							GenerationQueueId = pendingText.Id,
							BlobPath = $"{GeneratedVideosBucket}/{objectNames[i]}",
							UploadingStatus = UploadingStatus.NotUploaded,
							PartNumber = i + 1,
							TotalParts = objectNames.Length
						});
					}

					pendingText.Status = GenerationStatus.Processed;

					await dbContext.SaveChangesAsync(token);

					dbContext.Dispose();
				}
				catch(Exception ex)
				{
					_logger.LogError(exception: ex, message: $"An error occurred while trying to execute {nameof(VideoMakerWorker)} background service : {ex.Message}");

					dbContext = _dbContextFactory.CreateDbContext();

					dbContext.Attach(pendingText);
					pendingText.Status = GenerationStatus.ReadyToProcess;

					await dbContext.SaveChangesAsync(token);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(exception: ex, message: $"An error occurred while trying to execute {nameof(VideoMakerWorker)} background service : {ex.Message}");
			}
			await Task.Delay(TimeSpan.FromMinutes(JobIntervalInMinutes), token);
		}
    }
}
