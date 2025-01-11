namespace mixdrop_back.Models;

public class Song
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Bpm { get; set; }
    public double Pitch { get; set; }
    public int ArtistId { get; set; }
    public Artist Artist { get; set; }
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
