using mixdrop_back.Repositories;

namespace mixdrop_back;

public class UnitOfWork
{
    private readonly MixDropContext _context;
    private UserRepository _userRepository;
    private StateRepository _stateRepository;
    private FriendshipRepository _friendshipRepository;
    private UserFriendRepository _userFriendRepository;

    // poner todos los repositorys

    public UserRepository UserRepository => _userRepository ??= new UserRepository(_context);
    public StateRepository StateRepositoty => _stateRepository ??= new StateRepository(_context);
    public FriendshipRepository FriendshipRepository => _friendshipRepository ??= new FriendshipRepository(_context);
    public UserFriendRepository UserFriendRepository => _userFriendRepository ??= new UserFriendRepository(_context);

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
