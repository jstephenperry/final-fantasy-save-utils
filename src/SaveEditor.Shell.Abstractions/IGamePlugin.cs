namespace SaveEditor.Shell.Abstractions;

/// <summary>
/// Interface for game-specific save editor plugins.
/// Each game (FF4, FF6, etc.) implements this to integrate with the shared UI shell.
/// </summary>
public interface IGamePlugin
{
    /// <summary>Short game name for display (e.g., "FF4", "FF6").</summary>
    string GameName { get; }

    /// <summary>Full title for file dialogs (e.g., "Open FF4 Save File").</summary>
    string OpenDialogTitle { get; }

    /// <summary>Full title for save dialogs.</summary>
    string SaveDialogTitle { get; }

    /// <summary>Number of save slots in this game's format.</summary>
    int SlotCount { get; }

    /// <summary>
    /// Load raw SRAM bytes, parse into internal state, and return slot view models.
    /// </summary>
    IReadOnlyList<ISlotViewModel> Load(byte[] data, Action markDirty);

    /// <summary>
    /// Serialize current state back to raw SRAM bytes.
    /// </summary>
    byte[] Save();
}
