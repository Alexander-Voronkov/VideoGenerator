using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using VideoGenerator.Configurations;
using VideoGenerator.Exceptions;
using VideoGenerator.Models.TranslationService;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class TranslationService : ITranslationService
{
    private readonly IOptions<Configuration> _config;
    private readonly HttpClient _client;

    public TranslationService(IOptions<Configuration> config, HttpClient client)
    {
        _config = config;
        _client = client;
    }

    public async Task<IEnumerable<LanguageDto>> GetAvailableLanguages(CancellationToken token = default)
    {
        try
        {
            var response = await _client
                .GetFromJsonAsync<LanguageDto[]>($"languages?" +
                $"auth_key={_config.Value.DEEPL_API_KEY}",
                token);

            return response;
        }
        catch (Exception ex)
        {
            throw new LanguageLoadError(ex.Message);
        }
    }

    public async Task<TranslationResultDto> Translate(string text, CultureInfo targetLanguage, CancellationToken token = default)
    {
        try
        {
            var response = await _client
                .GetFromJsonAsync<TranslationResultDto>($"https://api-free.deepl.com/v2/translate?" +
                $"auth_key={_config.Value.DEEPL_API_KEY}" +
                $"&text={text}" +
                $"&target_lang={targetLanguage.TwoLetterISOLanguageName}",
                token);

            return response;
        }
        catch (Exception ex)
        {
            throw new TranslationError(ex.Message);
        }
    }
}