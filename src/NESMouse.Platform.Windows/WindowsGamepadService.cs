using NESMouse.Core.Abstractions;
using NESMouse.Core.Models;
using Vortice.DirectInput;

namespace NESMouse.Platform.Windows;

/// <summary>
/// Windows implementation of IGamepadService using Vortice.DirectInput.
/// </summary>
public class WindowsGamepadService : IGamepadService
{
    private readonly IDirectInput8 _directInput;
    private readonly Dictionary<string, IDirectInputDevice8> _devices = new();
    private IDirectInputDevice8? _selectedDevice;
    private GamepadDevice? _selectedGamepadDevice;
    private bool _disposed;

    public WindowsGamepadService()
    {
        _directInput = DInput.DirectInput8Create();
    }

    public GamepadDevice? SelectedDevice => _selectedGamepadDevice;

    public event EventHandler<DeviceChangedEventArgs>? DeviceChanged;

    public IReadOnlyList<GamepadDevice> GetConnectedDevices()
    {
        var devices = new List<GamepadDevice>();

        foreach (var deviceInstance in _directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
        {
            var id = deviceInstance.InstanceGuid.ToString();
            var name = deviceInstance.InstanceName;

            if (!_devices.ContainsKey(id))
            {
                try
                {
                    var device = _directInput.CreateDevice(deviceInstance.InstanceGuid);
                    device.SetDataFormat<RawJoystickState>();
                    device.SetCooperativeLevel(IntPtr.Zero, CooperativeLevel.NonExclusive | CooperativeLevel.Background);
                    device.Acquire();

                    // Set axis range to -100 to 100
                    foreach (var deviceObject in device.GetObjects())
                    {
                        if ((deviceObject.ObjectId.Flags & DeviceObjectTypeFlags.Axis) != 0)
                        {
                            var props = device.GetObjectPropertiesById(deviceObject.ObjectId);
                            props.Range = new InputRange(-100, 100);
                        }
                    }

                    _devices[id] = device;
                }
                catch (SharpGen.Runtime.SharpGenException)
                {
                    continue;
                }
            }

            devices.Add(new GamepadDevice(id, name));
        }

        return devices;
    }

    public void SelectDevice(GamepadDevice device)
    {
        if (_devices.TryGetValue(device.Id, out var diDevice))
        {
            _selectedDevice = diDevice;
            _selectedGamepadDevice = device;
        }
    }

    public GamepadState? Poll()
    {
        if (_selectedDevice == null)
            return null;

        try
        {
            _selectedDevice.Poll();
            var state = _selectedDevice.GetCurrentJoystickState();

            return new GamepadState(
                XAxis: state.X,
                YAxis: state.Y,
                Buttons: state.Buttons
            );
        }
        catch
        {
            // Device may have been disconnected
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var device in _devices.Values)
        {
            try
            {
                device.Unacquire();
                device.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        _devices.Clear();
        _directInput.Dispose();
    }
}
