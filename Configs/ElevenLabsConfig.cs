using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Configs;
public class ElevenLabsConfig
{
	public string ApiKey { get; set; }
	
	public Dictionary<string, string> Voices { get; set; }
}
