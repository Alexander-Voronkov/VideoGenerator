using Microsoft.Extensions.Options;
using System.Text;
using VideoGenerator.Configurations;

namespace VideoGenerator.Services.Interfaces;
public class AssConvertService: IAssConvertService
{
    private readonly SubtitlesConfig _subtitlesConfig;

    public AssConvertService(IOptions<SubtitlesConfig> options)
    {
        _subtitlesConfig = options.Value;
    }

    public string ConvertFromCrt(string[] crtSubtitles)
    {
        var style = "[Script Info]\n" +
            "ScriptType: v4.00+\n" +
            "PlayResX: 384\n" +
            "PlayResY: 288\n" +
            "ScaledBorderAndShadow: yes\n" +
            "\n" +
            "[V4+ Styles]\n" +
            "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n" +
            $"Style: Default,{_subtitlesConfig.PrimaryStyle}\n" +
            $"Style: Highlight,{_subtitlesConfig.HighlightStyle}\n" +
            "\n" +
            "[Events]\n" +
            "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n";

        var eventLines = SrtToAssEvents(crtSubtitles);

        return style + string.Join("\n", eventLines);
    }

    private string[] SrtToAssEvents(string[] srtLines)
    {
        var events = new List<string>();
        int i = 0;

        while (i < srtLines.Length)
        {
            if (string.IsNullOrWhiteSpace(srtLines[i])) { i++; continue; }
            i++;

            var timeParts = srtLines[i++].Split(new[] { " --> " }, StringSplitOptions.None);
            var start = timeParts[0].Replace(",", ".").Substring(0, timeParts[0].Length - 1);
            var end = timeParts[1].Replace(",", ".").Substring(0, timeParts[0].Length - 1);

            var sb = new StringBuilder();
            while (i < srtLines.Length && !string.IsNullOrWhiteSpace(srtLines[i]))
            {
                sb.Append(srtLines[i++] + "\\N");
            }

            var text = sb.ToString().TrimEnd('\\', 'N');

            var words = text.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 0)
            {
                int idx = Random.Shared.Next(words.Length);
                words[idx] = $"{{\\rHighlight}}{words[idx]}{{\\r}}";
                text = string.Join(" ", [_subtitlesConfig.Animation, ..words]);
            }

            events.Add($"Dialogue: 0,{start},{end},Default,,0,0,0,,{text}");
        }

        return events.ToArray();
    }
}
