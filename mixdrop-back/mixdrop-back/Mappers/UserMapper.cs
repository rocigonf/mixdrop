﻿using mixdrop_back.DTOs;
using mixdrop_back.Models;

namespace mixdrop_back.Mappers;

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
            StateId = user.StateId,
            UserFriends = user.UserFriends,
            BattleUsers = user.BattleUsers
        };
    }

    //Pasar la lista de usuarios a dtos
    public IEnumerable<UserDto> ToDto(IEnumerable<User> users)
    {
        return users.Select(ToDto);
    }


}
