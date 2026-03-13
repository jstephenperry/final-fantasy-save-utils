using System.Text.Json.Serialization;
using FF4SaveEditor.Core.Models;

namespace FF4SaveEditor.Core.GameData;

/// <summary>
/// Definition of a single item from the game database.
/// </summary>
public class ItemDef
{
    [JsonPropertyName("id")]
    public byte Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Display name with category label for UI dropdowns.</summary>
    [JsonIgnore]
    public string DisplayLabel => Id == 0 ? Name : $"{Name} ({CategoryLabel})";

    private string CategoryLabel => Category switch
    {
        ItemCategory.BodyArmor => "Armor",
        _ => Category.ToString(),
    };

    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ItemCategory Category { get; set; }

    [JsonPropertyName("attack")]
    public byte Attack { get; set; }

    [JsonPropertyName("hitRate")]
    public byte HitRate { get; set; }

    [JsonPropertyName("defense")]
    public byte Defense { get; set; }

    [JsonPropertyName("magicDefense")]
    public byte MagicDefense { get; set; }

    [JsonPropertyName("evasion")]
    public byte Evasion { get; set; }

    [JsonPropertyName("magicEvasion")]
    public byte MagicEvasion { get; set; }

    /// <summary>Stat modifier flags: bit4=Str, bit3=Agi, bit2=Sta, bit1=Int, bit0=Spi</summary>
    [JsonPropertyName("statBonuses")]
    public StatBonuses? StatBonuses { get; set; }

    /// <summary>True if this weapon is two-handed (no shield allowed).</summary>
    [JsonPropertyName("twoHanded")]
    public bool TwoHanded { get; set; }

    /// <summary>True if this weapon is a bow (needs arrow in other hand).</summary>
    [JsonPropertyName("isBow")]
    public bool IsBow { get; set; }

    /// <summary>True if this item is an arrow (used with bows).</summary>
    [JsonPropertyName("isArrow")]
    public bool IsArrow { get; set; }

    /// <summary>
    /// Bitmask of character classes that can equip this item.
    /// Matches the ROM's ItemClasses table format.
    /// </summary>
    [JsonPropertyName("equipMask")]
    public uint EquipMask { get; set; }

    public bool CanEquip(CharacterId characterId)
    {
        int classIndex = GetClassIndex(characterId);
        if (classIndex < 0) return false;
        return (EquipMask & (1u << classIndex)) != 0;
    }

    private static int GetClassIndex(CharacterId id) => id switch
    {
        CharacterId.CecilDarkKnight => 0,
        CharacterId.Kain or CharacterId.Kain2 or CharacterId.Kain3 => 1,
        CharacterId.RydiaChild => 2,
        CharacterId.Tellah or CharacterId.Tellah2 or CharacterId.Tellah3 => 3,
        CharacterId.Edward => 4,
        CharacterId.Rosa or CharacterId.Rosa2 => 5,
        CharacterId.Yang or CharacterId.Yang2 => 6,
        CharacterId.Palom => 7,
        CharacterId.Porom => 8,
        CharacterId.CecilPaladin => 9,
        CharacterId.Cid => 10,
        CharacterId.RydiaAdult => 11,
        CharacterId.Edge => 12,
        CharacterId.FuSoYa => 13,
        CharacterId.Golbez => 3, // Golbez uses Tellah's equip class
        _ => -1,
    };
}

public class StatBonuses
{
    [JsonPropertyName("strength")]
    public int Strength { get; set; }

    [JsonPropertyName("agility")]
    public int Agility { get; set; }

    [JsonPropertyName("stamina")]
    public int Stamina { get; set; }

    [JsonPropertyName("intellect")]
    public int Intellect { get; set; }

    [JsonPropertyName("spirit")]
    public int Spirit { get; set; }
}
