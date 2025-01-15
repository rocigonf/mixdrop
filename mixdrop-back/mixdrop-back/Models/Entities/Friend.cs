namespace mixdrop_back.Models.Entities;

public class Friend
{
    public int Id { get; set; }
    public ICollection<UserFriend> UserFriends { get; set; }
}
