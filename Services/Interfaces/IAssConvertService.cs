using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevenLabs;

namespace VideoGenerator.Services.Interfaces;
public interface IAssConvertService
{
    string ConvertFromCrt(string[] crtSubtitles);
    
    string ConvertFromTimestampedTranscript(TimestampedTranscriptCharacter[] characters, int maxCharactersPerLine = 25);
}
