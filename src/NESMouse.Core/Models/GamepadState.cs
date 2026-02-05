namespace NESMouse.Core.Models;

/// <summary>
/// Represents the current state of a gamepad.
/// </summary>
/// <param name="XAxis">X-axis value (typically -100 to 100).</param>
/// <param name="YAxis">Y-axis value (typically -100 to 100).</param>
/// <param name="Buttons">Array of button states (true = pressed).</param>
public record GamepadState(int XAxis, int YAxis, bool[] Buttons);
