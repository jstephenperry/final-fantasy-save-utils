using FF1SaveEditor.Core.Models;

namespace FF1SaveEditor.Core.IO;

/// <summary>
/// Reads and writes FF1 NES .SAV save files (8192 bytes SRAM, 1 save slot).
/// </summary>
public static class SrmFile
{
    public static SaveFile Load(string path)
    {
        var data = File.ReadAllBytes(path);
        return SaveFile.FromBytes(data);
    }

    public static void Save(string path, SaveFile saveFile)
    {
        var data = saveFile.ToBytes();
        File.WriteAllBytes(path, data);
    }

    public static SaveFile LoadFromBytes(byte[] data)
    {
        return SaveFile.FromBytes(data);
    }

    public static byte[] SaveToBytes(SaveFile saveFile)
    {
        return saveFile.ToBytes();
    }
}
