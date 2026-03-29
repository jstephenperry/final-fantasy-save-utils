using FF1SaveEditor.Core.IO;
using FF1SaveEditor.Core.Models;

namespace FF1SaveEditor.Tests;

public class SrmFileTests
{
    private static byte[] CreateValidSlot()
    {
        var slot = new byte[SaveSlot.Size];

        // Character 1: Fighter, level 10, name "AAAA"
        slot[0x100] = 0; // Class: Fighter
        slot[0x102] = 0x8A; // 'A' in FF1 encoding
        slot[0x103] = 0x8A;
        slot[0x104] = 0x8A;
        slot[0x105] = 0x8A;
        slot[0x126] = 10; // Level

        // HP: 200/250
        slot[0x10A] = 200; slot[0x10B] = 0;
        slot[0x10C] = 250; slot[0x10D] = 0;

        // Stats
        slot[0x110] = 30; // Str
        slot[0x111] = 20; // Agi
        slot[0x112] = 15; // Int
        slot[0x113] = 25; // Vit
        slot[0x114] = 10; // Luck

        // Gil: 5000 (0x1388)
        slot[0x1C] = 0x88;
        slot[0x1D] = 0x13;
        slot[0x1E] = 0x00;

        // Compute checksum
        var span = slot.AsSpan();
        slot[Checksum.ChecksumOffset] = Checksum.ComputeChecksumByte(span);

        return slot;
    }

    [Fact]
    public void RoundTrip_PreservesAllData()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        // Write to validated region
        Array.Copy(slotData, 0, fullSav, SaveFile.ValidatedRegionOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        var result = saveFile.ToBytes();

        Assert.True(saveFile.Slot.IsValid);
        Assert.Equal(fullSav.Length, result.Length);

        // Verify checksum is valid in both regions
        Assert.True(Checksum.Verify(result.AsSpan(SaveFile.ValidatedRegionOffset, SaveSlot.Size)));
    }

    [Fact]
    public void ParsesCharacterCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.ValidatedRegionOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        var character = saveFile.Slot.Characters[0];

        Assert.Equal(0, character.ClassId); // Fighter
        Assert.Equal("AAAA", character.Name);
        Assert.Equal(10, character.Level);
        Assert.Equal(200, character.CurrentHp);
        Assert.Equal(250, character.MaxHp);
        Assert.Equal(30, character.Strength);
        Assert.Equal(20, character.Agility);
        Assert.Equal(15, character.Intelligence);
        Assert.Equal(25, character.Vitality);
        Assert.Equal(10, character.Luck);
    }

    [Fact]
    public void ParsesGilCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.ValidatedRegionOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        Assert.Equal(5000u, saveFile.Slot.Gil);
    }

    [Fact]
    public void EmptyFile_SlotNotValid()
    {
        var fullSav = new byte[SaveFile.FileSize];
        var saveFile = SaveFile.FromBytes(fullSav);
        Assert.False(saveFile.Slot.IsValid);
    }

    [Fact]
    public void ModifyAndRoundTrip_ChecksumUpdated()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.ValidatedRegionOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        saveFile.Slot.Characters[0].Level = 50;
        saveFile.Slot.Gil = 999_999;

        var result = saveFile.ToBytes();
        Assert.True(Checksum.Verify(result.AsSpan(SaveFile.ValidatedRegionOffset, SaveSlot.Size)));

        var reloaded = SaveFile.FromBytes(result);
        Assert.Equal(50, reloaded.Slot.Characters[0].Level);
        Assert.Equal(999_999u, reloaded.Slot.Gil);
    }

    [Fact]
    public void InvalidFileSize_Throws()
    {
        var badData = new byte[1000];
        Assert.Throws<ArgumentException>(() => SaveFile.FromBytes(badData));
    }
}
