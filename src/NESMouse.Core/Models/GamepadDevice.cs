namespace NESMouse.Core.Models;

/// <summary>
/// Represents a connected gamepad device.
/// </summary>
/// <param name="Id">Unique identifier for the device.</param>
/// <param name="Name">Human-readable name of the device.</param>
public record GamepadDevice(string Id, string Name);
