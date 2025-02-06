using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Mappers;
using mixdrop_back.Sockets.Game;
using System.Text.Json;
using System.Text.Json.Serialization;
using Action = mixdrop_back.Models.DTOs.Action;

namespace mixdrop_back.Sockets;
// SLAY QUEEN 💅✨
public class GayHandler // GameHandler :3
{
    private const int ACTIONS_REQUIRED = 1;

    public readonly ICollection<UserBattle> _participants = new List<UserBattle>();
    public Battle Battle { get; set; }

    // Lista obtenida de la base de datos
    private static ICollection<Card> _cards = new List<Card>();

    private readonly Board _board = new Board();

    private int TotalActions { get; set; } = 0;
    private int TotalTurns { get; set; } = 0;

    private UserBattleMapper _mapper = new UserBattleMapper();


    private const string OUTPUT_SONGS_FOLDER = "songs/output/";


    /// <summary>
    /// Método que agrega participantes a la batalla
    /// </summary>
    /// <returns>Nada (por ahora)</returns>
    public async Task<UserBattleDto> AddParticipant(Battle battle, int userId, UnitOfWork unitOfWork) //💀💀💀💀
    {
        UserBattle player = battle.BattleUsers.FirstOrDefault(user => user.UserId == userId);
        if (_participants.Contains(player) || _participants.Count == 2)
        {
            return null;
        }

        if (_cards.Count == 0)
        {
            _cards = await unitOfWork.CardRepository.GetAllCardsAsync();
        }

        Random rand = new Random(); // Obtiene 5 cartas aleatorias

        for (int i = 0; i < 5; i++)
        {
            Card card = _cards.ElementAt(rand.Next(0, _cards.Count));
            player.Cards.Add(card);
        }

        if (_participants.Count + 1 == 2)
        {
            player.IsTheirTurn = true;
        }

        _participants.Add(player);

        return _mapper.ToDto(player);
    }

