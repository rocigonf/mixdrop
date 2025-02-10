using System.Text.Json.Serialization;

namespace mixdrop_back.Models.DTOs;

public class ActionType
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
