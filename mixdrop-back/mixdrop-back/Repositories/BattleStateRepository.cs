using mixdrop_back.Models.Entities;
using mixdrop_back.Repositories.Base;

namespace mixdrop_back.Repositories;

public class BattleStateRepository : Repository<BattleState, int>
{
    public BattleStateRepository(MixDropContext context) : base(context) { }
}
