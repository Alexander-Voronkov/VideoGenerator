namespace VideoGenerator.Entities;

public class SplitHistory
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string ParentBlobPath { get; set; }
	public string BlobPath { get; set; }
	public TimeSpan Duration { get; set; }
	public DateTime? LastTookPartAt { get; set; }
}
