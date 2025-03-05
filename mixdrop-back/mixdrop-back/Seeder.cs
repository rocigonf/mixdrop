using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Helper;
using mixdrop_back.Sockets.Game;



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
            //new Card {
            //    ImagePath = "cards/kingoftheworld_lead.jpg",
            //    Level = 1,
            //    Track = new Track
            //    {
            //        TrackPath = "Songs/Input/Vocal/KingOfTheWorld_LEAD_KingOfTheWorld_VOX_Gb_80(120).mp3",
            //        Song = new Song // TODO: Poner cada canción y artista en sus métodos respectivos 😭
            //        {
            //            Name = "King Of The World",
            //            Bpm = 80,
            //            Pitch = "Gb",
            //            Artist = new Artist {
            //                Name = "Weezer"
            //            }
            //        },
            //        PartId = 1
            //    },
            //    CardTypeId = 1
            //},
            new Card {
                ImagePath = "cards/bringmetolife_lead.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/BringMeToLife_LEAD_BringMeToLife_VOX1_Em_95(120).mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Em"], false)
                    {
                        Name = "Bring Me To Life",
                        Bpm = 120,
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
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Ebm"], false)
                    {
                        Name = "Attention",
                        Bpm = 120,
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
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Bm"], false)
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
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Em"], false)
                    {
                        Name = "Centuries",
                        Bpm = 120,
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
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Em"], false)
                    {
                        Name = "Animals",
                        Bpm = 120,
                        Pitch = "Em",
                        Artist = new Artist {
                            Name = "Maroon 5"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },
            new Card
            {
                ImagePath = "cards/scifi3_lead.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/WhoLetTheDogsOut_VOX_WhoLetTheDogsOut_VOX3_C_130.mp3",
                    Song = new Song("C", true)
                    {
                        Name = "Who Let The Dogs Out",
                        Bpm = 130,
                        Pitch = "C",
                        Artist = new Artist {
                            Name = "Baha Men"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },
            new Card
            {
                ImagePath = "cards/shutupanddance_lead.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/TheMotherWeShare_LEAD_TheMotherWeShare_VOX_Dbm_87(120).mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Dbm"], false)
                    {
                        Name = "The Mother We Share",
                        Bpm = 120,
                        Pitch = "Dbm",
                        Artist = new Artist {
                            Name = "Chvrches"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },
            new Card
            {
                ImagePath = "cards/allaboutthatbass_lead.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/AllAboutThatBass_LEAD_AllAboutThatBass_VOX_A_134.mp3",
                    Song = new Song("A", true)
                    {
                        Name = "All About That Bass",
                        Bpm = 134,
                        Pitch = "A",
                        Artist = new Artist {
                            Name = "Meghan Trainor"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },
            new Card
            {
                ImagePath = "cards/wanttowantme_lead.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Vocal/September_September_VOX_A_126.mp3",
                    Song = new Song("A", true)
                    {
                        Name = "September",
                        Bpm = 126,
                        Pitch = "A",
                        Artist = new Artist {
                            Name = "Earth, Wind and Fire"
                        }
                    },
                    PartId = 1
                },
                CardTypeId = 1
            },

            /* PRINCIPAL */
            new Card {
                ImagePath = "cards/wipeout_loop.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/Wipeout_LOOP_Wipeout_GTR_B_150.mp3",
                    Song = new Song("B", true)
                    {
                        Name = "Wipeout",
                        Bpm = 150,
                        Pitch = "B",
                        Artist = new Artist {
                            Name = "The Surfaris"
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
                    TrackPath = "Songs/Input/Main/Warm_GTR_warm_loop_bmin_132.mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Bm"], false)
                    {
                        Name = "Warm",
                        Bpm = 132,
                        Pitch = "Bm",
                        Artist = new Artist {
                            Name = "A Clutch of Heart Shards"
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
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Em"], false)
                    {
                        Name = "Gimme Chocolate!!",
                        Bpm = 120,
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
                    Song = new Song("G", true)
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
                ImagePath = "cards/temperature_loop.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/ScreamAndShout_LOOP_ScreamAndShout_SYN_G_130.mp3",
                    Song = new Song("G", true)
                    {
                        Name = "Scream And Shout",
                        Bpm = 130,
                        Pitch = "G",
                        Artist = new Artist {
                            Name = "Britney Spears"
                        }
                    },
                    PartId = 2
                },
                CardTypeId = 2
            },
            new Card {
                ImagePath = "cards/findyou_loop.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/FindYou_LOOP_guitar.mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Am"], false)
                    {
                        Name = "Find You",
                        Bpm = 150,
                        Pitch = "Am",
                        Artist = new Artist {
                            Name = "Nightfeels"
                        }
                    },
                    PartId = 2
                },
                CardTypeId = 2
            },
            new Card {
                ImagePath = "cards/iwillsurvive_loop.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/IWillSurvive_LOOP_IWillSurvive_STR_A_120(116).mp3",
                    Song = new Song("A", true)
                    {
                        Name = "I Will Survive",
                        Bpm = 116,
                        Pitch = "A",
                        Artist = new Artist {
                            Name = "Gloria Gaynor"
                        }
                    },
                    PartId = 2
                },
                CardTypeId = 2
            },
            new Card {
                ImagePath = "cards/fashion1_loop.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/LaCamisaNegra_GTR_LaCamisaNegra_GTR_Gbm_97(120).mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Gbm"], false)
                    {
                        Name = "La Camisa Negra",
                        Bpm = 120,
                        Pitch = "Gbm",
                        Artist = new Artist {
                            Name = "Juanes"
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
                    Song = new Song("C", true)
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
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Em"], false)
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
                    Song = new Song("A", true)
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
                ImagePath = "cards/scifi2_bass.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Bass/WhatIsLove_WhatIsLove_BASS_G_124.mp3",
                    Song = new Song("G", true)
                    {
                        Name = "What Is Love",
                        Bpm = 124,
                        Pitch = "G",
                        Artist = new Artist {
                            Name = "Haddaway"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },
            new Card {
               ImagePath = "cards/city1_bass.jpg",
               Level = 2,
               Track = new Track
               {
                   TrackPath = "Songs/Input/Bass/MovesLikeJagger_MovesLikeJagger_BASS1_B_128.mp3",
                   Song = new Song("B", true)
                   {
                       Name = "Moves Like Jagger",
                       Bpm = 128,
                       Pitch = "B",
                       Artist = new Artist {
                          Name = "Maroon 5"
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
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Dm"], false)
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
                    Song = new Song("A" ,true)
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
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Gbm"], false)
                    {
                        Name = "Cheap Thrills",
                        Bpm = 120,
                        Pitch = "Gbm",
                        Artist = new Artist {
                            Name = "Sia ft.Sean Paul"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },
            new Card {
                ImagePath = "cards/superfreak_wild.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Bass/Superfreak_Superfreak_BASS_A_132.mp3",
                    Song = new Song("A", true)
                    {
                        Name = "Superfreak",
                        Bpm = 132,
                        Pitch = "A",
                        Artist = new Artist {
                            Name = "Rick James"
                        }
                    },
                    PartId = 3
                },
                CardTypeId = 3
            },
            new Card {
                ImagePath = "cards/sexyandiknowit_bass.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Bass/StraightUp_BASS_StraightUp_BASS_D_96(120).mp3",
                    Song = new Song("D", true)
                    {
                        Name = "Straight Up",
                        Bpm = 120,
                        Pitch = "D",
                        Artist = new Artist {
                            Name = "Paula Abdul"
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
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Ebm"], false)
                    {
                        Name = "Atomic Handbrake",
                        Bpm = 129,
                        Pitch = "Ebm",
                        Artist = new Artist {
                            Name = "Bullwheel"
                        }
                    },
                    PartId = 4
                },
                CardTypeId = 4
            },
            new Card {
                ImagePath = "cards/boomboompow_beat.jpg",
                Level = 1,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Drums/BoomBoomPow_DMSAm_130.mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Am"], false)
                    {
                        Name = "Boom Boom Pow",
                        Bpm = 130,
                        Pitch = "Am",
                        Artist = new Artist {
                            Name = "The Black Eyed Peas"
                        }
                    },
                    PartId = 4
                },
                CardTypeId = 4
            },
            new Card {
                ImagePath = "cards/fashion1_beat.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Drums/LaCamisaNegra_DMSGbm_97(120).mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Gbm"], false)
                    {
                        Name = "La Camisa Negra",
                        Bpm = 120,
                        Pitch = "Gbm",
                        Artist = new Artist {
                            Name = "Juanes"
                        }
                    },
                    PartId = 4
                },
                CardTypeId = 4
            },
            new Card {
                ImagePath = "cards/storybook1_beat.jpg",
                Level = 2,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Drums/TheBreaks_DMSBm_114(120).mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Bm"], false)
                    {
                        Name = "The Breaks",
                        Bpm = 120,
                        Pitch = "Bm",
                        Artist = new Artist {
                            Name = "Kurtis Blow"
                        }
                    },
                    PartId = 4
                },
                CardTypeId = 4
            },
            new Card
            {
                ImagePath = "cards/allaboutthatbass_beat.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Drums/AllAboutThatBass_DMSA_134.mp3",
                    Song = new Song("A", true)
                    {
                        Name = "All About That Bass",
                        Bpm = 134,
                        Pitch = "A",
                        Artist = new Artist {
                            Name = "Meghan Trainor"
                        }
                    },
                    PartId = 4
                },
                CardTypeId = 4
            },
            new Card {
                ImagePath = "cards/radioactive_beat.jpg",
                Level = 3,
                Track = new Track
                {
                    TrackPath = "Songs/Input/Drums/Radioactive_DMSBm_137.mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Bm"], false)
                    {
                        Name = "Radioactive",
                        Bpm = 137,
                        Pitch = "Bm",
                        Artist = new Artist {
                            Name = "Imagine Dragons"
                        }
                    },
                    PartId = 4
                },
                CardTypeId = 4
            },

            // Comodines
            new Card {
                ImagePath = "cards/storybookfx1_fx.jpg",
                Level = 0, // Los comodines deberían tener nivel 4

                // El track da igual, es para que no le de la paja xD
                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/FindYou_LOOP_guitar.mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Am"], false)
                    {
                        Name = "Find You",
                        Bpm = 150,
                        Pitch = "Am",
                        Artist = new Artist {
                            Name = "Nightfeels"
                        }
                    },
                    PartId = 2
                },

                CardTypeId = 4,
                Effect = "-1 punto al rival"
            },
            new Card {
                ImagePath = "cards/mvpfx2_fx.jpg",
                Level = 0,


                Track = new Track
                {
                    TrackPath = "Songs/Input/Main/FindYou_LOOP_guitar.mp3",
                    Song = new Song(MusicNotes.FIFTH_CIRCLE["Am"], false)
                    {
                        Name = "Find You",
                        Bpm = 150,
                        Pitch = "Am",
                        Artist = new Artist {
                            Name = "Nightfeels"
                        }
                    },
                    PartId = 2
                },

                CardTypeId = 4,
                Effect = "Baraja tus cartas"
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
            },
            new CardType {
                Name = "Gris"
            },
        };
        await _context.CardTypes.AddRangeAsync(cardType);
    }
}
