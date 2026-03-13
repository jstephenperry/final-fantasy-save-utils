using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FF4SaveEditor.Plugin;
using FF6SaveEditor.Plugin;
using SaveEditor.Shell.Abstractions;
using SaveEditor.Shell.ViewModels;
using SaveEditor.Shell.Views;

namespace SaveEditor.Shell;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var plugins = new IGamePlugin[]
            {
                new FF4GamePlugin(),
                new FF6GamePlugin(),
            };

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(plugins)
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
