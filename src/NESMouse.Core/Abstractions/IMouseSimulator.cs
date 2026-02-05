using NESMouse.Core.Models;

namespace NESMouse.Core.Abstractions;

/// <summary>
/// Abstraction for mouse simulation across platforms.
/// </summary>
public interface IMouseSimulator
{
    /// <summary>
    /// Gets the current cursor position.
    /// </summary>
    /// <returns>A tuple containing the X and Y coordinates.</returns>
    (int X, int Y) GetCursorPosition();

    /// <summary>
    /// Moves the cursor by the specified delta values.
    /// </summary>
    /// <param name="deltaX">Horizontal movement (positive = right).</param>
    /// <param name="deltaY">Vertical movement (positive = down).</param>
    void MoveCursor(int deltaX, int deltaY);

    /// <summary>
    /// Simulates pressing a mouse button down.
    /// </summary>
    /// <param name="button">The button to press.</param>
    void MouseDown(MouseButton button);

    /// <summary>
    /// Simulates releasing a mouse button.
    /// </summary>
    /// <param name="button">The button to release.</param>
    void MouseUp(MouseButton button);
}
