namespace mixdrop_back.Models.Entities;

public class Track
{
    public int Id { get; set; }
    public int SongId { get; set; }
    public int PartId { get; set; }
    public Song Song { get; set; }
    public Part Part { get; set; }
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
