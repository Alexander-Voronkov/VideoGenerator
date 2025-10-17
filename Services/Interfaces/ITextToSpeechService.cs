using ElevenLabs;
using VideoGenerator.Enums;

namespace VideoGenerator.Services.Interfaces;

public record TtsResult(byte[] Audio, TimestampedTranscriptCharacter[] Timestamps);
public interface ITextToSpeechService
{
    Task<TtsResult> CreateTextToSpeech(string text, SexType sexType, string language);
}