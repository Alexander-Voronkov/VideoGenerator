using System.Globalization;
using VideoGenerator.Models.TranslationService;

namespace VideoGenerator.Services.Interfaces;

public interface ITranslationService
{
    Task<TranslationResultDto> Translate(string text, CultureInfo targetLanguage, CancellationToken token = default);
    Task<IEnumerable<LanguageDto>> GetAvailableLanguages(CancellationToken token = default);
}