using VideoGenerator.Models.TiktokMetadata;

namespace VideoGenerator.Services.Interfaces;

public interface ITiktokMetadataScraperService
{
    Task<TiktokMetadataResponseDto> ScrapeData(string videoUrl);
}
