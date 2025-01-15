using mixdrop_back.Models;
using mixdrop_back.Repositories.Base;

namespace mixdrop_back.Repositories;

public class StateRepository : Repository<State, int>
{

    public StateRepository(MixDropContext context) : base(context) { }

}

