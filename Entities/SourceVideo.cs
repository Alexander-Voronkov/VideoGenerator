namespace VideoGenerator.Entities;

public class SourceVideo
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string Path { get; set; }
	public TimeSpan Duration { get; set; }
	public DateTime AddedAt { get; set; } = DateTime.UtcNow;

	public List<ProcessedVideo> ProcessedVideos { get; set; } = [];
}
