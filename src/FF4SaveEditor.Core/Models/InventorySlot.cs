namespace FF4SaveEditor.Core.Models;

public class InventorySlot
{
    public byte ItemId { get; set; }
    public byte Quantity { get; set; }

    public bool IsEmpty => ItemId == 0 && Quantity == 0;

    public static InventorySlot FromBytes(byte itemId, byte quantity)
        => new() { ItemId = itemId, Quantity = quantity };

    public void Clear()
    {
        ItemId = 0;
        Quantity = 0;
    }
}
