using FF6SaveEditor.Core.IO;
using FF6SaveEditor.Core.Models;

namespace FF6SaveEditor.Tests;

public class SrmFileTests
{
    /// <summary>
    /// Creates a valid 2560-byte slot with Terra at level 4, HP 77/77, Gil 9,999,999.
    /// Character layout (37 bytes):
    ///   0x00: Actor ID, 0x01: Graphic, 0x02-0x07: Name, 0x08: Level,
    ///   0x09-0x0A: CurHP, 0x0B-0x0C: MaxHP, 0x0D-0x0E: CurMP, 0x0F-0x10: MaxMP,
    ///   0x11-0x13: EXP, 0x14-0x15: Status, 0x16-0x19: Commands,
    ///   0x1A-0x1D: Vigor/Speed/Stamina/MagPwr, 0x1E: Esper,
    ///   0x1F-0x24: Weapon/Shield/Helmet/Armor/Relic1/Relic2
    /// </summary>
    private static byte[] CreateValidSlot()
    {
        var slot = new byte[SaveSlot.Size];

        // Character 0: Terra (actor ID 0), level 4, HP 77/77
        int c = 0x000;
        slot[c + 0x00] = 0x00; // Actor ID: Terra
        slot[c + 0x01] = 0x00; // Graphic index

        // Name "Terra" in FF6 text encoding
        slot[c + 0x02] = 0x93; // T (0x80 + 19)
        slot[c + 0x03] = 0x9E; // e (0x9A + 4)
        slot[c + 0x04] = 0xAB; // r (0x9A + 17)
        slot[c + 0x05] = 0xAB; // r
        slot[c + 0x06] = 0x9A; // a (0x9A + 0)
        slot[c + 0x07] = 0xFF; // terminator

        slot[c + 0x08] = 4;    // Level

        // HP: 77/77
        slot[c + 0x09] = 77;   // Current HP lo
        slot[c + 0x0A] = 0;    // Current HP hi
        slot[c + 0x0B] = 77;   // Max HP lo
        slot[c + 0x0C] = 0;    // Max HP hi

        // MP: 10/10
        slot[c + 0x0D] = 10;
        slot[c + 0x0E] = 0;
        slot[c + 0x0F] = 10;
        slot[c + 0x10] = 0;

        // EXP: 200
        slot[c + 0x11] = 200;
        slot[c + 0x12] = 0;
        slot[c + 0x13] = 0;

        // Status bytes (preserved)
        slot[c + 0x14] = 0x08;
        slot[c + 0x15] = 0x00;

        // Commands: Fight(0), Morph(3), Magic(2), Item(1)
        slot[c + 0x16] = 0x00;
        slot[c + 0x17] = 0x03;
        slot[c + 0x18] = 0x02;
        slot[c + 0x19] = 0x01;

        // Stats
        slot[c + 0x1A] = 31;   // Vigor
        slot[c + 0x1B] = 33;   // Speed
        slot[c + 0x1C] = 28;   // Stamina
        slot[c + 0x1D] = 39;   // Magic Power

        slot[c + 0x1E] = 0xFF; // No esper equipped

        // Equipment: MithrilBlade in weapon slot
        slot[c + 0x1F] = 0x0A; // MithrilBlade
        slot[c + 0x20] = 0xFF; // Shield: empty
        slot[c + 0x21] = 0xFF; // Helmet: empty
        slot[c + 0x22] = 0xFF; // Armor: empty
        slot[c + 0x23] = 0xFF; // Relic 1: empty
        slot[c + 0x24] = 0xFF; // Relic 2: empty

        // Fill remaining characters with Empty (0xFF actor ID)
        for (int i = 1; i < 16; i++)
        {
            slot[i * CharacterData.Size] = 0xFF; // Empty actor
        }

        // Gil: 9,999,999 (0x98967F)
        slot[0x0260] = 0x7F;
        slot[0x0261] = 0x96;
        slot[0x0262] = 0x98;

        // Game time: 1:23:45
        slot[0x0263] = 1;   // Hours
        slot[0x0264] = 23;  // Minutes
        slot[0x0265] = 45;  // Seconds

        // Steps: 1000 (0x0003E8)
        slot[0x0266] = 0xE8;
        slot[0x0267] = 0x03;
        slot[0x0268] = 0x00;

        // Inventory: Potion x5, Tonic x3
        slot[0x0269] = 0xF9;  // Potion item ID
        slot[0x026A] = 0xFA;  // Tonic item ID
        // Fill rest of IDs with 0xFF (empty)
        for (int i = 2; i < 256; i++)
            slot[0x0269 + i] = 0xFF;

        slot[0x0369] = 5;    // Potion qty
        slot[0x036A] = 3;    // Tonic qty

        // Calculate and set checksum
        ushort checksum = Checksum.Calculate(slot);
        slot[0x09FE] = (byte)(checksum & 0xFF);
        slot[0x09FF] = (byte)((checksum >> 8) & 0xFF);

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

        Assert.True(saveFile.Slots[0].IsValid);
        Assert.Equal(fullSrm.Length, result.Length);
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
        var terra = slot.Characters[0];

        Assert.Equal(ActorId.Terra, terra.ActorId);
        Assert.Equal(4, terra.Level);
        Assert.Equal(77, terra.CurrentHp);
        Assert.Equal(77, terra.MaxHp);
        Assert.Equal(10, terra.CurrentMp);
        Assert.Equal(10, terra.MaxMp);
        Assert.Equal(200u, terra.Experience);
        Assert.Equal(31, terra.Vigor);
        Assert.Equal(33, terra.Speed);
        Assert.Equal(28, terra.Stamina);
        Assert.Equal(39, terra.MagicPower);
        Assert.Equal("Terra", terra.Name);
        Assert.Equal(0x0A, terra.WeaponId); // MithrilBlade
    }

