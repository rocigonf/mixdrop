namespace mixdrop_back.Models.Entities;

public class Cart
{
    public int Id { get; set; }
    public string ImagePath { get; set; }
    public int Level { get; set; }
    public int TrackId { get; set; }
    public int CartTypeId { get; set; }
    public int CollectionId { get; set; }
    public Track Track { get; set; }
    public CartType CartType { get; set; }
    public Collection Collection { get; set; }
    public ICollection<BattleCart> BattleCarts { get; set; }
}
