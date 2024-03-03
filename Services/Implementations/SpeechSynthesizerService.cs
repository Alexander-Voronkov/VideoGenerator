using System.Globalization;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
public class SpeechSynthesizerService : ISpeechSynthesizerService
{
    private readonly SpeechSynthesizer _speechSynthesizerService;

    public SpeechSynthesizerService()
    {
        _speechSynthesizerService = new();
    }

    public Task SynthesizeToStream(string text, Stream destination, CultureInfo culture = default)
    {
        var voices = _speechSynthesizerService.GetInstalledVoices(culture);
        _speechSynthesizerService.SelectVoice(voices[Random.Shared.Next(0, voices.Count - 1)].VoiceInfo.Name);
        _speechSynthesizerService.SetOutputToAudioStream(destination, new System.Speech.AudioFormat.SpeechAudioFormatInfo(48000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
        _speechSynthesizerService.Speak(text);
        return Task.CompletedTask;
    }

    public Task SynthesizeToFile(string text, string destination, CultureInfo culture = default)
    {
        var voices = _speechSynthesizerService.GetInstalledVoices(culture);
        _speechSynthesizerService.SelectVoice(voices[Random.Shared.Next(0, voices.Count - 1)].VoiceInfo.Name);

        if (Path.HasExtension(destination) && Path.GetExtension(destination) != ".wav")
        {
            destination = Path.ChangeExtension(destination, ".wav");
        }

        _speechSynthesizerService.SetOutputToWaveFile(destination);
        _speechSynthesizerService.Speak(text);
        return Task.CompletedTask;
    }
}