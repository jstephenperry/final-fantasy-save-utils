namespace FF6SaveEditor.Core.Models;

public class InventorySlot
{
    public byte ItemId { get; set; } = 0xFF;
    public byte Quantity { get; set; }

    public bool IsEmpty => ItemId == 0xFF;

    public static InventorySlot FromBytes(byte itemId, byte quantity)
        => new() { ItemId = itemId, Quantity = quantity };

    public void Clear()
    {
        ItemId = 0xFF;
        Quantity = 0;
    }
}
