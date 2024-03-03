using System.Globalization;

namespace VideoGenerator.Services.Interfaces;

public interface ISpeechSynthesizerService
{
    Task SynthesizeToStream(string text, Stream destination, CultureInfo culture = default);
    Task SynthesizeToFile(string text, string destination, CultureInfo culture = default);
}
