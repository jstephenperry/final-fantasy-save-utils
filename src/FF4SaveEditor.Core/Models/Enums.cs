namespace FF4SaveEditor.Core.Models;

public enum CharacterId : byte
{
    None = 0x00,
    CecilDarkKnight = 0x01,
    Kain = 0x02,
    RydiaChild = 0x03,
    Tellah = 0x04,
    Edward = 0x05,
    Rosa = 0x06,
    Yang = 0x07,
    Palom = 0x08,
    Porom = 0x09,
    Tellah2 = 0x0A,
    CecilPaladin = 0x0B,
    Tellah3 = 0x0C,
    Yang2 = 0x0D,
    Cid = 0x0E,
    Kain2 = 0x0F,
    Rosa2 = 0x10,
    RydiaAdult = 0x11,
    Edge = 0x12,
    FuSoYa = 0x13,
    Kain3 = 0x14,
    Golbez = 0x15,
}

public enum ItemCategory
{
    Weapon,
    BodyArmor,
    Shield,
    Helmet,
    Accessory,
    Consumable,
}

public enum EquipSlot
{
    RightHand,
    LeftHand,
    Helmet,
    Armor,
    Accessory,
}

public enum OptimizeProfile
{
    Balanced,
    PhysicalAttack,
    MagicPower,
    Defense,
}

public static class CharacterIdExtensions
{
    private static readonly Dictionary<CharacterId, string> Names = new()
    {
        [CharacterId.None] = "(Empty)",
        [CharacterId.CecilDarkKnight] = "Cecil (Dark Knight)",
        [CharacterId.Kain] = "Kain",
        [CharacterId.RydiaChild] = "Rydia (Child)",
        [CharacterId.Tellah] = "Tellah",
        [CharacterId.Edward] = "Edward",
        [CharacterId.Rosa] = "Rosa",
        [CharacterId.Yang] = "Yang",
        [CharacterId.Palom] = "Palom",
        [CharacterId.Porom] = "Porom",
        [CharacterId.Tellah2] = "Tellah",
        [CharacterId.CecilPaladin] = "Cecil (Paladin)",
        [CharacterId.Tellah3] = "Tellah",
        [CharacterId.Yang2] = "Yang",
        [CharacterId.Cid] = "Cid",
        [CharacterId.Kain2] = "Kain",
        [CharacterId.Rosa2] = "Rosa",
        [CharacterId.RydiaAdult] = "Rydia (Adult)",
        [CharacterId.Edge] = "Edge",
        [CharacterId.FuSoYa] = "FuSoYa",
        [CharacterId.Kain3] = "Kain",
        [CharacterId.Golbez] = "Golbez",
    };

    public static string GetDisplayName(this CharacterId id)
        => Names.TryGetValue(id, out var name) ? name : $"Unknown (0x{(byte)id:X2})";

    /// <summary>
    /// Returns the canonical character class for equipment lookups.
    /// Multiple IDs can map to the same class (e.g. Kain, Kain2, Kain3 all map to Kain).
    /// </summary>
    public static CharacterId GetEquipClass(this CharacterId id) => id switch
    {
        CharacterId.Tellah2 or CharacterId.Tellah3 => CharacterId.Tellah,
        CharacterId.Yang2 => CharacterId.Yang,
        CharacterId.Kain2 or CharacterId.Kain3 => CharacterId.Kain,
        CharacterId.Rosa2 => CharacterId.Rosa,
        CharacterId.RydiaAdult => CharacterId.RydiaChild, // Same equip class
        _ => id,
    };

    public static ItemCategory GetItemCategory(byte itemId) => itemId switch
    {
        <= 0x5F => ItemCategory.Weapon,
        0x60 => ItemCategory.BodyArmor,     // Karate outfit at 0x60
        >= 0x61 and <= 0x6C => ItemCategory.Shield,
        >= 0x6D and <= 0x80 => ItemCategory.Helmet,
        >= 0x81 and <= 0x9B => ItemCategory.BodyArmor,
        >= 0x9C and <= 0xAF => ItemCategory.Accessory,
        _ => ItemCategory.Consumable,
    };
}
