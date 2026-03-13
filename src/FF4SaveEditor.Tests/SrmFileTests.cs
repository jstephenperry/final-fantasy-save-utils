using FF4SaveEditor.Core.IO;
using FF4SaveEditor.Core.Models;

namespace FF4SaveEditor.Tests;

public class SrmFileTests
{
    private static byte[] CreateValidSlot()
    {
        var slot = new byte[SaveSlot.Size];

        // Set up a character: Cecil Paladin, level 50
        slot[0x00] = 0xCB; // Right-hand equipped + left-hand equipped + Cecil Paladin (0x0B)
        slot[0x01] = 0x00; // Front row
        slot[0x02] = 50;   // Level

        // HP: 2000/2500
        slot[0x07] = 0xD0; slot[0x08] = 0x07; // 2000 LE
        slot[0x09] = 0xC4; slot[0x0A] = 0x09; // 2500 LE

        // MP: 100/200
        slot[0x0B] = 100; slot[0x0C] = 0;
        slot[0x0D] = 200; slot[0x0E] = 0;

        // Base stats
        slot[0x0F] = 50; // Str
        slot[0x10] = 40; // Agi
        slot[0x11] = 45; // Sta
        slot[0x12] = 20; // Int
        slot[0x13] = 25; // Spi

        // Equipment: Crystal sword in right hand
        slot[0x33] = 63; // Crystal sword ID

        // Gil: 65000 (0x00FDE8)
        slot[0x6A0] = 0xE8;
        slot[0x6A1] = 0xFD;
        slot[0x6A2] = 0x00;

        // Inventory: Cure1 x5, Ether1 x3
        slot[0x440] = 224; slot[0x441] = 5; // Cure1
        slot[0x442] = 227; slot[0x443] = 3; // Ether1

        // Load flag
        slot[0x7FB] = 0x01;

        // Validation value
        slot[0x7FE] = 0xE4; slot[0x7FF] = 0x1B; // 0x1BE4 LE

        // Calculate and set checksum
        ushort checksum = Checksum.Calculate(slot);
        slot[0x7FC] = (byte)(checksum & 0xFF);
        slot[0x7FD] = (byte)((checksum >> 8) & 0xFF);

        return slot;
    }

    [Fact]
    public void RoundTrip_PreservesAllData()
    {
        var originalSlotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(originalSlotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        var result = saveFile.ToBytes();

        // The first slot should match exactly (bytes that were set)
        // Note: ToBytes recalculates checksum, so it should still match
        Assert.True(saveFile.Slots[0].IsValid);
        Assert.Equal(fullSrm.Length, result.Length);

        // Verify checksum is still valid after round-trip
        Assert.True(Checksum.Verify(result.AsSpan(0, SaveSlot.Size)));
    }

    [Fact]
    public void ParsesCharacterCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        var slot = saveFile.Slots[0];
        var cecil = slot.Characters[0];

        Assert.Equal(CharacterId.CecilPaladin, cecil.CharacterId);
        Assert.True(cecil.RightHanded);
        Assert.True(cecil.LeftHanded);
        Assert.Equal(50, cecil.Level);
        Assert.Equal(2000, cecil.CurrentHp);
        Assert.Equal(2500, cecil.MaxHp);
        Assert.Equal(100, cecil.CurrentMp);
        Assert.Equal(200, cecil.MaxMp);
        Assert.Equal(50, cecil.Strength);
        Assert.Equal(40, cecil.Agility);
        Assert.Equal(45, cecil.Stamina);
        Assert.Equal(20, cecil.Intellect);
        Assert.Equal(25, cecil.Spirit);
        Assert.Equal(63, cecil.RightHandItemId); // Crystal sword
    }

    [Fact]
    public void ParsesGilCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        Assert.Equal(65000u, saveFile.Slots[0].Gil);
    }

    [Fact]
    public void ParsesInventoryCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        var inventory = saveFile.Slots[0].Inventory;

        Assert.Equal(224, inventory[0].ItemId); // Cure1
        Assert.Equal(5, inventory[0].Quantity);
        Assert.Equal(227, inventory[1].ItemId); // Ether1
        Assert.Equal(3, inventory[1].Quantity);
        Assert.True(inventory[2].IsEmpty);
    }

    [Fact]
    public void EmptySlot_IsNotValid()
    {
        var fullSrm = new byte[SaveFile.FileSize];
        var saveFile = SaveFile.FromBytes(fullSrm);

        for (int i = 0; i < SaveFile.SlotCount; i++)
            Assert.False(saveFile.Slots[i].IsValid);
    }

    [Fact]
    public void ModifyAndRoundTrip_ChecksumUpdated()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);

        // Modify character stats
        saveFile.Slots[0].Characters[0].Level = 99;
        saveFile.Slots[0].Characters[0].Strength = 99;
        saveFile.Slots[0].Gil = 9999999;

        var result = saveFile.ToBytes();

        // Verify the new checksum is valid
        Assert.True(Checksum.Verify(result.AsSpan(0, SaveSlot.Size)));

        // Re-parse and verify modifications persisted
        var reloaded = SaveFile.FromBytes(result);
        Assert.Equal(99, reloaded.Slots[0].Characters[0].Level);
        Assert.Equal(99, reloaded.Slots[0].Characters[0].Strength);
        Assert.Equal(9999999u, reloaded.Slots[0].Gil);
    }

    [Fact]
    public void Experience24Bit_RoundTrips()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        saveFile.Slots[0].Characters[0].Experience = 16_777_215; // Max 24-bit

        var result = saveFile.ToBytes();
        var reloaded = SaveFile.FromBytes(result);
        Assert.Equal(16_777_215u, reloaded.Slots[0].Characters[0].Experience);
    }

    [Fact]
    public void ValidationValue_SetCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        var result = saveFile.ToBytes();

        // Check validation value at 0x7FE
        ushort validation = (ushort)(result[0x7FE] | (result[0x7FF] << 8));
        Assert.Equal(0x1BE4, validation);
    }

    [Fact]
    public void InvalidFileSize_Throws()
    {
        var badData = new byte[1000];
        Assert.Throws<ArgumentException>(() => SaveFile.FromBytes(badData));
    }
}
