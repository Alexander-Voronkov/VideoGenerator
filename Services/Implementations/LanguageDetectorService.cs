using DetectLanguage;
using Microsoft.Extensions.Options;
using System.Globalization;
using VideoGenerator.Configurations;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class LanguageDetectorService : ILanguageDetectorService
{
    private readonly DetectLanguageClient _languageDetector;
    private readonly IOptions<Configuration> _config;

    public LanguageDetectorService(IOptions<Configuration> config, DetectLanguageClient detectLanguageClient)
    {
        _config = config;
        _languageDetector = detectLanguageClient;
    }

    public async Task<CultureInfo[]> Detect(string text, CancellationToken token = default)
    {
        var lang = await _languageDetector.DetectAsync(text);
        return lang.Select(x => CultureInfo.GetCultureInfo(x.language)).ToArray();
    }
}