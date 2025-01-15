namespace mixdrop_back.Models.Entities;

public class CartType
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
