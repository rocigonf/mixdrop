using mixdrop_back.Repositories;

namespace mixdrop_back;

public class UnitOfWork
{
    private readonly MixDropContext _context;
    private UserRepository _userRepository;
    private StateRepository _stateRepository;
    private FriendshipRepository _friendshipRepository;
    //private UserFriendRepository _userFriendRepository;
    private BattleRepository _battleRepository;
    private UserBattleRepository _userBattleRepository;
    private BattleStateRepository _battleStateRepository;
    private CardRepository _cardRepository;
    private BattleResultRepository _battleResultRepository;

    // poner todos los repositorys

    public UserRepository UserRepository => _userRepository ??= new UserRepository(_context);
    public StateRepository StateRepositoty => _stateRepository ??= new StateRepository(_context);
    public FriendshipRepository FriendshipRepository => _friendshipRepository ??= new FriendshipRepository(_context);
    //public UserFriendRepository UserFriendRepository => _userFriendRepository ??= new UserFriendRepository(_context);
    public BattleRepository BattleRepository => _battleRepository ??= new BattleRepository(_context);
    public UserBattleRepository UserBattleRepository => _userBattleRepository ??= new UserBattleRepository(_context);
    public BattleStateRepository BattleStateRepository => _battleStateRepository ??= new BattleStateRepository(_context);
    public CardRepository CardRepository => _cardRepository ??= new CardRepository(_context);
    public BattleResultRepository BattleResultRepository => _battleResultRepository ??= new BattleResultRepository(_context);

    // poner todos los repositorys

    public UnitOfWork(MixDropContext context)
    {
        _context = context;
    }

    public MixDropContext Context => _context;

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
