using Microsoft.EntityFrameworkCore;
using mixdrop_back.Models.Entities;
using mixdrop_back.Repositories.Base;

namespace mixdrop_back.Repositories;

public class UserBattleRepository : Repository<UserBattle, int>
{
    public UserBattleRepository(MixDropContext context) : base(context) { }
}

