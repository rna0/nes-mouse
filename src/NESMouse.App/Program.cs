using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NESMouse.Core.Abstractions;
using NESMouse.Core.Services;
using System.Runtime.InteropServices;

namespace NESMouse.App;

class Program
{
    public static IServiceProvider? Services { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        Services = ConfigureServices();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register platform-specific implementations
        RegisterPlatformServices(services);

        // Register core services
        services.AddSingleton<GamepadToMouseController>();

        return services.BuildServiceProvider();
    }

    private static void RegisterPlatformServices(IServiceCollection services)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            RegisterWindowsServices(services);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            RegisterMacOSServices(services);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            RegisterLinuxServices(services);
        }
        else
        {
            throw new PlatformNotSupportedException("This platform is not supported.");
        }
    }

    private static void RegisterWindowsServices(IServiceCollection services)
    {
#if WINDOWS
        services.AddSingleton<IGamepadService, NESMouse.Platform.Windows.WindowsGamepadService>();
        services.AddSingleton<IMouseSimulator, NESMouse.Platform.Windows.WindowsMouseSimulator>();
#endif
    }

    private static void RegisterMacOSServices(IServiceCollection services)
    {
#if MACOS
        services.AddSingleton<IGamepadService, NESMouse.Platform.macOS.MacOSGamepadService>();
        services.AddSingleton<IMouseSimulator, NESMouse.Platform.macOS.MacOSMouseSimulator>();
#endif
    }

    private static void RegisterLinuxServices(IServiceCollection services)
    {
#if LINUX
        services.AddSingleton<IGamepadService, NESMouse.Platform.Linux.LinuxGamepadService>();
        services.AddSingleton<IMouseSimulator, NESMouse.Platform.Linux.LinuxMouseSimulator>();
#endif
    }
}
