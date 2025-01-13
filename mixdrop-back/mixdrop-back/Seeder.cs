using mixdrop_back.Helper;
using mixdrop_back.Models;



namespace mixdrop_back;

public class Seeder
{
    private readonly MixDropContext _context;

    public Seeder(MixDropContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {

        await SeedStateAsync();
        await _context.SaveChangesAsync();

        await SeedUsersAsync();
        await _context.SaveChangesAsync();
    }


    private async Task SeedUsersAsync()
    {
        User[] users = [
                new User {
                    Nickname = "zero" ,
                    Email = "maria@gmail.com",
                    Password = PasswordHelper.Hash("123456"),
                    AvatarPath = "avatar/zero.webp",
                    Role = "Admin",
                    StateId = 1
                },
                new User {
                    Nickname = "shio" ,
                    Email = "rocio@gmail.com",
                    Password = PasswordHelper.Hash("123456"),
                    AvatarPath = "avatar/shio.webp",
                    Role = "Admin",
                    StateId = 1
                },
                new User {
                    Nickname = "moguism" ,
                    Email = "mauricio@gmail.com",
                    Password = PasswordHelper.Hash("123456"),
                    AvatarPath = "avatar/moguism.webp",
                    Role = "Admin",
                    StateId = 1
                }
            ];

        await _context.Users.AddRangeAsync(users);
    }


    private async Task SeedStateAsync()
    {
        State[] states = [
                new State {
                    Name = "Desconectado"
                },
                new State {
                    Name = "Conectado"
                },
                new State {
                    Name = "Jugando"
                }
            ];

        await _context.States.AddRangeAsync(states);
    }

}
