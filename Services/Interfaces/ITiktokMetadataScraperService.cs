using VideoGenerator.Models;

namespace VideoGenerator.Services.Interfaces;

public interface ITiktokMetadataScraperService
{
    Task<TiktokMetadataResponseDto> ScrapeData(string videoUrl);
}
