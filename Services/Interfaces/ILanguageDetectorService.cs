using System.Globalization;

namespace VideoGenerator.Services.Interfaces;

public interface ILanguageDetectorService
{
    Task<CultureInfo[]> Detect(string text, CancellationToken token = default);
}
