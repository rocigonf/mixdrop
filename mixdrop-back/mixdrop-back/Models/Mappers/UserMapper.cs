using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.Mappers;

public class UserMapper
{
    //Pasar de usuario a dto
    public UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Email = user.Email,
            Role = user.Role,
            AvatarPath = user.AvatarPath,
            IsInQueue = user.IsInQueue,
            Banned = user.Banned,
            StateId = user.StateId,
            Friendships = user.Friendships,
            //BattleUsers = user.BattleUsers
        };
    }

    //Pasar la lista de usuarios a dtos
    public IEnumerable<UserDto> ToDto(IEnumerable<User> users)
    {
        return users.Select(ToDto);
    }


}
