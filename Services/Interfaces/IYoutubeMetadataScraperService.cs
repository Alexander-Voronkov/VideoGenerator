using VideoGenerator.Models.YoutubeMetadata;

namespace VideoGenerator.Services.Interfaces;

public interface IYoutubeMetadataScraperService
{
    Task<YoutubeMetadataResponseDto> ScrapeData(string videoUrl);
}
