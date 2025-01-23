namespace mixdrop_back.Models.Entities;

public class Friendship
{
    public int Id { get; set; }
    public bool Accepted { get; set; } = false;
    public User SenderUser { get; set; }
    public User ReceiverUser { get; set; }

    public int SenderUserId { get; set; }
    public int ReceiverUserId { get; set; }
}
