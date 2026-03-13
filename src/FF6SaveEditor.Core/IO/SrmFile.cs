using FF6SaveEditor.Core.Models;

namespace FF6SaveEditor.Core.IO;

/// <summary>
/// Reads and writes FF6 SNES .SRM save files (8192 bytes, 3 slots).
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
