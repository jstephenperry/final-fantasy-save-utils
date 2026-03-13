using System.Text.Json.Serialization;
using FF6SaveEditor.Core.Models;

namespace FF6SaveEditor.Core.GameData;

/// <summary>
/// Definition of a single item from the FF6 game database.
/// </summary>
public class ItemDef
{
    [JsonPropertyName("id")]
    public byte Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ItemCategory Category { get; set; }

    [JsonPropertyName("equipMask")]
    public uint EquipMask { get; set; }

    public bool CanEquip(ActorId actorId)
    {
        int classIndex = GetClassIndex(actorId);
        if (classIndex < 0) return false;
        return (EquipMask & (1u << classIndex)) != 0;
    }

    /// <summary>
    /// Maps ActorId to equip class index (0-13).
    /// Terra=0, Locke=1, Cyan=2, Shadow=3, Edgar=4, Sabin=5,
    /// Celes=6, Strago=7, Relm=8, Setzer=9, Mog=10, Gau=11, Gogo=12, Umaro=13
    /// </summary>
    private static int GetClassIndex(ActorId id) => id switch
    {
        ActorId.Terra => 0,
        ActorId.Locke => 1,
        ActorId.Cyan => 2,
        ActorId.Shadow => 3,
        ActorId.Edgar => 4,
        ActorId.Sabin => 5,
        ActorId.Celes => 6,
        ActorId.Strago => 7,
        ActorId.Relm => 8,
        ActorId.Setzer => 9,
        ActorId.Mog => 10,
        ActorId.Gau => 11,
        ActorId.Gogo => 12,
        ActorId.Umaro => 13,
        ActorId.Wedge or ActorId.Vicks => 0, // Use Terra's equip class
        _ => -1,
    };
}
