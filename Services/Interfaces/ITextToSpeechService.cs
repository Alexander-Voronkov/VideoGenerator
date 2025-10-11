using ElevenLabs;

namespace VideoGenerator.Services.Interfaces;

public record TtsResult(byte[] Audio, TimestampedTranscriptCharacter[] Timestamps);
public interface ITextToSpeechService
{
    Task<TtsResult> CreateTextToSpeech(string text, string language);
}