namespace mixdrop_back.Models.Entities;

public class Friendship
{
    public int Id { get; set; }
    public bool Accepted { get; set; } = false;
    public ICollection<UserFriend> UserFriends { get; set; }
}
