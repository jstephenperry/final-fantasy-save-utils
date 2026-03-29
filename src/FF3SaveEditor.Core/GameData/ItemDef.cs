using System.Text.Json.Serialization;

namespace FF3SaveEditor.Core.GameData;

public class ItemDef
{
    [JsonPropertyName("id")]
    public byte Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ItemCategory Category { get; set; }

    [JsonPropertyName("attack")]
    public byte Attack { get; set; }

    [JsonPropertyName("defense")]
    public byte Defense { get; set; }

    [JsonIgnore]
    public string DisplayLabel => Id == 0 ? Name : $"{Name} ({Category})";
}

public enum ItemCategory
{
    None,
    Weapon,
    Armor,
    Helmet,
    Shield,
    Gloves,
    Consumable,
    Magic,
}
