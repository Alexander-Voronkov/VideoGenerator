namespace VideoGenerator.Entities;

public class GeneratedHistory
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string RawMetadata { get; set; }
	public string ClearMetadata { get; set; }
	public TimeSpan GenerationDuration { get; set; }
	public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

	public string GeneratedSubtitleId { get; set; }
	public GeneratedSubtitle GeneratedSubtitle { get; set; }
}
