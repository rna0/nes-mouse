namespace NESMouse.Core.Models;

/// <summary>
/// Event arguments for device connection/disconnection events.
/// </summary>
public class DeviceChangedEventArgs : EventArgs
{
    /// <summary>
    /// The device that was connected or disconnected.
    /// </summary>
    public GamepadDevice Device { get; }

    /// <summary>
    /// True if the device was connected, false if disconnected.
    /// </summary>
    public bool IsConnected { get; }

    public DeviceChangedEventArgs(GamepadDevice device, bool isConnected)
    {
        Device = device;
        IsConnected = isConnected;
    }
}
