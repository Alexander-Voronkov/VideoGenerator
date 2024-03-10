using System.Text.Json.Serialization;

namespace VideoGenerator.Models.TranslationService;

public class TranslationResultDto
{
    [JsonPropertyName("translations")]
    public IEnumerable<WordDto> Translations { get; set; }
}