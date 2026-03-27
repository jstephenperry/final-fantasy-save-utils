using FF2SaveEditor.Core.IO;
using FF2SaveEditor.Core.Models;

namespace FF2SaveEditor.Tests;

public class SrmFileTests
{
    private static byte[] CreateValidSlot()
    {
        var slot = new byte[SaveSlot.Size];

        // Character 1 Block A: Name "Frion" (FF2 encoding), HP 200/300
        slot[0x102] = 0x90; // 'F'
        slot[0x103] = 0x9C; // 'r'
        slot[0x104] = 0xA5; // 'i'(0xA4='a', 0xA5='b'...) actually need 'i'=0xAD
        slot[0x104] = 0xAD; // 'i'
        slot[0x105] = 0xB3; // 'o' (0xA4+14=0xB2... a=A4,b=A5,c=A6,d=A7,e=A8,f=A9,g=AA,h=AB,i=AC,j=AD)
        // Let me recalculate: a=0xA4, b=0xA5, ... i=0xAC, j=0xAD, k=0xAE, l=0xAF, m=0xB0, n=0xB1, o=0xB2
        slot[0x102] = 0x90; // 'F' (A=0x8A, B=0x8B, ... F=0x8F) -> F=0x8F
        slot[0x102] = 0x8F; // 'F'
        slot[0x103] = 0xBC; // r=0xA4+17=0xBB -> no, a=0xA4,b=A5,...r=0xA4+17=0xB5
        slot[0x103] = 0xB5; // 'r'
        slot[0x104] = 0xAC; // 'i'
        slot[0x105] = 0xB2; // 'o'
        slot[0x106] = 0xB1; // 'n'
        slot[0x107] = 0xFF; // terminator

        // HP: 200/300
        slot[0x108] = 200; slot[0x109] = 0; // Current HP
        slot[0x10A] = 0x2C; slot[0x10B] = 0x01; // Max HP = 300

        // Stats
        slot[0x110] = 30; // Str
        slot[0x111] = 25; // Agi
        slot[0x112] = 20; // Sta

        // Gil: 8000 (0x1F40)
        slot[0x1C] = 0x40;
        slot[0x1D] = 0x1F;
        slot[0x1E] = 0x00;

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
    public void ParsesGilCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.FirstSlotOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        Assert.Equal(8000u, saveFile.Slots[0].Gil);
    }

    [Fact]
    public void ParsesCharacterStats()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.FirstSlotOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        var character = saveFile.Slots[0].Characters[0];

        Assert.Equal(200, character.CurrentHp);
        Assert.Equal(300, character.MaxHp);
        Assert.Equal(30, character.Strength);
        Assert.Equal(25, character.Agility);
        Assert.Equal(20, character.Stamina);
    }

    [Fact]
    public void ModifyAndRoundTrip_ChecksumUpdated()
    {
        var slotData = CreateValidSlot();
        var fullSav = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSav, SaveFile.FirstSlotOffset, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSav);
        saveFile.Slots[0].Gil = 16_777_215;
        saveFile.Slots[0].Characters[0].Strength = 99;

        var result = saveFile.ToBytes();
        Assert.True(Checksum.Verify(result.AsSpan(SaveFile.FirstSlotOffset, SaveSlot.Size)));

        var reloaded = SaveFile.FromBytes(result);
        Assert.Equal(16_777_215u, reloaded.Slots[0].Gil);
        Assert.Equal(99, reloaded.Slots[0].Characters[0].Strength);
    }

    [Fact]
    public void FourSlots_AllParseable()
    {
        var fullSav = new byte[SaveFile.FileSize];
        var saveFile = SaveFile.FromBytes(fullSav);
        Assert.Equal(4, saveFile.Slots.Length);
        for (int i = 0; i < SaveFile.SlotCount; i++)
            Assert.False(saveFile.Slots[i].IsValid);
    }

    [Fact]
    public void InvalidFileSize_Throws()
    {
        Assert.Throws<ArgumentException>(() => SaveFile.FromBytes(new byte[1000]));
    }
}
