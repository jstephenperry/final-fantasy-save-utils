using FF1SaveEditor.Core.IO;
using FF1SaveEditor.Core.Models;
using FF1SaveEditor.Plugin.ViewModels;
using SaveEditor.Shell.Abstractions;

namespace FF1SaveEditor.Plugin;

public class FF1GamePlugin : IGamePlugin
{
    private SaveFile? _saveFile;

    public string GameName => "FF1";
    public string OpenDialogTitle => "Open FF1 Save File";
    public string SaveDialogTitle => "Save FF1 Save File";
    public int SlotCount => SaveFile.SlotCount;

    public IReadOnlyList<ISlotViewModel> Load(byte[] data, Action markDirty)
    {
        _saveFile = SrmFile.LoadFromBytes(data);
        return new List<ISlotViewModel>
        {
            new SlotViewModel(_saveFile.Slot, 1, markDirty)
        };
    }

    public byte[] Save()
    {
        if (_saveFile == null)
            throw new InvalidOperationException("No file loaded.");
        return SrmFile.SaveToBytes(_saveFile);
    }
}
