using System.Text.Json.Serialization;

namespace VideoGenerator.Models.TranslationService;

public class LanguageDto
{
    [JsonPropertyName("language")]
    public string ShortForm { get; set; }
}
