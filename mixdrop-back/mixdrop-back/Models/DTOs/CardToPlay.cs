using System.Text.Json.Serialization;

namespace mixdrop_back.Models.DTOs;

public class CardToPlay
{
    [JsonPropertyName("cardId")]
    public int CardId { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }
}
