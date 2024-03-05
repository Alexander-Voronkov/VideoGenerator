using VideoGenerator.Models;

namespace VideoGenerator.Services.Interfaces;

public interface IYoutubeMetadataScraperService
{
    Task<YoutubeMetadataResponseDto> ScrapeData(string videoUrl);
}
