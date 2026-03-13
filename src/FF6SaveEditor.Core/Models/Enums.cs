namespace FF6SaveEditor.Core.Models;

public enum ActorId : byte
{
    Terra = 0,
    Locke = 1,
    Cyan = 2,
    Shadow = 3,
    Edgar = 4,
    Sabin = 5,
    Celes = 6,
    Strago = 7,
    Relm = 8,
    Setzer = 9,
    Mog = 10,
    Gau = 11,
    Gogo = 12,
    Umaro = 13,
    Wedge = 0x20,
    Vicks = 0x21,
    Empty = 0xFF,
}

public enum ItemCategory
{
    Weapon,
    Shield,
    Helmet,
    Armor,
    Relic,
    Consumable,
}

public static class ActorIdExtensions
{
    private static readonly Dictionary<ActorId, string> Names = new()
    {
        [ActorId.Terra] = "Terra",
        [ActorId.Locke] = "Locke",
        [ActorId.Cyan] = "Cyan",
        [ActorId.Shadow] = "Shadow",
        [ActorId.Edgar] = "Edgar",
        [ActorId.Sabin] = "Sabin",
        [ActorId.Celes] = "Celes",
        [ActorId.Strago] = "Strago",
        [ActorId.Relm] = "Relm",
        [ActorId.Setzer] = "Setzer",
        [ActorId.Mog] = "Mog",
        [ActorId.Gau] = "Gau",
        [ActorId.Gogo] = "Gogo",
        [ActorId.Umaro] = "Umaro",
        [ActorId.Wedge] = "Wedge",
        [ActorId.Vicks] = "Vicks",
        [ActorId.Empty] = "(Empty)",
    };

    public static string GetDisplayName(this ActorId id)
        => Names.TryGetValue(id, out var name) ? name : $"Unknown (0x{(byte)id:X2})";

    public static ItemCategory GetItemCategory(byte itemId) => itemId switch
    {
        <= 0x5C => ItemCategory.Weapon,
        >= 0x5D and <= 0x66 => ItemCategory.Shield,
        >= 0x67 and <= 0x76 => ItemCategory.Helmet,
        >= 0x77 and <= 0x8D => ItemCategory.Armor,
        >= 0x8E and <= 0xB2 => ItemCategory.Relic,
        0xFF => ItemCategory.Consumable, // Empty
        _ => ItemCategory.Consumable,
    };
}
