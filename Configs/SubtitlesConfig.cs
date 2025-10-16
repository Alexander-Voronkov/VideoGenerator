namespace VideoGenerator.Configurations;
public class SubtitlesConfig
{
    public string PrimaryStyle { get; set; }

    public string HighlightStyle { get; set; }

    public string Animation { get; set; }
    
    public int PlayResX { get; set; } = 1080;
    
    public int PlayResY { get; set; } = 1920;
}
