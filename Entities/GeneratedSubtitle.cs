namespace VideoGenerator.Entities;

public class GeneratedSubtitle
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string TtsBlobPath { get; set; }
	public string AssBlobPath { get; set; }
	public string Timestamps { get; set; }
}
