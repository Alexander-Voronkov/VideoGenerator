using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevenLabs;

namespace VideoGenerator.Services.Interfaces;

public record TimestampedLine(string Line, TimeSpan Start, TimeSpan End);

public interface IAssConvertService
{
    string ConvertFromCrt(string[] crtSubtitles);
    
    string ConvertFromTimestampedTranscript(TimestampedTranscriptCharacter[] characters, int maxCharactersPerLine = 25, bool enableHighlight = true);
    
    string GenerateFromTimestampedLines(TimestampedLine[] lines, int? fontSize = null, int? position = null);
}
