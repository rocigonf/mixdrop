using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Helper;
using System.Reflection.Emit;
using System.Xml.Linq;



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

        await SeedBattleResultAsync();
        await _context.SaveChangesAsync();

        await SeedBattleStateAsync();
        await _context.SaveChangesAsync();

        await SeedPartsAsync();
        await _context.SaveChangesAsync();

        await SeedCardTypeAsync();
        await _context.SaveChangesAsync();

        await SeedCardsAsync();
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

    private async Task SeedBattleResultAsync()
    {
        BattleResult[] results = [
                new BattleResult {
                    Name = "Victoria"
                },
                new BattleResult {
                    Name = "Derrota"
                },
                new BattleResult {
                    Name = "Empate"
                }
            ];

        await _context.BattleResults.AddRangeAsync(results);
    }

    private async Task SeedBattleStateAsync()
    {
        BattleState[] battleStates = [
            new BattleState {
                Name = "Pendiente de aceptar", // No está aceptado aún
            },
            new BattleState {
                Name = "Pendiente de empezar", // Está aceptado pero no está jugando aún
            },
            new BattleState {
                Name = "Jugando",
            },
            new BattleState {
                Name = "Finalizada",
            }
        ];
        await _context.BattleStates.AddRangeAsync(battleStates);
    }

    private async Task SeedCardsAsync()
    {
        Card[] cards = {
            new Card {
                ImagePath = "cards/minero.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "",
                    Song = new Song // TODO: Poner cada canción y artista en sus métodos respectivos 😭
                    {
                        Name = "Minero",
                        Bpm = 100,
                        Pitch = 100,
                        Artist = new Artist {
                            Name = "Rubius"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1,
                Collection = new Collection // No sé q es esto JASDJASJD
                {
                    Name = "Mondongo"
                }
            },
            new Card {
                ImagePath = "cards/insane.png",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "",
                    Song = new Song
                    {
                        Name = "Insane",
                        Bpm = 200,
                        Pitch = 200,
                        Artist = new Artist {
                            Name = "Black Gryph0n"
                        }
                    },
                    PartId = 2
                },
                CardTypeId = 3,
                Collection = new Collection
                {
                    Name = "Alastor"
                }
            },
            new Card {
                ImagePath = "cards/theline.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "",
                    Song = new Song
                    {
                        Name = "The Line",
                        Bpm = 100,
                        Pitch = 100,
                        Artist = new Artist {
                            Name = "Twenty One Pilots"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1,
                Collection = new Collection
                {
                    Name = "La línea, Cádiz"
                }
            },
            new Card {
                ImagePath = "cards/getJinxed.png",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "",
                    Song = new Song
                    {
                        Name = "Get Jinxed",
                        Bpm = 130,
                        Pitch = 110,
                        Artist = new Artist {
                            Name = "Djerv"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1,
                Collection = new Collection
                {
                    Name = "Violeta Voltereta y su hermana Majareta"
                }
            },
            new Card {
                ImagePath = "cards/warden.gif",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "",
                    Song = new Song
                    {
                        Name = "Disco 5",
                        Bpm = 10,
                        Pitch = 1,
                        Artist = new Artist {
                            Name = "Warden?"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1,
                Collection = new Collection
                {
                    Name = "Minecraft"
                }
            },
            new Card {
                ImagePath = "cards/himnoEspana.png",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "",
                    Song = new Song
                    {
                        Name = "Himno de España",
                        Bpm = 3,
                        Pitch = 3,
                        Artist = new Artist {
                            Name = "No se jaja"
                        }
                    },
                    PartId = 2
                },
                CardTypeId = 4,
                Collection = new Collection
                {
                    Name = "Fachasong"
                }
            }
        };
        await _context.Cards.AddRangeAsync(cards);
    }

    private async Task SeedPartsAsync()
    {
        Part[] parts = {
            new Part {
                Name = "Voz"
            },
            new Part {
                Name = "Piano"
            },
            new Part {
                Name = "Guitarra"
            },
            new Part {
                Name = "Batería"
            }
        };
        await _context.Parts.AddRangeAsync(parts);
    }

    private async Task SeedCardTypeAsync() // No se q es esto pero pongo el color xD
    {
        CardType[] cardType = {
            new CardType {
                Name = "Rojo"
            },
            new CardType {
                Name = "Verde"
            },
            new CardType {
                Name = "Azul"
            },
            new CardType {
                Name = "Amarillo"
            }
        };
        await _context.CardTypes.AddRangeAsync(cardType);
    }
}