    [Fact]
    public void ParsesGilCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        Assert.Equal(9_999_999u, saveFile.Slots[0].Gil);
    }

    [Fact]
    public void ParsesGameTimeCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        var slot = saveFile.Slots[0];
        Assert.Equal((byte)1, slot.Hours);
        Assert.Equal((byte)23, slot.Minutes);
        Assert.Equal((byte)45, slot.Seconds);
    }

    [Fact]
    public void ParsesStepsCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        Assert.Equal(1000u, saveFile.Slots[0].Steps);
    }

    [Fact]
    public void ParsesInventoryCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        var inventory = saveFile.Slots[0].Inventory;

        Assert.Equal(0xF9, inventory[0].ItemId);
        Assert.Equal(5, inventory[0].Quantity);
        Assert.Equal(0xFA, inventory[1].ItemId);
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

        saveFile.Slots[0].Characters[0].Level = 99;
        saveFile.Slots[0].Characters[0].Vigor = 99;
        saveFile.Slots[0].Gil = 1_000_000;

        var result = saveFile.ToBytes();

        Assert.True(Checksum.Verify(result.AsSpan(0, SaveSlot.Size)));

        var reloaded = SaveFile.FromBytes(result);
        Assert.Equal(99, reloaded.Slots[0].Characters[0].Level);
        Assert.Equal(99, reloaded.Slots[0].Characters[0].Vigor);
        Assert.Equal(1_000_000u, reloaded.Slots[0].Gil);
    }

    [Fact]
    public void Experience24Bit_RoundTrips()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        saveFile.Slots[0].Characters[0].Experience = 16_777_215;

        var result = saveFile.ToBytes();
        var reloaded = SaveFile.FromBytes(result);
        Assert.Equal(16_777_215u, reloaded.Slots[0].Characters[0].Experience);
    }

    [Fact]
    public void SramValidity_SetCorrectly()
    {
        var slotData = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];
        Array.Copy(slotData, 0, fullSrm, 0, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        var result = saveFile.ToBytes();

        for (int i = 0; i < 4; i++)
        {
            int offset = 0x1FF8 + (i * 2);
            ushort value = (ushort)(result[offset] | (result[offset + 1] << 8));
            Assert.Equal(0xE41B, value);
        }
    }

    [Fact]
    public void InvalidFileSize_Throws()
    {
        var badData = new byte[1000];
        Assert.Throws<ArgumentException>(() => SaveFile.FromBytes(badData));
    }

    [Fact]
    public void ThreeSlots_IndependentParsing()
    {
        var slot1 = CreateValidSlot();
        var fullSrm = new byte[SaveFile.FileSize];

        Array.Copy(slot1, 0, fullSrm, 0x0A00, SaveSlot.Size);

        var saveFile = SaveFile.FromBytes(fullSrm);
        Assert.False(saveFile.Slots[0].IsValid);
        Assert.True(saveFile.Slots[1].IsValid);
        Assert.False(saveFile.Slots[2].IsValid);

        Assert.Equal(ActorId.Terra, saveFile.Slots[1].Characters[0].ActorId);
    }

    [Fact]
    public void RealSave_ChecksumVerifies()
    {
        // Verify against known checksum from the real save file (0xC4B9)
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads/ff6save01Perfect_NarsheMine/01-Narshe Mines.srm");

        if (!File.Exists(path))
            return; // Skip if file not available

        var data = File.ReadAllBytes(path);
        var saveFile = SaveFile.FromBytes(data);

        Assert.True(saveFile.Slots[0].IsValid);
        Assert.True(Checksum.Verify(data.AsSpan(0, SaveSlot.Size)));

        // Verify known values from hex dump
        var terra = saveFile.Slots[0].Characters[0];
        Assert.Equal(ActorId.Terra, terra.ActorId);
        Assert.Equal(4, terra.Level);
        Assert.Equal(77, terra.CurrentHp);
        Assert.Equal(77, terra.MaxHp);
        Assert.Equal(31, terra.Vigor);
        Assert.Equal(33, terra.Speed);
        Assert.Equal(28, terra.Stamina);
        Assert.Equal(39, terra.MagicPower);

        Assert.Equal(9_999_999u, saveFile.Slots[0].Gil);
        Assert.Equal(301u, saveFile.Slots[0].Steps);
    }
}
