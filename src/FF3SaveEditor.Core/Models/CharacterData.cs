using FF3SaveEditor.Core.GameData;

namespace FF3SaveEditor.Core.Models;

/// <summary>
/// Represents a character from an FF3 save slot.
/// FF3 splits character data into two 64-byte blocks (Part A at $100, Part B at $200).
/// This class manages both blocks for one character.
/// </summary>
public class CharacterData
{
    public const int BlockSize = 64;

    private readonly byte[] _rawA = new byte[BlockSize]; // Block 1: combat stats
    private readonly byte[] _rawB = new byte[BlockSize]; // Block 2: equipment, job levels

    // --- Block A: Combat stats ($100+n*$40) ---

    // +$00: Job ID
    public byte JobId
    {
        get => _rawA[0x00];
        set => _rawA[0x00] = value;
    }

    // +$01: Level
    public byte Level
    {
        get => _rawA[0x01];
        set => _rawA[0x01] = Math.Clamp(value, (byte)1, (byte)99);
    }

    // +$02: Status
    public byte Status
    {
        get => _rawA[0x02];
        set => _rawA[0x02] = value;
    }

    // +$03-$05: Experience (24-bit LE)
    public uint Experience
    {
        get => (uint)(_rawA[0x03] | (_rawA[0x04] << 8) | (_rawA[0x05] << 16));
        set
        {
            var clamped = Math.Min(value, 16_777_215u);
            _rawA[0x03] = (byte)(clamped & 0xFF);
            _rawA[0x04] = (byte)((clamped >> 8) & 0xFF);
            _rawA[0x05] = (byte)((clamped >> 16) & 0xFF);
        }
    }

    // +$06-$0B: Name (6 bytes)
    public string Name
    {
        get => TextEncoding.Decode(_rawA.AsSpan(0x06, 6));
        set
        {
            var encoded = TextEncoding.Encode(value, 6);
            Array.Copy(encoded, 0, _rawA, 0x06, 6);
        }
    }

    // +$0C-$0D: Current HP
    public ushort CurrentHp
    {
        get => ReadUInt16LE(_rawA, 0x0C);
        set => WriteUInt16LE(_rawA, 0x0C, Math.Min(value, (ushort)9999));
    }

    // +$0E-$0F: Max HP
    public ushort MaxHp
    {
        get => ReadUInt16LE(_rawA, 0x0E);
        set => WriteUInt16LE(_rawA, 0x0E, Math.Min(value, (ushort)9999));
    }

    // +$12: Strength
    public byte Strength { get => _rawA[0x12]; set => _rawA[0x12] = Math.Min(value, (byte)99); }
    // +$13: Agility
    public byte Agility { get => _rawA[0x13]; set => _rawA[0x13] = Math.Min(value, (byte)99); }
    // +$14: Vitality
    public byte Vitality { get => _rawA[0x14]; set => _rawA[0x14] = Math.Min(value, (byte)99); }
    // +$10: Intelligence
    public byte Intelligence { get => _rawA[0x10]; set => _rawA[0x10] = Math.Min(value, (byte)99); }
    // +$11: Spirit
    public byte Spirit { get => _rawA[0x11]; set => _rawA[0x11] = Math.Min(value, (byte)99); }

    // +$30-$3F: Current/Max MP per level (8 levels x 2 bytes)
    public byte GetCurrentMp(int level) => _rawA[0x30 + level * 2];
    public void SetCurrentMp(int level, byte value) => _rawA[0x30 + level * 2] = value;
    public byte GetMaxMp(int level) => _rawA[0x31 + level * 2];
    public void SetMaxMp(int level, byte value) => _rawA[0x31 + level * 2] = value;

    // --- Block B: Equipment, job levels ($200+n*$40) ---

    // +$00: Helmet
    public byte HelmetId { get => _rawB[0x00]; set => _rawB[0x00] = value; }
    // +$01: Armor
    public byte ArmorId { get => _rawB[0x01]; set => _rawB[0x01] = value; }
    // +$02: Gloves (not present in all docs, may be shield)
    public byte GlovesId { get => _rawB[0x02]; set => _rawB[0x02] = value; }
    // +$03: Right hand weapon
    public byte RightHandId { get => _rawB[0x03]; set => _rawB[0x03] = value; }
    // +$04: Right hand arrow quantity
    public byte RightHandArrowQty { get => _rawB[0x04]; set => _rawB[0x04] = value; }
    // +$05: Left hand weapon
    public byte LeftHandId { get => _rawB[0x05]; set => _rawB[0x05] = value; }
    // +$06: Left hand arrow quantity
    public byte LeftHandArrowQty { get => _rawB[0x06]; set => _rawB[0x06] = value; }

    // +$10-$3B: Job levels/experience (22 jobs x 2 bytes: level, exp)
    public byte GetJobLevel(int jobIndex) => _rawB[0x10 + jobIndex * 2];
    public void SetJobLevel(int jobIndex, byte value) => _rawB[0x10 + jobIndex * 2] = value;
    public byte GetJobExp(int jobIndex) => _rawB[0x11 + jobIndex * 2];
    public void SetJobExp(int jobIndex, byte value) => _rawB[0x11 + jobIndex * 2] = value;

    public bool IsEmpty => _rawA[0x01] == 0 && _rawA[0x06] == 0xFF;

    public string DisplayName => IsEmpty ? "(Empty)" : Name;
    public string JobName => JobDb.GetJobName(JobId);

    public ReadOnlySpan<byte> RawDataA => _rawA;
    public ReadOnlySpan<byte> RawDataB => _rawB;

    public static CharacterData FromBytes(ReadOnlySpan<byte> blockA, ReadOnlySpan<byte> blockB)
    {
        var character = new CharacterData();
        blockA[..BlockSize].CopyTo(character._rawA);
        blockB[..BlockSize].CopyTo(character._rawB);
        return character;
    }

    public void WriteATo(Span<byte> destination) => _rawA.CopyTo(destination);
    public void WriteBTo(Span<byte> destination) => _rawB.CopyTo(destination);

    private static ushort ReadUInt16LE(byte[] data, int offset)
        => (ushort)(data[offset] | (data[offset + 1] << 8));

    private static void WriteUInt16LE(byte[] data, int offset, ushort value)
    {
        data[offset] = (byte)(value & 0xFF);
        data[offset + 1] = (byte)((value >> 8) & 0xFF);
    }
}
