using ElevenLabs;
using ElevenLabs.Models;
using ElevenLabs.TextToSpeech;
using ElevenLabs.Voices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System.Text;
using VideoGenerator.Configs;
using VideoGenerator.Enums;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class ElevenLabsTtsService: ITextToSpeechService
{
    private ElevenLabsClient _elevenLabsClient;
    private readonly ElevenLabsConfig _elevenLabsConfig;
    private readonly ILogger _logger;
	private int _currentKeyIndex = 0;

	public ElevenLabsTtsService(
        IOptions<ElevenLabsConfig> elevenLabsConfig,  
        ILogger<ElevenLabsTtsService> logger)
    {
        _elevenLabsConfig = elevenLabsConfig.Value;
        _elevenLabsClient = new ElevenLabsClient(_elevenLabsConfig.ApiKeys.First());
        _logger = logger;
    }
    
    public async Task<TtsResult> CreateTextToSpeech(string text, SexType sexType, string language)
    {
        var voices = sexType == SexType.Female 
            ? _elevenLabsConfig.Voices.Female 
            : _elevenLabsConfig.Voices.Male;
        
        if (!voices.TryGetValue(language, out var voiceId))
        {
            _logger.LogError("Voice not found for language {Language} and {Sex}", language, sexType.ToString());
            throw new Exception($"Voice not found for language {language} - {sexType.ToString()}");
        }
        
        var voice = new Voice(voiceId, "");
		var request = new TextToSpeechRequest(voice, PrepareText(text), withTimestamps: true, model: Model.MultiLingualV2, voiceSettings: new VoiceSettings(speed: _elevenLabsConfig.SpeedMultiplier));

		var retryPolicy = Policy
		    .Handle<Exception>()
		    .RetryAsync(
			    retryCount: _elevenLabsConfig.ApiKeys.Length - 1,
			    onRetryAsync: async (exception, retryCount, context) =>
			    {
				    Console.WriteLine($"❗ Ошибка: {exception.Message}, попытка {retryCount}");

				    _currentKeyIndex = retryCount % _elevenLabsConfig.ApiKeys.Length;

				    var newApiKey = _elevenLabsConfig.ApiKeys[_currentKeyIndex];
				    _elevenLabsClient = new ElevenLabsClient(newApiKey);

				    await Task.CompletedTask;
			    });

		var result = await retryPolicy.ExecuteAsync(async () =>
		{
			return await _elevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request);
		});

		return new TtsResult(result.ClipData.ToArray(), result.TimestampedTranscriptCharacters);
    }

    private string PrepareText(string rawText)
    {
        var sb = new StringBuilder(rawText);

        sb.Replace("--", "-");
        sb.Replace("/n/n", " ");
        sb.Replace("/n", " ");

        return sb.ToString();
    }
}