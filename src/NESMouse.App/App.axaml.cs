using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NESMouse.App.ViewModels;
using NESMouse.Core.Services;

namespace NESMouse.App;

public partial class App : Application
{
    private TrayIconViewModel? _viewModel;
    private GamepadToMouseController? _controller;
    private TrayIcon? _trayIcon;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Don't show any main window - this is a tray-only app
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Initialize from DI
            if (Program.Services != null)
            {
                _controller = Program.Services.GetService<GamepadToMouseController>();

                if (_controller != null)
                {
                    _viewModel = new TrayIconViewModel(_controller, desktop);

                    // Setup tray icon
                    SetupTrayIcon();

                    // Start the controller
                    _ = _controller.StartAsync();
                }
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void SetupTrayIcon()
    {
        var menu = new NativeMenu();
        _viewModel?.SetupMenu(menu);

        _trayIcon = new TrayIcon
        {
            ToolTipText = "NES Mouse",
            Menu = menu,
            IsVisible = true
        };

        // Load icon from Avalonia assets
        try
        {
            var uri = new Uri("avares://NESMouse.App/Assets/icon.ico");
            var assets = Avalonia.Platform.AssetLoader.Open(uri);
            _trayIcon.Icon = new WindowIcon(assets);
        }
        catch
        {
            // Icon loading failed, continue without icon
        }

        // Add to application tray icons
        var icons = new TrayIcons { _trayIcon };
        SetValue(TrayIcon.IconsProperty, icons);
    }
}
