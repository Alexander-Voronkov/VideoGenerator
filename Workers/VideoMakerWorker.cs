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

	private string audioTextTestPath = "C:\\Users\\Zoranais\\source\\repos\\VideoGaynerator\\Output\\testaudio.mp3";
	private string brainrotVideoPath = "C:\\Users\\Zoranais\\source\\repos\\VideoGaynerator\\Output\\brainrot.mp4";
    private string outputPath = "C:\\Users\\Zoranais\\source\\repos\\VideoGaynerator\\Output\\";

    public VideoMakerWorker(
		ILogger<VideoMakerWorker> logger, 
		IVideoGenerationService videoService, 
		ISubtitleGeneratorService subtitleGeneratorService)
    {
        _logger = logger;
        _videoService = videoService;
        _subtitleGeneratorService = subtitleGeneratorService;
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

				//var subtitlesName = outputPath +"subtitle" + Guid.NewGuid() + ".srt";
				//await _subtitleGeneratorService.GenerateSubtitles(audioTextTestPath, subtitlesName, token);

				var subtitlesName = outputPath + "testaudio.srt";

                var videoName = outputPath + "video" + Guid.NewGuid() + ".mp4";
				await _videoService.CreateVideo(audioTextTestPath, subtitlesName, brainrotVideoPath, videoName);
				_logger.LogInformation("VideoMakerWorker generated a stupid brainrot shit");
            }
			catch (Exception ex)
			{
				_logger.LogError(exception: ex, message: ex.Message);
			}
			await Task.Delay(1000);
		}
    }
}
