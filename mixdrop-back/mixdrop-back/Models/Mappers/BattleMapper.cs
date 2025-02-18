using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.Mappers;

public class BattleMapper
{
    public IEnumerable<BattleDto> ToDto(ICollection<Battle> battles)
    {
        List<BattleDto> battleDtos = new List<BattleDto>();
        foreach (var battle in battles)
        {
            var user = battle.BattleUsers.FirstOrDefault();
            user.User.Password = null;
            //user.User.BattleUsers = null;

            BattleDto battleDto = new BattleDto();
            battleDto.Id = battle.Id;
            battleDto.BattleStateId = battle.BattleStateId;
            battleDto.User = user.User;
            battleDto.UserId = user.Id;

            battleDtos.Add(battleDto);

        }
        return battleDtos;
    }

    public List<BattleDto> ToDtoWithAllInfo(ICollection<Battle> battles)
    {
        List<BattleDto> battleDtos = new List<BattleDto>();
        UserBattleMapper userBattleMapper = new UserBattleMapper();
        foreach (var battle in battles)
        {
            BattleDto battleDto = new BattleDto();
            battleDto.Id = battle.Id;
            battleDto.IsAgainstBot = battle.IsAgainstBot;
            battleDto.BattleStateId = battle.BattleStateId;
            battleDto.CreatedAt = battle.CreatedAt;
            battleDto.FinishedAt = battle.FinishedAt;

            var users = userBattleMapper.ToDto(battle.BattleUsers);
            battleDto.UsersBattles = users;

            battleDtos.Add(battleDto);

        }
        return battleDtos;
    }
}
