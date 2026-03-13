using System.ComponentModel;

namespace SaveEditor.Shell.Abstractions;

/// <summary>
/// Minimal interface for slot view models exposed to the shell.
/// The shell only needs the header text and validity flag;
/// all other rendering is handled by game-specific DataTemplates.
/// </summary>
public interface ISlotViewModel : INotifyPropertyChanged
{
    string Header { get; }
    bool IsValid { get; }
}
