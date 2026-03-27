using FF3SaveEditor.Core.IO;
using FF3SaveEditor.Core.Models;
using FF3SaveEditor.Plugin.ViewModels;
using SaveEditor.Shell.Abstractions;

namespace FF3SaveEditor.Plugin;

public class FF3GamePlugin : IGamePlugin
{
    private SaveFile? _saveFile;

    public string GameName => "FF3";
    public string OpenDialogTitle => "Open FF3 Save File";
    public string SaveDialogTitle => "Save FF3 Save File";
    public int SlotCount => SaveFile.SlotCount;

    public IReadOnlyList<ISlotViewModel> Load(byte[] data, Action markDirty)
    {
        _saveFile = SrmFile.LoadFromBytes(data);
        var slots = new List<ISlotViewModel>();
        for (int i = 0; i < SaveFile.SlotCount; i++)
            slots.Add(new SlotViewModel(_saveFile.Slots[i], i + 1, markDirty));
        return slots;
    }

    public byte[] Save()
    {
        if (_saveFile == null)
            throw new InvalidOperationException("No file loaded.");
        return SrmFile.SaveToBytes(_saveFile);
    }
}
