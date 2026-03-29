using FF3SaveEditor.Core.IO;
using FF3SaveEditor.Core.Models;

namespace FF3SaveEditor.Tests;

public class SrmFileTests
{
    private static byte[] CreateValidSlot()
    {
        var slot = new byte[SaveSlot.Size];

        // Character 1 Block A: Fighter, level 15
        slot[0x100] = 1; // Job: Fighter
        slot[0x101] = 15; // Level
        // Name: "AAAA" in FF3 encoding
        slot[0x106] = 0x8A; slot[0x107] = 0x8A; slot[0x108] = 0x8A; slot[0x109] = 0x8A;
        slot[0x10A] = 0xFF; slot[0x10B] = 0xFF;
        // HP: 300/400
        slot[0x10C] = 0x2C; slot[0x10D] = 0x01; // 300
        slot[0x10E] = 0x90; slot[0x10F] = 0x01; // 400
        // Stats
        slot[0x112] = 30; // Str
        slot[0x113] = 20; // Agi
        slot[0x114] = 25; // Vit

        // Gil: 10000 (0x2710)
        slot[0x1C] = 0x10;
        slot[0x1D] = 0x27;
        slot[0x1E] = 0x00;

        // Inventory: item 80 x 5
        slot[0xC0] = 80;
        slot[0xE0] = 5;

        // Set validity and checksum
        slot[Checksum.ValidityOffset] = Checksum.ValidityMarker;
        var span = slot.AsSpan();
        slot[Checksum.ChecksumOffset] = Checksum.ComputeChecksumByte(span);

        return slot;
    }

    [Fact]
    public void RoundTrip_PreservesAllData()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.FirstSlotOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        var result = saveFile.ToBytes();

        Assert.True(saveFile.Slots[0].IsValid);
        Assert.Equal(fullSav.Length, result.Length);
        Assert.True(Checksum.Verify(result.AsSpan(SaveFile.FirstSlotOffset, SaveSlot.Size)));
    }

    [Fact]
    public void ParsesCharacterCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.FirstSlotOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        var character = saveFile.Slots[0].Characters[0];

        Assert.Equal(1, character.JobId); // Fighter
        Assert.Equal(15, character.Level);
        Assert.Equal("AAAA", character.Name);
        Assert.Equal(300, character.CurrentHp);
        Assert.Equal(400, character.MaxHp);
        Assert.Equal(30, character.Strength);
        Assert.Equal(20, character.Agility);
        Assert.Equal(25, character.Vitality);
    }

    [Fact]
    public void ParsesGilCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.FirstSlotOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        Assert.Equal(10000u, saveFile.Slots[0].Gil);
    }

    [Fact]
    public void ParsesInventoryCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.FirstSlotOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        Assert.Equal(80, saveFile.Slots[0].Inventory[0].ItemId);
        Assert.Equal(5, saveFile.Slots[0].Inventory[0].Quantity);
    }

    [Fact]
    public void ModifyAndRoundTrip_ChecksumUpdated()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.FirstSlotOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        saveFile.Slots[0].Characters[0].Level = 99;
        saveFile.Slots[0].Gil = 9_999_999;

        var result = saveFile.ToBytes();
        Assert.True(Checksum.Verify(result.AsSpan(SaveFile.FirstSlotOffset, SaveSlot.Size)));

        var reloaded = SaveFile.FromBytes(result);
        Assert.Equal(99, reloaded.Slots[0].Characters[0].Level);
        Assert.Equal(9_999_999u, reloaded.Slots[0].Gil);
    }

    [Fact]
    public void EmptySlots_NotValid()
    {
        var fullSav = new byte[SaveFile.FileSize];
        var saveFile = SaveFile.FromBytes(fullSav);
        for (int i = 0; i < SaveFile.SlotCount; i++)
            Assert.False(saveFile.Slots[i].IsValid);
    }

    [Fact]
    public void InvalidFileSize_Throws()
    {
        Assert.Throws<ArgumentException>(() => SaveFile.FromBytes(new byte[1000]));
    }
}
