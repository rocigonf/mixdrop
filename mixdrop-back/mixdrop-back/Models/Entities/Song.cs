namespace mixdrop_back.Models.Entities;

public class Song
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Bpm { get; set; }
    public string Pitch { get; set; }
    public int ArtistId { get; set; }
    public Artist Artist { get; set; }
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
