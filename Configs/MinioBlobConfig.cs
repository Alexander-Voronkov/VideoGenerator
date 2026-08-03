namespace VideoGenerator.Configs;

public class MinioBlobConfig
{
	public string Host { get; set; } = default!;
	public string AccessKey { get; set; } = default!;
	public string SecretKey { get; set; } = default!;
	public bool Ssl { get; set; }
}
