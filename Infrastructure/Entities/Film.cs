namespace VideoGenerator.Infrastructure.Entities;

public class Film
{
    public int FilmId { get; set; }
    public string Name { get; set; }
    public int SeasonSeriesNumber { get; set; }
    public int SeasonNumber { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime PublishDate { get; set; }
    public byte[] Content { get; set; }
    public string MimeType { get; set; }
    public string Language { get; set; }
}