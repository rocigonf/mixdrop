using mixdrop_back.Models.Entities;
using mixdrop_back.Repositories.Base;

namespace mixdrop_back.Repositories;

public class BattleResultRepository : Repository<BattleResult, int>
{
    public BattleResultRepository(MixDropContext context) : base(context) { }
}
