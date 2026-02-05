using NESMouse.Core.Models;

namespace NESMouse.Core.Abstractions;

/// <summary>
/// Abstraction for gamepad input across platforms.
/// </summary>
public interface IGamepadService : IDisposable
{
    /// <summary>
    /// Gets the list of currently connected gamepad devices.
    /// </summary>
    IReadOnlyList<GamepadDevice> GetConnectedDevices();

    /// <summary>
    /// Selects a device to be the active gamepad for polling.
    /// </summary>
    /// <param name="device">The device to select.</param>
    void SelectDevice(GamepadDevice device);

    /// <summary>
    /// Gets the currently selected device, if any.
    /// </summary>
    GamepadDevice? SelectedDevice { get; }

    /// <summary>
    /// Polls the currently selected gamepad and returns its state.
    /// </summary>
    /// <returns>The current gamepad state, or null if no device is selected or available.</returns>
    GamepadState? Poll();

    /// <summary>
    /// Raised when a device is connected or disconnected.
    /// </summary>
    event EventHandler<DeviceChangedEventArgs>? DeviceChanged;
}
