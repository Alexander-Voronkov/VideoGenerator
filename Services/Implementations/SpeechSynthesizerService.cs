using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class SpeechSynthesizerService : ISpeechSynthesizerService
{
    private readonly SpeechSynthesizer _speechSynthesizer;
    public SpeechSynthesizerService()
    {
        _speechSynthesizer = new();
    }

    public async Task Synthesize(string text)
    {
        _speechSynthesizer.
        _speechSynthesizer.SpeakAsync(text);
    }
}