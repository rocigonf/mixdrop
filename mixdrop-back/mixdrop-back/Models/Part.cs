namespace mixdrop_back.Models;

public class Part
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
