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
    public int ToneId { get; set; }
    public Tone Preferred { get; set; }
    //public Tone Alternative { get; }

    public Song() { }

    public Song(string toneNote, bool isMajorPreferred)
    {
        Tone major = new Tone(toneNote, true);
        Tone minor = new Tone(toneNote, false);

        if (isMajorPreferred)
        {
            Preferred = major;
            //Alternative = minor;
        }
        else
        {
            Preferred = minor;
            //Alternative = major;
        }
    }
}
