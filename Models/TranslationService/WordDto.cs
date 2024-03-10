using System.Text.Json.Serialization;

namespace VideoGenerator.Models.TranslationService;

public class WordDto
{
    [JsonPropertyName("detected_source_language")]
    public string DetectedLanguage { get; set; }

    [JsonPropertyName("text")]
    public string WordText { get; set; }
}
