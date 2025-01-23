namespace mixdrop_back.Models.Entities;

public class Friendship
{
    public int Id { get; set; }
    public bool Accepted { get; set; } = false;
    public User User1 { get; set; }
    public User User2 { get; set; }

    public int User1Id { get; set; }
    public int User2Id { get; set; }

    public int ReceiverId { get; set; }
}
