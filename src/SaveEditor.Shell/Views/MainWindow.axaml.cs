using Avalonia.Controls;
using Avalonia.Interactivity;
using SaveEditor.Shell.ViewModels;

namespace SaveEditor.Shell.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            await vm.OpenFile(this);
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            await vm.SaveFile(this);
    }

    private async void OnSaveAsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            await vm.SaveFileAs(this);
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
