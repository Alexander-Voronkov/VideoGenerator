namespace VideoGenerator.Entities;

public class ProcessedVideo
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public TimeSpan Duration { get; set; }
	public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
	public TimeSpan ProcessingDuration { get; set; }

	public string SourceVideoId { get; set; }
	public SourceVideo SourceVideo { get; set; }

	public string GeneratedSubtitleId { get; set; }
	public GeneratedSubtitle GeneratedSubtitle { get; set; }
}
