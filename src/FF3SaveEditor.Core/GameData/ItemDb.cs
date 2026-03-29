using System.Reflection;
using System.Text.Json;

namespace FF3SaveEditor.Core.GameData;

public sealed class ItemDb
{
    private static readonly Lazy<ItemDb> _instance = new(() => new ItemDb());
    public static ItemDb Instance => _instance.Value;

    private readonly ItemDef[] _items;
    private readonly Dictionary<byte, ItemDef> _byId;

    private ItemDb()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "FF3SaveEditor.Core.GameData.Items.json";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        _items = JsonSerializer.Deserialize<ItemDef[]>(stream)
            ?? throw new InvalidOperationException("Failed to deserialize Items.json.");
        _byId = _items.ToDictionary(i => i.Id);
    }

    public ItemDef GetById(byte id)
        => _byId.TryGetValue(id, out var item) ? item : new ItemDef { Id = id, Name = $"Unknown (0x{id:X2})" };

    public IReadOnlyList<ItemDef> All => _items;

    public IEnumerable<ItemDef> GetByCategory(ItemCategory category)
        => _items.Where(i => i.Category == category);
}
