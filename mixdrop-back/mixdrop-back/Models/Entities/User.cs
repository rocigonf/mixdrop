using Microsoft.EntityFrameworkCore;

namespace mixdrop_back.Models.Entities;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Nickname), IsUnique = true)]
public class User
{
    public int Id { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public string AvatarPath { get; set; }
    public bool IsInQueue { get; set; }
    public bool Banned { get; set; } = false;
    public int StateId { get; set; }
    public State State { get; set; }
    public ICollection<UserFriend> UserFriends { get; set; } = new List<UserFriend>();
    public ICollection<UserBattle> BattleUsers { get; set; } = new List<UserBattle>();
}