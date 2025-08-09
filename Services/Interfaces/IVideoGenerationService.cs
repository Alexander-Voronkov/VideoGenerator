using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGenerator.Services.Interfaces;
public interface IVideoGenerationService
{
    Task CreateVideo(string audioPath, string subtitlePath, string backgroundVideoPath, string outputPath);
}
