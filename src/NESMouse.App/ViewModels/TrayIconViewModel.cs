using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using NESMouse.Core.Models;
using NESMouse.Core.Services;

namespace NESMouse.App.ViewModels;

public class TrayIconViewModel
{
    private readonly GamepadToMouseController _controller;
    private readonly IClassicDesktopStyleApplicationLifetime _lifetime;
    private NativeMenu? _menu;
    private NativeMenuItem? _devicesSubmenu;
    private readonly List<NativeMenuItem> _deviceMenuItems = new();

    public TrayIconViewModel(GamepadToMouseController controller, IClassicDesktopStyleApplicationLifetime lifetime)
    {
        _controller = controller;
        _lifetime = lifetime;
    }

    public void SetupMenu(NativeMenu menu)
    {
        _menu = menu;
        menu.Items.Clear();

        // Devices submenu
        _devicesSubmenu = new NativeMenuItem("Devices");
        _devicesSubmenu.Menu = new NativeMenu();
        menu.Items.Add(_devicesSubmenu);

        // Refresh devices
        RefreshDevices();

        // Separator
        menu.Items.Add(new NativeMenuItemSeparator());

        // Exit
        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += (_, _) => Exit();
        menu.Items.Add(exitItem);
    }

    public void RefreshDevices()
    {
        if (_devicesSubmenu?.Menu == null)
            return;

        _devicesSubmenu.Menu.Items.Clear();
        _deviceMenuItems.Clear();

        var devices = _controller.GamepadService.GetConnectedDevices();

        if (devices.Count == 0)
        {
            var noDevicesItem = new NativeMenuItem("No devices found");
            noDevicesItem.IsEnabled = false;
            _devicesSubmenu.Menu.Items.Add(noDevicesItem);

            // Add refresh option
            _devicesSubmenu.Menu.Items.Add(new NativeMenuItemSeparator());
            var refreshItem = new NativeMenuItem("Refresh");
            refreshItem.Click += (_, _) => RefreshDevices();
            _devicesSubmenu.Menu.Items.Add(refreshItem);
            return;
        }

        for (int i = 0; i < devices.Count; i++)
        {
            var device = devices[i];
            var menuItem = new NativeMenuItem($"{i}: {device.Name}");

            // Mark selected device
            var selectedDevice = _controller.GamepadService.SelectedDevice;
            if (selectedDevice != null && selectedDevice.Id == device.Id)
            {
                menuItem.Header = $"* {i}: {device.Name}";
            }

            var capturedDevice = device;
            menuItem.Click += (_, _) => SelectDevice(capturedDevice);

            _deviceMenuItems.Add(menuItem);
            _devicesSubmenu.Menu.Items.Add(menuItem);
        }

        // Select first device if none selected
        if (_controller.GamepadService.SelectedDevice == null && devices.Count > 0)
        {
            SelectDevice(devices[0]);
        }

        // Add refresh option
        _devicesSubmenu.Menu.Items.Add(new NativeMenuItemSeparator());
        var refreshMenuItem = new NativeMenuItem("Refresh");
        refreshMenuItem.Click += (_, _) => RefreshDevices();
        _devicesSubmenu.Menu.Items.Add(refreshMenuItem);
    }

    private void SelectDevice(GamepadDevice device)
    {
        _controller.GamepadService.SelectDevice(device);

        // Update menu to show selected device
        RefreshDevices();
    }

    private void Exit()
    {
        _controller.Stop();
        _controller.Dispose();
        _lifetime.Shutdown();
    }
}
