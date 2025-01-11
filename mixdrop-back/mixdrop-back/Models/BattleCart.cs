namespace mixdrop_back.Models;

public class BattleCart
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int BattleId { get; set; }
    public Cart Cart { get; set; }
    public Battle Battle { get; set; }
}