    public async Task PlayCard(Action action, int userId, UnitOfWork unitOfWork)
    {
        int total = 0;
        bool wasEmpty = false;

        UserBattle playerInTurn = _participants.FirstOrDefault(u => u.UserId == userId && u.IsTheirTurn);

        if (playerInTurn == null)
        {
            Console.WriteLine("No le toca a este jugador");
            return;
        }

        string filePath = "";

        // Juega las cartas que quiera
        for (int i = 0; i < action.Cards.Length; i++)
        {
            CardToPlay card = action.Cards[i];

            // Se comprueba que el jugador tuviese esta carta
            Card existingCard = playerInTurn.Cards.FirstOrDefault(c => c.Id == card.Card.Id);
            if (existingCard == null)
            {
                Console.WriteLine("La carta no existe");
                return;
            }


            // Chequeo para ver si hay puntos extra
            Slot slut = _board.Slots.ElementAt(card.Position);
            if (slut == null)
            {
                wasEmpty = true;
            }
            else
            {
                // Chequeo del nivel para que no se jueguen cartas inferiores
                if (slut.Card.Level > existingCard.Level)
                {
                    Console.WriteLine("El nivel de la carta jugada es inferior");
                    return;
                }
            }

            bool isCorrectType = true;
            string partName = existingCard.Track.Part.Name;

            // Chequeo que se pueda jugar una carta del tipo correcto para esa posición
            switch (card.Position)
            {
                case 0:
                    isCorrectType = CheckCardType(["Voz", "Piano"], partName);
                    break;
                case 1:
                    isCorrectType = CheckCardType(["Piano"], partName);
                    break;
                case 2:
                    isCorrectType = CheckCardType(["Guitarra", "Batería"], partName);
                    break;
                case 3:
                    isCorrectType = CheckCardType(["Batería"], partName);
                    break;
                default:
                    Console.WriteLine("La posición no es correcta");
                    return;
            }

            if (!isCorrectType)
            {
                Console.WriteLine("El tipo de la carta no es el correcto");
                return;
            }

            // Si todo está correcto, establezco la nueva carta y la borro del mazo
            slut.Card = existingCard;
            playerInTurn.Cards.Remove(existingCard);
            total++;

            // Establezco la nueva mezcla
            filePath = PlayMusic(_board.Playing, existingCard);

            // Si ya ha hecho sus acciones, rompo el bucle
            if (total == ACTIONS_REQUIRED)
            {
                break;
            }
        }

        // Si aún debe seguir jugando (solo ha tirado una carta, chequeo sus acciones)
        if (total < ACTIONS_REQUIRED)
        {
            // TODO: Implementar acciones
            for (int i = 0; i < action.ActionsType.Length; i++)
            {
                ActionType actionType = action.ActionsType[i];
                switch (actionType.Name)
                {
                    default:
                        Console.WriteLine("La acción no existe");
                        break;
                }

                total++;
                if (total == ACTIONS_REQUIRED) { break; }
            }

        }

        // Si el total de acciones en la partida es par, significa que se ha completado un turno entero
        TotalActions++;
        if (TotalActions % 2 == 0)
        {
            TotalTurns++;
            if (wasEmpty) { playerInTurn.Punctuation++; } // Puntos extra
        }

        playerInTurn.Punctuation += 1;

        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.TurnResult },
            { "board", _board },
            { "player", _mapper.ToDto(playerInTurn) },
            { "filepath", filePath }
        };

        // Cambio el turno
        UserBattle otherUser = _participants.FirstOrDefault(u => u.UserId != userId);
        playerInTurn.IsTheirTurn = false;
        otherUser.IsTheirTurn = true;

        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Doy por terminada la batalla
        if (playerInTurn.Punctuation == 21)
        {
            BattleState battleState = await unitOfWork.BattleStateRepository.GetByIdAsync(4);
            ICollection<BattleResult> results = await unitOfWork.BattleResultRepository.GetAllAsync();

            Battle.BattleState = battleState;
            Battle.BattleStateId = battleState.Id;

            BattleResult victory = results.FirstOrDefault(b => b.Name == "Victoria");
            BattleResult defeat = results.FirstOrDefault(b => b.Name == "Derrota");

            playerInTurn.BattleResult = victory;
            playerInTurn.BattleResultId = victory.Id;

            otherUser.BattleResult = defeat;
            otherUser.BattleResultId = defeat.Id;

            unitOfWork.BattleRepository.Update(Battle);
            unitOfWork.UserBattleRepository.Update(playerInTurn);
            unitOfWork.UserBattleRepository.Update(otherUser);

            await unitOfWork.SaveAsync();

            dict["messageType"] = MessageType.EndGame;
            GayNetwork._handlers.Remove(this);
        }

        // Notifico a los usuarios
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), playerInTurn.UserId);

        dict["player"] = _mapper.ToDto(otherUser);
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), otherUser.UserId);

    }

    private static bool CheckCardType(List<string> possibleTypes, string actualType)
    {
        return possibleTypes.Contains(actualType);
    }

    private string PlayMusic(Track playing, Card card)
    {
        if (playing == null)
        {
            _board.Playing = card.Track;
            return card.Track.TrackPath;
        }
        else
        {
            // Ficheros de guardado
            string relativePathCurrent = $"{OUTPUT_SONGS_FOLDER}{Guid.NewGuid()}.wav";
            string relativePathNew = $"{OUTPUT_SONGS_FOLDER}{Guid.NewGuid()}.wav";
            string output = $"{OUTPUT_SONGS_FOLDER}{Guid.NewGuid()}.wav";

            // Cálculo de los nuevos BPM
            float currentBpm = playing.Song.Bpm;
            float cardBpm = card.Track.Song.Bpm;
            float average = (currentBpm + cardBpm) / 2;

            float changeForCurrent = (currentBpm - average) / currentBpm;
            float changeForCard = (cardBpm - average) / cardBpm;

            float newBpmForCurrent = CalculateNewBpm(changeForCurrent);
            float newBpmForCard = CalculateNewBpm(changeForCard);
            
            // Cálculo del nuevo pitch
            int semitoneCurrent = MusicNotes.NOTE_MAP[playing.Song.Pitch];
            int semitoneCard = MusicNotes.NOTE_MAP[card.Track.Song.Pitch];
            double newSemitoneCurrent = 1; // Aparentemente el nuevo semitono se aplica siempre a la que ya se esta reproduciendo (según Gepetronco)

            int difference = semitoneCard - semitoneCurrent;
            if (difference > 6)
            {
                difference -= 12;
            }
            else if (difference < -6)
            {
                difference += 12;
            }

            if (difference > 0)
            {
                newSemitoneCurrent = MusicNotes.SEMITONE / -difference;
            }
            else if(difference > 0)
            {
                newSemitoneCurrent = MusicNotes.SEMITONE * difference;
            }

            HellIsForever.ChangeBPM(playing.TrackPath, relativePathCurrent, newBpmForCurrent, (float) newSemitoneCurrent);
            HellIsForever.ChangeBPM(card.Track.TrackPath, relativePathNew, newBpmForCard);
            HellIsForever.MixFiles(relativePathCurrent, relativePathNew, output);

            return output;
        }
    }

    private static float CalculateNewBpm(float difference)
    {
        if (difference > 0)
        {
            return 1 + difference;
        }
        else
        {
            return 1 - difference;
        }
    }
}
