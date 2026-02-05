using System.Runtime.InteropServices;
using NESMouse.Core.Abstractions;
using NESMouse.Core.Models;

namespace NESMouse.Platform.Windows;

/// <summary>
/// Windows implementation of IMouseSimulator using SendInput API.
/// </summary>
public class WindowsMouseSimulator : IMouseSimulator
{
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint Type;
        public MOUSEINPUT Mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    private const uint INPUT_MOUSE = 0;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const uint MOUSEEVENTF_MOVE = 0x0001;

    public (int X, int Y) GetCursorPosition()
    {
        GetCursorPos(out var point);
        return (point.X, point.Y);
    }

    public void MoveCursor(int deltaX, int deltaY)
    {
        if (deltaX == 0 && deltaY == 0)
            return;

        var (currentX, currentY) = GetCursorPosition();
        SetCursorPos(currentX + deltaX, currentY + deltaY);
    }

    public void MouseDown(MouseButton button)
    {
        SendMouseEvent(GetDownFlag(button));
    }

    public void MouseUp(MouseButton button)
    {
        SendMouseEvent(GetUpFlag(button));
    }

    private static uint GetDownFlag(MouseButton button) => button switch
    {
        MouseButton.Left => MOUSEEVENTF_LEFTDOWN,
        MouseButton.Right => MOUSEEVENTF_RIGHTDOWN,
        MouseButton.Middle => MOUSEEVENTF_MIDDLEDOWN,
        _ => throw new ArgumentOutOfRangeException(nameof(button))
    };

    private static uint GetUpFlag(MouseButton button) => button switch
    {
        MouseButton.Left => MOUSEEVENTF_LEFTUP,
        MouseButton.Right => MOUSEEVENTF_RIGHTUP,
        MouseButton.Middle => MOUSEEVENTF_MIDDLEUP,
        _ => throw new ArgumentOutOfRangeException(nameof(button))
    };

    private static void SendMouseEvent(uint flags)
    {
        var inputs = new INPUT[1];
        inputs[0].Type = INPUT_MOUSE;
        inputs[0].Mi.DwFlags = flags;

        SendInput(1, inputs, Marshal.SizeOf<INPUT>());
    }
}
