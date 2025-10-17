using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Configs;

public class VoiceConfig
{
	public Dictionary<string, string> Male { get; set; }
	public Dictionary<string, string> Female { get; set; }
}

public class ElevenLabsConfig
{
	public string ApiKey { get; set; }
	
	public VoiceConfig Voices { get; set; } = new VoiceConfig();
	
	public float SpeedMultiplier { get; set; }
	
}
