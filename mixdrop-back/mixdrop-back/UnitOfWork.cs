using mixdrop_back.Repositories;

namespace mixdrop_back;

public class UnitOfWork
{
    private readonly MixDropContext _context;


    private UserRepository _userRepository;

    // poner todos los repositorys




    public UserRepository UserRepository => _userRepository ??= new UserRepository(_context);
   


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
