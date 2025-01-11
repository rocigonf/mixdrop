namespace mixdrop_back.Models;

public class Collection
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
