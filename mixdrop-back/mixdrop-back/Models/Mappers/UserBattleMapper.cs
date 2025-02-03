using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.Mappers;

public class UserBattleMapper
{
    //Pasar de usuario a dto
    public UserBattleDto ToDto(UserBattle user)
    {
        return new UserBattleDto
        {
            Cards = user.Cards,
            IsTheirTurn = user.IsTheirTurn,
        };
    }
}
