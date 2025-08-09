using DetectLanguage;
using System.Globalization;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class LanguageDetectorService : ILanguageDetectorService
{
    private readonly DetectLanguageClient _languageDetector;

    public LanguageDetectorService(
        DetectLanguageClient detectLanguageClient)
    {
        _languageDetector = detectLanguageClient;
    }

    public async Task<CultureInfo[]> Detect(string text, CancellationToken token = default)
    {
        var lang = await _languageDetector.DetectAsync(text);
        return lang.Select(x => CultureInfo.GetCultureInfo(x.language)).ToArray();
    }
}