using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.DTOs;

// sin contraseña
public class UserDto
{
    public int Id { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string AvatarPath { get; set; }
    public bool IsInQueue { get; set; }
    public int StateId { get; set; }
    public State State { get; set; }
    public ICollection<UserFriend> UserFriends { get; set; }
    public ICollection<UserBattle> BattleUsers { get; set; } = new List<UserBattle>();
}
