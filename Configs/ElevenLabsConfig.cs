namespace VideoGenerator.Configs;

public class VoiceConfig
{
	public Dictionary<string, string> Male { get; set; }
	public Dictionary<string, string> Female { get; set; }
}

public class ElevenLabsConfig
{
	public string[] ApiKeys { get; set; }
	
	public VoiceConfig Voices { get; set; } = new VoiceConfig();
	
	public float SpeedMultiplier { get; set; }
	
	public float Stability { get; set; }
	
	public float Clarity { get; set; }

	public float Style { get; set; }
}
