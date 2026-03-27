using System.Text.Json.Serialization;

namespace FF1SaveEditor.Core.GameData;

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

    [JsonPropertyName("hitRate")]
    public byte HitRate { get; set; }

    [JsonPropertyName("defense")]
    public byte Defense { get; set; }

    [JsonPropertyName("evasion")]
    public byte Evasion { get; set; }

    /// <summary>Equip bitmask for 12 character classes (6 base + 6 promoted).</summary>
    [JsonPropertyName("equipMask")]
    public uint EquipMask { get; set; }

    [JsonIgnore]
    public string DisplayLabel => Id == 0 ? Name : $"{Name} ({Category})";

    public bool CanEquip(byte classId)
    {
        if (classId >= 12) return false;
        return (EquipMask & (1u << classId)) != 0;
    }
}

public enum ItemCategory
{
    None,
    Weapon,
    Armor,
    Helmet,
    Shield,
    Gauntlet,
    Consumable,
    KeyItem,
}
