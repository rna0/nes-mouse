using System.Runtime.InteropServices;
using NESMouse.Core.Abstractions;
using NESMouse.Core.Models;

namespace NESMouse.Platform.macOS;

/// <summary>
/// macOS implementation of IMouseSimulator using Core Graphics CGEventPost.
/// Note: Requires Accessibility permission to be granted in System Preferences.
/// </summary>
public class MacOSMouseSimulator : IMouseSimulator
{
    // Core Graphics event types
    private const int kCGEventMouseMoved = 5;
    private const int kCGEventLeftMouseDown = 1;
    private const int kCGEventLeftMouseUp = 2;
    private const int kCGEventRightMouseDown = 3;
    private const int kCGEventRightMouseUp = 4;
    private const int kCGEventOtherMouseDown = 25;
    private const int kCGEventOtherMouseUp = 26;

    // Mouse button constants
    private const int kCGMouseButtonLeft = 0;
    private const int kCGMouseButtonRight = 1;
    private const int kCGMouseButtonCenter = 2;

    // HID event source state
    private const int kCGHIDEventTap = 0;

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGEventCreateMouseEvent(
        IntPtr source,
        int mouseType,
        CGPoint mouseCursorPosition,
        int mouseButton);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern void CGEventPost(int tap, IntPtr eventRef);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern void CFRelease(IntPtr cf);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern CGPoint CGEventGetLocation(IntPtr eventRef);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGEventCreate(IntPtr source);

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;

        public CGPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public (int X, int Y) GetCursorPosition()
    {
        var eventRef = CGEventCreate(IntPtr.Zero);
        if (eventRef == IntPtr.Zero)
            return (0, 0);

        try
        {
            var point = CGEventGetLocation(eventRef);
            return ((int)point.X, (int)point.Y);
        }
        finally
        {
            CFRelease(eventRef);
        }
    }

    public void MoveCursor(int deltaX, int deltaY)
    {
        if (deltaX == 0 && deltaY == 0)
            return;

        var (currentX, currentY) = GetCursorPosition();
        var newPosition = new CGPoint(currentX + deltaX, currentY + deltaY);

        var eventRef = CGEventCreateMouseEvent(
            IntPtr.Zero,
            kCGEventMouseMoved,
            newPosition,
            kCGMouseButtonLeft);

        if (eventRef != IntPtr.Zero)
        {
            CGEventPost(kCGHIDEventTap, eventRef);
            CFRelease(eventRef);
        }
    }

    public void MouseDown(MouseButton button)
    {
        var (x, y) = GetCursorPosition();
        var position = new CGPoint(x, y);
        var (eventType, mouseButton) = GetMouseDownParams(button);

        var eventRef = CGEventCreateMouseEvent(
            IntPtr.Zero,
            eventType,
            position,
            mouseButton);

        if (eventRef != IntPtr.Zero)
        {
            CGEventPost(kCGHIDEventTap, eventRef);
            CFRelease(eventRef);
        }
    }

    public void MouseUp(MouseButton button)
    {
        var (x, y) = GetCursorPosition();
        var position = new CGPoint(x, y);
        var (eventType, mouseButton) = GetMouseUpParams(button);

        var eventRef = CGEventCreateMouseEvent(
            IntPtr.Zero,
            eventType,
            position,
            mouseButton);

        if (eventRef != IntPtr.Zero)
        {
            CGEventPost(kCGHIDEventTap, eventRef);
            CFRelease(eventRef);
        }
    }

    private static (int eventType, int mouseButton) GetMouseDownParams(MouseButton button) => button switch
    {
        MouseButton.Left => (kCGEventLeftMouseDown, kCGMouseButtonLeft),
        MouseButton.Right => (kCGEventRightMouseDown, kCGMouseButtonRight),
        MouseButton.Middle => (kCGEventOtherMouseDown, kCGMouseButtonCenter),
        _ => throw new ArgumentOutOfRangeException(nameof(button))
    };

    private static (int eventType, int mouseButton) GetMouseUpParams(MouseButton button) => button switch
    {
        MouseButton.Left => (kCGEventLeftMouseUp, kCGMouseButtonLeft),
        MouseButton.Right => (kCGEventRightMouseUp, kCGMouseButtonRight),
        MouseButton.Middle => (kCGEventOtherMouseUp, kCGMouseButtonCenter),
        _ => throw new ArgumentOutOfRangeException(nameof(button))
    };
}
