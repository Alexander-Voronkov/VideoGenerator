using VideoGenerator.Models.FilmMetadata;

namespace VideoGenerator.Services.Interfaces;

public interface IFilmMetadataScraperService
{
    Task<FilmMetadata> GetFilmMetadataAsync(string filmName, CancellationToken token = default);
    Task<FilmMetadata[]> GetPopularFilmsListAsync(CancellationToken token = default);
    Task<DownloadLink[]> GetFilmDownloadLinksAsync(string filmLink, CancellationToken token = default);
    Task<string[]> GetSearchResultsByFilmNameAsync(string filmName, CancellationToken token = default);
}