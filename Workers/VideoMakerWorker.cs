using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoGenerator.Services.Interfaces;
using VideoGenerator.Helpers;

namespace VideoGenerator.Workers;

public class VideoMakerWorker : BackgroundService
{
    private readonly ILogger _logger;
	private readonly IVideoGenerationService _videoService;
	private readonly ISubtitleGeneratorService _subtitleGeneratorService;
	private readonly ITextToSpeechService _textToSpeechService;
	private readonly IAssConvertService _assConvertService;

	private string audioTextTestPath = "C:\\Users\\Zoranais\\source\\repos\\VideoGaynerator\\Output\\testaudio.mp3";
	private string brainrotVideoPath = "C:\\VideoGenerator\\Input\\brainrot.mp4";
    private string outputPath = "C:\\VideoGenerator\\Output\\";

    public VideoMakerWorker(
		ILogger<VideoMakerWorker> logger, 
		IVideoGenerationService videoService, 
		ISubtitleGeneratorService subtitleGeneratorService,
		ITextToSpeechService textToSpeechService,
		IAssConvertService assConvertService)
    {
        _logger = logger;
        _videoService = videoService;
        _subtitleGeneratorService = subtitleGeneratorService;
        _textToSpeechService = textToSpeechService;
        _assConvertService = assConvertService;
    }

    protected override async Task ExecuteAsync(CancellationToken token = default)
    {
		try
		{
			_logger.LogInformation("VideoMakerWorker started dependecies installation");
			await InstallDependenciesHelper.InstallAllDependencies(token);
			_logger.LogInformation("VideoMakerWorker finished dependecies installation");
		}
		catch (Exception ex)
		{
			_logger.LogError(exception: ex, message: ex.Message);
		}

        await Task.Delay(1);
        while (!token.IsCancellationRequested)
		{
			try
			{
				_logger.LogInformation("VideoMakerWorker started successfully");

				var testText = "My name is Irina Dzerjinska, and I am literally Dubai chocolate.\n\nOdin crafted me from divine cacao during a sandstorm over the Burj Khalifa, declaring, “Let sweetness conquer vanity.” Thor laughed, struck his hammer, and the lightning tempered my shell to perfection.\n\nNow I walk among mortals — part goddess, part dessert — melting hearts faster than heat ever could. Some call me a miracle, others a myth.\n\nBut when thunder rolls across the Gulf, I know the gods still crave a taste.";
				var audioOutputPath = outputPath + "tts" + Guid.NewGuid() + ".mp3";
				var audioResult = await _textToSpeechService.CreateTextToSpeech(testText, "en");
				await File.WriteAllBytesAsync(audioOutputPath, audioResult.Audio, token);

				var subtitles = _assConvertService.ConvertFromTimestampedTranscript(audioResult.Timestamps, 9);
				
				var subtitlesPath = outputPath + "subtitles" + Guid.NewGuid() + ".ass";
				await File.WriteAllTextAsync(subtitlesPath, subtitles, token);
				
				var videoName = outputPath + "video" + Guid.NewGuid() + ".mp4";
				await _videoService.CreateVideo(audioOutputPath, subtitlesPath, brainrotVideoPath, videoName);
				_logger.LogInformation("VideoMakerWorker generated a stupid brainrot shit");
            }
			catch (Exception ex)
			{
				_logger.LogError(exception: ex, message: ex.Message);
			}
			await Task.Delay(1000000);
		}
    }
}
