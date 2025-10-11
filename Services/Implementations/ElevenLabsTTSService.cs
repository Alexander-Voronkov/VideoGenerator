using ElevenLabs;
using ElevenLabs.TextToSpeech;
using ElevenLabs.Voices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoGenerator.Configs;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class ElevenLabsTtsService: ITextToSpeechService
{
    private readonly ElevenLabsClient _elevenLabsClient;
    private readonly ElevenLabsConfig _elevenLabsConfig;
    private readonly ILogger<ElevenLabsTtsService> _logger;

    public ElevenLabsTtsService(IOptions<ElevenLabsConfig> elevenLabsConfig,  ILogger<ElevenLabsTtsService> logger)
    {
        _elevenLabsConfig = elevenLabsConfig.Value;
        _elevenLabsClient = new ElevenLabsClient(elevenLabsConfig.Value.ApiKey);
        _logger = logger;
    }
    
    public async Task<TtsResult> CreateTextToSpeech(string text, string language)
    {
        if (!_elevenLabsConfig.Voices.TryGetValue(language, out var voiceId))
        {
            _logger.LogError($"Voice not found for language {language}");
            throw new Exception($"Voice not found for language {language}");
        }
        
        var voice = new Voice(voiceId, "");
        var request = new TextToSpeechRequest(voice, text, withTimestamps:  true);
        
        var result = await _elevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request);
        
        return new TtsResult(result.ClipData.ToArray(), result.TimestampedTranscriptCharacters);
    }
}