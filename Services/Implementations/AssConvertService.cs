using Microsoft.Extensions.Options;
using System.Text;
using System.Text.RegularExpressions;
using ElevenLabs;
using VideoGenerator.Configurations;

namespace VideoGenerator.Services.Interfaces;

public class AssConvertService : IAssConvertService
{
    private readonly SubtitlesConfig _config;

    public AssConvertService(IOptions<SubtitlesConfig> options)
    {
        _config = options.Value;
    }

    public string ConvertFromCrt(string[] crtSubtitles)
    {
        var header = BuildAssHeader();
        var events = ConvertSrtToAss(crtSubtitles);
        return header + string.Join("\n", events);
    }

    public string ConvertFromTimestampedTranscript(TimestampedTranscriptCharacter[] chars, int maxLineLength)
    {
        var header = BuildAssHeader();
        var events = ConvertTimestamped(chars, maxLineLength);
        return header + string.Join("\n", events);
    }

    private string BuildAssHeader()
    {
        return
            "[Script Info]\n" +
            "ScriptType: v4.00+\n" +
            "PlayResX: 384\n" +
            "PlayResY: 288\n" +
            "ScaledBorderAndShadow: yes\n\n" +
            "[V4+ Styles]\n" +
            "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, " +
            "Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, " +
            "Alignment, MarginL, MarginR, MarginV, Encoding\n" +
            $"Style: Default,{_config.PrimaryStyle}\n" +
            $"Style: Highlight,{_config.HighlightStyle}\n\n" +
            "[Events]\n" +
            "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n";
    }

    private string[] ConvertSrtToAss(string[] lines)
    {
        var result = new List<string>();
        int i = 0;

        while (i < lines.Length)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) { i++; continue; }

            // skip index line
            i++;

            if (i >= lines.Length) break;
            var times = lines[i++].Split(" --> ", StringSplitOptions.None);
            var start = NormalizeTime(times[0]);
            var end = NormalizeTime(times[1]);

            var sb = new StringBuilder();
            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                sb.Append(lines[i++] + "\\N");

            var text = sb.ToString().TrimEnd('\\', 'N');
            text = ApplyHighlight(text);

            result.Add($"Dialogue: 0,{start},{end},Default,,0,0,0,,{text}");
        }

        return result.ToArray();
    }

    private string[] ConvertTimestamped(TimestampedTranscriptCharacter[] chars, int maxLineLength)
    {
        if (chars == null || chars.Length == 0)
            return Array.Empty<string>();

        var events = new List<string>();
        var lineBuffer = new List<TimestampedTranscriptCharacter>();
        int currentLength = 0;

        for (int i = 0; i < chars.Length; i++)
        {
            var ch = chars[i];
            lineBuffer.Add(ch);
            currentLength++;

            bool isLast = i == chars.Length - 1;
            bool isBreakPoint = ch.Character == " " && currentLength >= maxLineLength;

            if (isBreakPoint || isLast)
            {
                ProcessTimestampedLine(lineBuffer, events);
                lineBuffer.Clear();
                currentLength = 0;
            }
        }

        return events.ToArray();
    }
    
    void ProcessTimestampedLine(List<TimestampedTranscriptCharacter> lineChars, List<string> output)
    {
        if (lineChars.Count == 0)
            return;

        // Build clean text from characters
        string text = string.Concat(lineChars.Select(c => c.Character));
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        if (string.IsNullOrWhiteSpace(text))
            return;

        // Parse words with timing
        var words = new List<(string Word, double Start, double End)>();
        var currentWord = new StringBuilder();
        double cStart = -1;
        double cEnd = -1;

        for (int i = 0; i < lineChars.Count; i++)
        {
            var c = lineChars[i];
            var ch = c.Character[0];
            bool isAlphaNum = char.IsLetterOrDigit(ch);

            if (isAlphaNum)
            {
                if (currentWord.Length == 0)
                    cStart = c.StartTime;

                currentWord.Append(ch);
                cEnd = c.EndTime;
            }

            bool wordEnded = (!isAlphaNum && currentWord.Length > 0)
                             || (i == lineChars.Count - 1 && currentWord.Length > 0);

            if (wordEnded)
            {
                var word = currentWord.ToString();
                currentWord.Clear();

                // Ignore non-alphanumeric "words" (punctuation-only)
                if (word.Any(char.IsLetterOrDigit))
                    words.Add((word, cStart, cEnd));
            }
        }

        if (words.Count == 0)
            return;

        // Generate highlight events for each word in the line
        for (int wIndex = 0; wIndex < words.Count; wIndex++)
        {
            var (word, start, end) = words[wIndex];
            var splitWords = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Highlight *nth* occurrence of the current word
            int occurrence = 0;
            for (int i = 0; i < splitWords.Length; i++)
            {
                if (string.Equals(splitWords[i].TrimEnd('.', ',', '!', '?', ':', ';', '”', '“'),
                                  word, StringComparison.OrdinalIgnoreCase))
                {
                    if (occurrence == 0)
                    {
                        splitWords[i] = $"{{\\rHighlight}}{splitWords[i]}{{\\r}}";
                        occurrence++;
                    }
                }
            }

            string lineText = string.Join(" ", splitWords);
            string dialogue = $"Dialogue: 0,{FormatTime(start)},{FormatTime(end)},Default,,0,0,0,,{_config.Animation} {lineText}";
            output.Add(dialogue);
        }
    }
    
    private static string NormalizeTime(string input)
    {
        input = input.Trim().Replace(",", ".");
        if (input.Length > 10) input = input[..10];
        return input;
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{(int)ts.TotalHours}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
    }

    private static string ApplyHighlight(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return text;

        int index = Random.Shared.Next(words.Length);
        words[index] = $"{{\\rHighlight}}{words[index]}{{\\r}}";
        return string.Join(" ", words);
    }
}
