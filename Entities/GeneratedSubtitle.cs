namespace VideoGenerator.Entities;

public class GeneratedSubtitle
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string BlobPath { get; set; }
	public TimeSpan GenerationDuration { get; set; }
	public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

	public string GeneratedHistoryId { get; set; }
	public GeneratedHistory GeneratedHistory { get; set; }

	public string ProcessedVideoId { get; set; }
	public ProcessedVideo ProcessedVideo { get; set; }
}
