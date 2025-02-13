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
    private readonly AudioModifier _audioModifier = new AudioModifier();

    private int TotalActions { get; set; } = 0;
    private int TotalTurns { get; set; } = 0;

    private readonly UserBattleMapper _mapper = new UserBattleMapper();
    private readonly Random _random = new Random();

    private string Bonus { get; set; } = "";


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

        Random rand = new Random();

        // Creo el bot y le reparto las cartas
        if (battle.IsAgainstBot)
        {
            UserBattle bot = new UserBattle() { IsBot = true };
            DistributeCards(bot);
            _participants.Add(bot);
        }

        DistributeCards(player);

        if (_participants.Count + 1 == 2)
        {
            player.IsTheirTurn = true;
            player.TimePlayed = 120;  // 2 minutos para jugar (esta en proceso esto)
            player.ActionsLeft = ACTIONS_REQUIRED;
        }

        _participants.Add(player);

        return _mapper.ToDto(player);
    }

    private void DistributeCards(UserBattle userBattle)
    {
        for (int i = 0; i < 5; i++)
        {
            Card card = _cards.ElementAt(_random.Next(0, _cards.Count));
            userBattle.Cards.Add(card);
        }
    }

    // TEMPORIZADOR (nose bien como hacerlo :_(  )
    /*
    private void Timer(UserBattle userBattle, UnitOfWork unitOfWork)
    {
        if (!userBattle.IsTheirTurn)
        {
            Console.WriteLine("No le toca a este jugador");
            return;
        }
        if (userBattle.TimePlayed <= 0)
        {
            Console.WriteLine("Al jugador no le queda tiempo");
            return;
        }


        while (userBattle.TimePlayed > 0)
        {
            userBattle.TimePlayed--;
        }
       
        if (userBattle.TimePlayed == 0)
        {   
            var userEnemy = _participants.FirstOrDefault(u => u.UserId != userBattle.UserId);
                        
            EndBattle(userEnemy, userBattle, unitOfWork);
        }

    }*/


    public async Task PlayCard(Action action, int userId, UnitOfWork unitOfWork)
    {
        UserBattle playerInTurn = _participants.FirstOrDefault(u => u.UserId == userId && u.IsTheirTurn);
        UserBattle otherUser = _participants.FirstOrDefault(u => u.UserId != userId);

        if (playerInTurn == null)
        {
            Console.WriteLine("No le toca a este jugador");
            return;
        }

        byte[] output = [];

        // Juega las cartas que quiera
        if (action.Card != null)
        {
            bool wasEmpty = false;

            CardToPlay card = action.Card;

            // Se comprueba que el jugador tuviese esta carta
            Card existingCard = playerInTurn.Cards.FirstOrDefault(c => c.Id == card.CardId);
            if (existingCard == null)
            {
                Console.WriteLine("La carta no existe");
                return;
            }

            // Chequeo para ver si hay puntos extra
            Slot slut = _board.Slots.ElementAt(card.Position);
            if (slut.Card == null)
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
                    isCorrectType = CheckCardType(["Vocal", "Main"], partName);
                    break;
                case 1:
                    isCorrectType = CheckCardType(["Main"], partName);
                    break;
                case 2:
                    isCorrectType = CheckCardType(["Main", "Drums"], partName);
                    break;
                case 3:
                    isCorrectType = CheckCardType(["Drums"], partName);
                    break;
                case 4:
                    isCorrectType = CheckCardType(["Drums", "Bass"], partName);
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
            slut.UserId = playerInTurn.UserId;
            playerInTurn.Cards.Remove(existingCard);
            playerInTurn.Cards.Add(_cards.ElementAt(_random.Next(0, _cards.Count)));

            // Bonificaciones random
            switch (Bonus)
            {
                case "Amarillo":
                    playerInTurn.Punctuation += CheckForCardType(1, existingCard.CardType.Id);
                    break;
                case "Rojo":
                    playerInTurn.Punctuation += CheckForCardType(2, existingCard.CardType.Id);
                    break;
                case "Verde":
                    playerInTurn.Punctuation += CheckForCardType(3, existingCard.CardType.Id);
                    break;
                case "Azul":
                    playerInTurn.Punctuation += CheckForCardType(4, existingCard.CardType.Id);
                    break;
            }

            // Establezco la nueva mezcla
            output = PlayMusic(_board.Playing, existingCard);
            playerInTurn.Punctuation += 1;

            if (TotalTurns >= 1)
            {
                if (wasEmpty) { playerInTurn.Punctuation++; } // Puntos extra
            }

            //mix = PlayMusic(_board.Playing, existingCard);
            playerInTurn.ActionsLeft--;
        }
        else if (action.ActionType != null)
        {
            ActionType actionType = action.ActionType;
            switch (actionType.Name)
            {
                case "button":
                    SpinTheWheel(otherUser);
                    playerInTurn.ActionsLeft--;
                    break;
                default:
                    Console.WriteLine("La acción no existe");
                    break;
            }
        }

        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.TurnResult },
            { "board", _board },
            { "player", _mapper.ToDto(playerInTurn) },
            { "filepath", Convert.ToBase64String(output) },
            { "bonus", Bonus },
            { "position", action.Card?.Position },
        };

        await NotifyUsers(dict, playerInTurn, otherUser);

        // Si aún puede seguir jugando
        if (playerInTurn.ActionsLeft > 0)
        {
            return;
        }

        // Doy por terminada la batalla
        if (playerInTurn.Punctuation == 21)
        {
            await EndBattle(playerInTurn, otherUser, unitOfWork);
            dict["messageType"] = MessageType.EndGame;
        }
        else
        {
            if (otherUser.IsBot)
            {
                output = await DoBotActions(otherUser, playerInTurn, unitOfWork, dict);
                dict["board"] = _board; // Actualizo de vuelta el tablero
                dict["filepath"] = output;
                playerInTurn.ActionsLeft = ACTIONS_REQUIRED;
            }
            else
            {
                playerInTurn.IsTheirTurn = false;

                otherUser.IsTheirTurn = true;
                otherUser.TimePlayed = 120;
                otherUser.ActionsLeft = ACTIONS_REQUIRED;
            }
        }

        // Si el total de acciones en la partida es par, significa que se ha completado un turno entero
        TotalActions++;
        if (TotalActions % 2 == 0)
        {
            TotalTurns++;

            // CUANDO TERMINO EL TURNO, GENERO UN BONUS ALEATORIO (25% de que ocurra, idk cuanto es en Dropmix realmente, y lo mismo habría que poner por nivel también)
            int random = _random.Next(0, 100);
            string[] colors = ["Amarillo", "Rojo", "Verde", "Azul"];
            if(random < 25)
            {
                Bonus = colors[_random.Next(0, colors.Length)];
            }
            else
            {
                Bonus = "";
            }
        }

        await NotifyUsers(dict, playerInTurn, otherUser);
    }

    private async Task NotifyUsers(Dictionary<object, object> dict, UserBattle playerInTurn, UserBattle otherUser)
    {
        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Notifico a los usuarios
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), playerInTurn.UserId);

        if (otherUser.IsBot)
        {
            return;
        }

        dict["player"] = _mapper.ToDto(otherUser);
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), otherUser.UserId);
    }

    private static int CheckForCardType(int desiredType, int actualType)
    {
        return desiredType == actualType ? 1 : 0;
    }

    private async Task<byte[]> DoBotActions(UserBattle bot, UserBattle notBot, UnitOfWork unitOfWork, Dictionary<object, object> dict)
    {
        byte[] output = [];
        int totalActions = 0;

        while (totalActions < ACTIONS_REQUIRED)
        {
            bool couldPlay = false;

            foreach (Slot currentSlot in _board.Slots)
            {
                Card card = GetValidCardForSlot(currentSlot, bot);
                if(card == null)
                {
                    continue;
                }

                bool canPlay = false;
                bool wasEmpty = false;

                if (currentSlot.Card == null)
                {
                    canPlay = true;
                    wasEmpty = true;
                }
                else
                {
                    // El Max está para comprobar si puede poner la carta ahí
                    if (currentSlot.Card.Level <= card.Level)
                    {
                        canPlay = true;
                    }
                }

                if (canPlay && card != null)
                {
                    currentSlot.Card = card;
                    currentSlot.UserId = 0;

                    bot.Cards.Remove(card);
                    bot.Cards.Add(_cards.ElementAt(_random.Next(0, _cards.Count)));
                    bot.Punctuation++;

                    output = PlayMusic(_board.Playing, card);

                    if (TotalTurns >= 1)
                    {
                        if (wasEmpty) { bot.Punctuation++; }
                    }

                    couldPlay = true;

                    totalActions++;
                    if (totalActions == ACTIONS_REQUIRED)
                    {
                        break;
                    }
                }
            }

            // Si no pudo jugar ninguna carta, tira la ruleta
            if (!couldPlay)
            {
                SpinTheWheel(notBot);
                totalActions++;
            }
        }

        TotalActions++;
        if (TotalActions % 2 == 0)
        {
            TotalTurns++;
        }

        if (bot.Punctuation == 21)
        {
            await EndBattle(bot, notBot, unitOfWork);
            dict["messageType"] = MessageType.EndGame;
        }

        return output;
    }

    private void SpinTheWheel(UserBattle opponent)
    {
        int result = _random.Next(0, 100);
        if (result < 50)
        {
            RemoveCardsOfLevel(3, opponent);
        }
        else if (result >= 50 && result < 75)
        {
            RemoveCardsOfLevel(2, opponent);
        }
        else if(result >= 75 && result < 90)
        {
            RemoveCardsOfLevel(1, opponent);
        }
        else
        {
            RemoveCardsOfLevel(0, opponent);
        }
    }

    private void RemoveCardsOfLevel(int level, UserBattle player)
    {
        for (int i = 0; i < _board.Slots.Length; i++)
        {
            Slot slot = _board.Slots[i];
            if(slot.Card == null || slot.UserId != player.UserId)
            {
                continue;
            }

            if(level == 0)
            {
                slot.Card = null;
            }
            else if(slot.Card.Level == level)
            {
                slot.Card = null;
            }
        }
    }

    private Card GetValidCardForSlot(Slot slot, UserBattle bot)
    {
        // Busca el índice del slot
        switch (Array.IndexOf(_board.Slots, slot))
        {
            case 0:
                return bot.Cards.FirstOrDefault(c => (c.Track.Part.Name.Equals("Vocal") || c.Track.Part.Name.Equals("Main")));
            case 1:
                return bot.Cards.FirstOrDefault(c => c.Track.Part.Name.Equals("Main"));
            case 2:
                return bot.Cards.FirstOrDefault(c => (c.Track.Part.Name.Equals("Main") || c.Track.Part.Name.Equals("Drums")));
            case 3:
                return bot.Cards.FirstOrDefault(c => c.Track.Part.Name.Equals("Drums"));
            case 4:
                return bot.Cards.FirstOrDefault(c => (c.Track.Part.Name.Equals("Drums") || c.Track.Part.Name.Equals("Bass")));
            default:
                return null;
        }
    }

    private async Task EndBattle(UserBattle winner, UserBattle loser, UnitOfWork unitOfWork)
    {
        BattleState battleState = await unitOfWork.BattleStateRepository.GetByIdAsync(4);
        ICollection<BattleResult> results = await unitOfWork.BattleResultRepository.GetAllAsync();

        Battle.BattleState = battleState;
        Battle.BattleStateId = battleState.Id;

        BattleResult victory = results.FirstOrDefault(b => b.Name == "Victoria");
        BattleResult defeat = results.FirstOrDefault(b => b.Name == "Derrota");

        unitOfWork.BattleRepository.Update(Battle);

        if (!winner.IsBot)
        {
            winner.BattleResult = victory;
            winner.BattleResultId = victory.Id;
            unitOfWork.UserBattleRepository.Update(winner);
        }

        if (!loser.IsBot)
        {
            loser.BattleResult = defeat;
            loser.BattleResultId = defeat.Id;
            unitOfWork.UserBattleRepository.Update(loser);
        }

        await unitOfWork.SaveAsync();
        GayNetwork._handlers.Remove(this);
    }

    private static bool CheckCardType(List<string> possibleTypes, string actualType)
    {
        return possibleTypes.Contains(actualType);
    }

    private byte[] PlayMusic(Track playing, Card card)
    {
        if (playing == null)
        {
            _board.Playing = card.Track;
            return File.ReadAllBytes("wwwroot/" + card.Track.TrackPath);
        }
        else
        {
            // Cálculo de los nuevos BPM
            float currentBpm = playing.Song.Bpm;
            float cardBpm = card.Track.Song.Bpm;

            float changeForCard = (cardBpm - currentBpm) / cardBpm;
            float newBpmForCard = CalculateNewBpm(changeForCard);

            // Cálculo del nuevo pitch (CALCULAR COMO EN EL BPM PORQUE SI EN LA NOTA "Do" LA CANTIDAD SE SEMITONOS A DIVIDIR SERÍA 0)            
            int semitoneCurrent = GetFromDictionary(playing.Song.Pitch);
            int semitoneCard = GetFromDictionary(card.Track.Song.Pitch);

            int difference = semitoneCard - semitoneCurrent;
            float pitchFactor = 1.0f;

            // Sólo aplico si es mayor que 1
            if (Math.Abs(difference) > 1)
            {
                pitchFactor = (float)Math.Pow(2, difference / 12.0);
            }

            byte[] newAudio = _audioModifier.Modify("wwwroot/" + card.Track.TrackPath, newBpmForCard, pitchFactor);
            //byte[] message = [..BitConverter.GetBytes(card.Id), .. newAudio];
            byte[] message = [..newAudio];

            _board.Playing = new Track()
            {
                Song = new Song()
                {
                    Bpm = playing.Song.Bpm,
                    Pitch = playing.Song.Pitch,
                }
            };

            return message;

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

    private static int GetFromDictionary(string value)
    {
        int semitone = 0;

        bool couldGet = MusicNotes.NOTE_MAP.TryGetValue(value, out semitone);

        // Si no lo puede conseguir significa que está en la escala menor
        if (!couldGet)
        {
            semitone = MusicNotes.NOTE_MAP[MusicNotes.FIFTH_CIRCLE[value]];
        }
        return semitone;
    }
}