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
            $"PlayResX: {_config.PlayResX}\n" +
            $"PlayResY: {_config.PlayResY}\n" +
            "ScaledBorderAndShadow: yes\n\n" +
            "[V4+ Styles]\n" +
            "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, " +
            "Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, " +
            "Alignment, MarginL, MarginR, MarginV, Encoding\n" +
            $"{_config.PrimaryStyle}\n" +
            $"{_config.HighlightStyle}\n\n" +
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

        // First, extract words with their timing information
        var words = ExtractWordsFromCharacters(chars);
        if (words.Count == 0) return Array.Empty<string>();

        var result = new List<string>();
        var currentLineWords = new List<(string word, double startTime, double endTime)>();
        var currentLineLength = 0;
        var previousLineContent = "";

        for (int i = 0; i < words.Count; i++)
        {
            var word = words[i];
            
            // Calculate the length if we add this word (including space if not first word)
            var wordLength = word.word.Length;
            var spaceLength = currentLineWords.Count > 0 ? 1 : 0; // space before word
            var totalLengthWithWord = currentLineLength + spaceLength + wordLength;
            
            // Check if adding this word would exceed the line limit
            if (totalLengthWithWord > maxLineLength && currentLineWords.Count > 0)
            {
                // Process current line before adding new word
                var lineContent = BuildLineContent(currentLineWords);
                var isNewLineContent = lineContent.Trim() != previousLineContent.Trim();
                
                CreateSubtitleEventsForLine(result, currentLineWords, lineContent, isNewLineContent, words, i);
                
                previousLineContent = lineContent;
                
                // Start new line with current word
                currentLineWords.Clear();
                currentLineWords.Add(word);
                currentLineLength = wordLength;
            }
            else
            {
                // Add word to current line
                currentLineWords.Add(word);
                currentLineLength = totalLengthWithWord;
            }
        }

        // Handle the last line
        if (currentLineWords.Count > 0)
        {
            var lineContent = BuildLineContent(currentLineWords);
            var isNewLineContent = lineContent.Trim() != previousLineContent.Trim();
            CreateSubtitleEventsForLine(result, currentLineWords, lineContent, isNewLineContent, words, words.Count);
        }

        return result.ToArray();
    }

    private string BuildLineContent(List<(string word, double startTime, double endTime)> words)
    {
        return string.Join(" ", words.Select(w => w.word));
    }

    private List<(string word, double startTime, double endTime)> ExtractWordsFromCharacters(TimestampedTranscriptCharacter[] chars)
    {
        var words = new List<(string word, double startTime, double endTime)>();
        var currentWordChars = new List<TimestampedTranscriptCharacter>();

        for (int i = 0; i < chars.Length; i++)
        {
            var currentChar = chars[i];
            
            if (char.IsWhiteSpace(currentChar.Character[0]))
            {
                // Complete current word if we have characters
                if (currentWordChars.Count > 0)
                {
                    var word = string.Join("", currentWordChars.Select(c => c.Character));
                    var startTime = currentWordChars[0].StartTime;
                    var endTime = currentWordChars[^1].EndTime;
                    words.Add((word, startTime, endTime));
                    currentWordChars.Clear();
                }
                // Skip the whitespace character (don't add it as a word)
            }
            else
            {
                currentWordChars.Add(currentChar);
            }
        }

        // Handle the last word
        if (currentWordChars.Count > 0)
        {
            var word = string.Join("", currentWordChars.Select(c => c.Character));
            var startTime = currentWordChars[0].StartTime;
            var endTime = currentWordChars[^1].EndTime;
            words.Add((word, startTime, endTime));
        }

        return words;
    }

    private void CreateSubtitleEventsForLine(List<string> result, 
                                           List<(string word, double startTime, double endTime)> words, 
                                           string lineText, 
                                           bool isNewLine,
                                           List<(string word, double startTime, double endTime)> allWords,
                                           int nextWordIndex)
    {
        if (words.Count == 0) return;

        for (int i = 0; i < words.Count; i++)
        {
            var word = words[i];
            
            // Create text with current word highlighted
            var highlightedText = HighlightSpecificWord(lineText, word.word, words);
            
            // Calculate end time - should be start of next word or end of current word
            double endTime;
            if (i < words.Count - 1)
            {
                // End when next word in same line starts
                endTime = words[i + 1].startTime;
            }
            else if (nextWordIndex < allWords.Count)
            {
                // Last word in line - end when next word in next line starts
                endTime = allWords[nextWordIndex].startTime;
            }
            else
            {
                // Very last word - use its natural end time
                endTime = word.endTime;
            }
            
            // Create dialogue event for this word's duration
            var start = FormatTime(word.startTime);
            var end = FormatTime(endTime);
            
            // Only apply animation to the first word of a truly new line (new content)
            // and only if it's the first word being processed in that line
            var shouldAnimate = isNewLine && i == 0;
            var wordAnimation = shouldAnimate ? _config.Animation : "";
            
            result.Add($"Dialogue: 0,{start},{end},Default,,0,0,0,,{wordAnimation}{highlightedText}");
        }
    }

    private string HighlightSpecificWord(string lineText, string currentWord, 
                                        List<(string word, double startTime, double endTime)> allWords)
    {
        var words = lineText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var highlightedWords = new List<string>();
        
        // Create a set of currently speaking words for faster lookup
        var currentWords = new HashSet<string> { currentWord.Trim() };
        
        foreach (var word in words)
        {
            if (currentWords.Contains(word.Trim()))
            {
                highlightedWords.Add($"{{\\rHighlight}}{word}{{\\r}}");
            }
            else
            {
                highlightedWords.Add(word);
            }
        }
        
        return string.Join(" ", highlightedWords);
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
        // Ensure proper centisecond precision (ASS format uses centiseconds, not milliseconds)
        var centiseconds = (int)Math.Round((seconds - Math.Floor(seconds)) * 100);
        return $"{(int)ts.TotalHours}:{ts.Minutes:00}:{ts.Seconds:00}.{centiseconds:00}";
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
