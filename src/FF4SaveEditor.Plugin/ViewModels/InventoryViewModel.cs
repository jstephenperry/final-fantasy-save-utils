using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FF4SaveEditor.Core.GameData;
using FF4SaveEditor.Core.Models;

namespace FF4SaveEditor.Plugin.ViewModels;

public enum InventorySortMode
{
    Default,
    ItemsFirst,
    WeaponsFirst,
    ArmorFirst,
}

public partial class InventoryViewModel : ObservableObject
{
    private readonly InventorySlot[] _inventory;
    private readonly Action _markDirty;
    public ObservableCollection<InventorySlotViewModel> Items { get; } = new();

    public InventorySortMode[] SortModes { get; } = Enum.GetValues<InventorySortMode>();

    public InventoryViewModel(InventorySlot[] inventory, Action markDirty)
    {
        _inventory = inventory;
        _markDirty = markDirty;
        for (int i = 0; i < inventory.Length; i++)
        {
            Items.Add(new InventorySlotViewModel(inventory[i], i, markDirty));
        }
    }

    [RelayCommand]
    private void SortInventory(InventorySortMode mode)
    {
        var sorted = _inventory
            .Select(s => (id: s.ItemId, qty: s.Quantity))
            .OrderBy(s => IsEmpty(s) ? 1 : 0)
            .ThenBy(s => IsEmpty(s) ? 0 : GetCategoryPriority(ItemDb.Instance.GetById(s.id).Category, mode))
            .ThenBy(s => s.id)
            .ToArray();

        ApplyAndRefresh(sorted);
    }

    [RelayCommand]
    private void ConsolidateItems()
    {
        var consolidated = _inventory
            .Where(s => !IsEmpty((s.ItemId, s.Quantity)))
            .GroupBy(s => s.ItemId)
            .SelectMany(g =>
            {
                int total = g.Sum(s => s.Quantity);
                var stacks = new List<(byte id, byte qty)>();
                while (total > 0)
                {
                    byte qty = (byte)Math.Min(total, 99);
                    stacks.Add((g.Key, qty));
                    total -= qty;
                }
                return stacks;
            })
            .ToList();

        while (consolidated.Count < _inventory.Length)
            consolidated.Add((0, 0));

        ApplyAndRefresh(consolidated.ToArray());
    }

    private void ApplyAndRefresh((byte id, byte qty)[] values)
    {
        for (int i = 0; i < _inventory.Length; i++)
        {
            _inventory[i].ItemId = values[i].id;
            _inventory[i].Quantity = values[i].qty;
        }

        Items.Clear();
        for (int i = 0; i < _inventory.Length; i++)
            Items.Add(new InventorySlotViewModel(_inventory[i], i, _markDirty));

        _markDirty();
    }

    private static bool IsEmpty((byte id, byte qty) s)
        => s.id is 0x00 or 0xFE or 0xFF || s.qty == 0;

    private static int GetCategoryPriority(ItemCategory cat, InventorySortMode mode) => mode switch
    {
        InventorySortMode.ItemsFirst => cat switch
        {
            ItemCategory.Consumable => 0,
            ItemCategory.Weapon => 1,
            ItemCategory.Shield => 2,
            ItemCategory.Helmet => 3,
            ItemCategory.BodyArmor => 4,
            ItemCategory.Accessory => 5,
            _ => 6,
        },
        InventorySortMode.WeaponsFirst => cat switch
        {
            ItemCategory.Weapon => 0,
            ItemCategory.Shield => 1,
            ItemCategory.Helmet => 2,
            ItemCategory.BodyArmor => 3,
            ItemCategory.Accessory => 4,
            ItemCategory.Consumable => 5,
            _ => 6,
        },
        InventorySortMode.ArmorFirst => cat switch
        {
            ItemCategory.Helmet => 0,
            ItemCategory.BodyArmor => 1,
            ItemCategory.Shield => 2,
            ItemCategory.Accessory => 3,
            ItemCategory.Weapon => 4,
            ItemCategory.Consumable => 5,
            _ => 6,
        },
        _ => 0,
    };
}

public partial class InventorySlotViewModel : ObservableObject
{
    private readonly InventorySlot _slot;
    private readonly Action _markDirty;

    public int Index { get; }
    public int SlotNumber => Index + 1;
    public ObservableCollection<ItemDef> AllItems { get; } = new();

    [ObservableProperty]
    private ItemDef? _selectedItem;

    [ObservableProperty]
    private byte _quantity;

    public InventorySlotViewModel(InventorySlot slot, int index, Action markDirty)
    {
        _slot = slot;
        _markDirty = markDirty;
        Index = index;

        var db = ItemDb.Instance;
        foreach (var item in db.All)
            AllItems.Add(item);

        _selectedItem = db.GetById(slot.ItemId);
        _quantity = slot.Quantity;
    }

    partial void OnSelectedItemChanged(ItemDef? value)
    {
        if (value != null)
        {
            _slot.ItemId = value.Id;
            _markDirty();
        }
    }

    partial void OnQuantityChanged(byte value)
    {
        _slot.Quantity = value;
        _markDirty();
    }
}
