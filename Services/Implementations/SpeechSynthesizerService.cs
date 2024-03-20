using System.Globalization;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class SpeechSynthesizerService : ISpeechSynthesizerService
{
    public SpeechSynthesizerService()
    {
    }

    public Task SynthesizeToStream(string text, Stream destination, CultureInfo culture = default)
    {
        return Task.CompletedTask;
    }

    public Task SynthesizeToFile(string text, string destination, CultureInfo culture = default)
    {
        return Task.CompletedTask;
    }
}