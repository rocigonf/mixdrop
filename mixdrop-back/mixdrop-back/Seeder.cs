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
            /* VOCALES */
            new Card {
                ImagePath = "cards/kingoftheworld_lead.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/KingOfTheWorld_LEAD_KingOfTheWorld_VOX_Gb_80(120).mp3",
                    Song = new Song // TODO: Poner cada canción y artista en sus métodos respectivos 😭
                    {
                        Name = "King Of The World",
                        Bpm = 80,
                        Pitch = "Gb",
                        Artist = new Artist {
                            Name = "Weezer"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },
            new Card {
                ImagePath = "cards/bringmetolife_lead.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/BringMeToLife_LEAD_BringMeToLife_VOX1_Em_95(120).mp3",
                    Song = new Song
                    {
                        Name = "Bring Me To Life",
                        Bpm = 95,
                        Pitch = "Em",
                        Artist = new Artist {
                            Name = "Evanescence"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },
            new Card {
                ImagePath = "cards/animals1_lead.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/Attention_VOX_Attention_VOX_Ebm_100(120).mp3",
                    Song = new Song
                    {
                        Name = "Attention",
                        Bpm = 100,
                        Pitch = "Ebm",
                        Artist = new Artist {
                            Name = "Charlie Puth"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },
            new Card {
                ImagePath = "cards/radioactive_lead.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/Radioactive_LEAD_Radioactive_VOX_Bm_137.mp3",
                    Song = new Song
                    {
                        Name = "Radioactive",
                        Bpm = 137,
                        Pitch = "Bm",
                        Artist = new Artist {
                            Name = "Imagine Dragons"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },
            new Card {
                ImagePath = "cards/centuries_lead.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/Centuries_LEAD_Centuries_VOX_Em_88(120).mp3",
                    Song = new Song
                    {
                        Name = "Centuries",
                        Bpm = 88,
                        Pitch = "Em",
                        Artist = new Artist {
                            Name = "Fall Out Boy"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },
            new Card {
                ImagePath = "cards/animals4_lead.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/AnimalsM5_VOX_AnimalsM5_VOX_Em_95(120).mp3",
                    Song = new Song
                    {
                        Name = "Animals",
                        Bpm = 95,
                        Pitch = "Em",
                        Artist = new Artist {
                            Name = "Maroon 5"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },

            /* PRINCIPAL */
            new Card {
                ImagePath = "cards/cantfeelmyface_loop.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/CantFeelMyFace_LOOP_CantFeelMyFace_LOOP_min.mp3",
                    Song = new Song
                    {
                        Name = "Can't Feel My Face",
                        Bpm = 108,
                        Pitch = "Am",
                        Artist = new Artist {
                            Name = "The Weeknd"
                        }
                    },
                    PartId = 2
                },
                CardTypeId = 2
            },
            new Card {
                ImagePath = "cards/closer_loop.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/Closer_LOOP_Closer_LOOP_min.mp3",
                    Song = new Song
                    {
                        Name = "Closer",
                        Bpm = 95,
                        Pitch = "Fm",
                        Artist = new Artist {
                            Name = "The Chainsmokers ft.Halsey"
                        }
                    },
                    PartId = 2
                },
                CardTypeId = 2
            },
            new Card {
                ImagePath = "cards/animals3_loop.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/GimmeChocolate_SYN_GimmeChocolate_SYN_Em_110(120).mp3",
                    Song = new Song
                    { 
                        Name = "Gimme Chocolate!!",
                        Bpm = 110,
                        Pitch = "Em",
                        Artist = new Artist {
                            Name = "BABYMETAL"
                        }
                    },
                    PartId = 2
                },
                CardTypeId = 2
            },
            new Card {
                ImagePath = "cards/callmemaybe_loop.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/CallMeMaybe_LOOP_CallMeMaybe_LOOP_maj.mp3",
                    Song = new Song
                    {
                        Name = "Call Me Maybe",
                        Bpm = 120,
                        Pitch = "G",
                        Artist = new Artist {
                            Name = "Carly Rae Jepsen"
                        }
                    },
                    PartId = 2
                },
                CardTypeId = 2
            },
            new Card {
                ImagePath = "cards/animals0_loop.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/AnimalsGarrix_SYN_AnimalsGarrix_SYN_Fm_128.mp3",
                    Song = new Song
                    {
                        Name = "Animals",
                        Bpm = 128,
                        Pitch = "Fm",
                        Artist = new Artist {
                            Name = "Martin Garrix"
                        }
                    },
                    PartId = 2
                },
                CardTypeId = 2
            },

            /* BAJO */

            new Card {
                ImagePath = "cards/daysaheaddaysbehind_bass.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Bass/DaysAhead_BASS_DaysAhead_BASS_C_124.mp3",
                    Song = new Song
                    {
                        Name = "Days Ahead, Days Behind",
                        Bpm = 124,
                        Pitch = "C",
                        Artist = new Artist {
                            Name = "Some Lover"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },
            new Card {
                ImagePath = "cards/takemeout_bass.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Bass/TakeMeOut_BASS_TakeMeOut_BASS_Em_120(104).mp3",
                    Song = new Song
                    {
                        Name = "Take Me Out",
                        Bpm = 104,
                        Pitch = "Em",
                        Artist = new Artist {
                            Name = "Franz Ferdinand"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },
            new Card {
                ImagePath = "cards/allaboutthatbass_bass.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Bass/AllAboutThatBass_BASS_AllAboutThatBass_BASS_A_134.mp3",
                    Song = new Song
                    {
                        Name = "All About That Bass",
                        Bpm = 134,
                        Pitch = "A",
                        Artist = new Artist {
                            Name = "Meghan Trainor"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },
            new Card {
                ImagePath = "cards/shutupanddance_bass.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Bass/ShutUpAndDance_BASS_ShutUpAndDance_BASS_maj.mp3",
                    Song = new Song
                    {
                        Name = "Shut Up And Dance",
                        Bpm = 128,
                        Pitch = "Db",
                        Artist = new Artist {
                            Name = "Walk The Moon"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },
            new Card {
                ImagePath = "cards/mvp2_bass.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Bass/AllIDoIsWin_BASS_AllIDoIsWin_BASS_Dm_151.mp3",
                    Song = new Song
                    {
                        Name = "All I Do Is Win",
                        Bpm = 151,
                        Pitch = "Dm",
                        Artist = new Artist {
                            Name = "DJ Khaled ft.T-Pain"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },
            new Card {
                ImagePath = "cards/nature4_bass.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Bass/AYo_BASS_AYo_BASS2_A_150.mp3",
                    Song = new Song
                    {
                        Name = "A-YO",
                        Bpm = 150,
                        Pitch = "A",
                        Artist = new Artist {
                            Name = "Lady Gaga"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },
            new Card {
                ImagePath = "cards/crush5_bass.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Bass/CheapThrills_BASS_CheapThrills_BASS_Gbm_90(120).mp3",
                    Song = new Song
                    {
                        Name = "Cheap Thrills",
                        Bpm = 90,
                        Pitch = "Gbm",
                        Artist = new Artist {
                            Name = "Sia ft.Sean Paul"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },

            /* BATERÍA */

            new Card {
                ImagePath = "cards/atomichandbrake_beat.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Drums/AtomicHandbrake_DMSebmin_129.mp3",
                    Song = new Song
                    {
                        Name = "Atomic Handbrake",
                        Bpm = 129,
                        Pitch = "Ebm",
                        Artist = new Artist {
                            Name = "Bullwheel"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },
        };
        await _context.Cards.AddRangeAsync(cards);
    }

    private async Task SeedPartsAsync()
    {
        Part[] parts = {
            new Part {
                Name = "Vocal"
            },
            new Part {
                Name = "Main"
            },
            new Part {
                Name = "Bass"
            },
            new Part {
                Name = "Drums"
            }
        };
        await _context.Parts.AddRangeAsync(parts);
    }

    private async Task SeedCardTypeAsync() // No se q es esto pero pongo el color xD
    { // CAMBIAR ESTO AL TIPO DE CARTA ( NORMAL, COMODÍN, EFECTOS)
        CardType[] cardType = {
            new CardType {
                Name = "Amarillo"
            },
            new CardType {
                Name = "Rojo"
            },
            new CardType {
                Name = "Verde"
            },
            new CardType {
                Name = "Azul"
            }
        };
        await _context.CardTypes.AddRangeAsync(cardType);
    }
}
