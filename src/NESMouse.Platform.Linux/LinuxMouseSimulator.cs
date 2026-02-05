using System.Runtime.InteropServices;
using NESMouse.Core.Abstractions;
using NESMouse.Core.Models;

namespace NESMouse.Platform.Linux;

/// <summary>
/// Linux implementation of IMouseSimulator using uinput.
/// This works on both X11 and Wayland.
/// Note: User must be in the 'input' group or have appropriate udev rules.
/// </summary>
public class LinuxMouseSimulator : IMouseSimulator
{
    private int _uinputFd = -1;
    private bool _disposed;

    // uinput constants
    private const string UinputPath = "/dev/uinput";
    private const int UI_SET_EVBIT = 0x40045564;
    private const int UI_SET_KEYBIT = 0x40045565;
    private const int UI_SET_RELBIT = 0x40045566;
    private const int UI_DEV_CREATE = 0x5501;
    private const int UI_DEV_DESTROY = 0x5502;

    // Event types
    private const ushort EV_SYN = 0x00;
    private const ushort EV_KEY = 0x01;
    private const ushort EV_REL = 0x02;

    // Relative axes
    private const ushort REL_X = 0x00;
    private const ushort REL_Y = 0x01;

    // Mouse button codes
    private const ushort BTN_LEFT = 0x110;
    private const ushort BTN_RIGHT = 0x111;
    private const ushort BTN_MIDDLE = 0x112;

    // Synchronization
    private const ushort SYN_REPORT = 0x00;

    // File open flags
    private const int O_WRONLY = 0x0001;
    private const int O_NONBLOCK = 0x0800;

    [StructLayout(LayoutKind.Sequential)]
    private struct InputEvent
    {
        public long Sec;
        public long Usec;
        public ushort Type;
        public ushort Code;
        public int Value;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct UinputSetup
    {
        public InputId Id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string Name;
        public uint FfEffectsMax;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct InputId
    {
        public ushort Bustype;
        public ushort Vendor;
        public ushort Product;
        public ushort Version;
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int open([MarshalAs(UnmanagedType.LPStr)] string pathname, int flags);

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd);

    [DllImport("libc", SetLastError = true)]
    private static extern int write(int fd, ref InputEvent buf, int count);

    [DllImport("libc", SetLastError = true)]
    private static extern int write(int fd, ref UinputSetup buf, int count);

    [DllImport("libc", SetLastError = true)]
    private static extern int ioctl(int fd, uint request, int value);

    // For getting cursor position, we need X11 (fallback for older X11 systems)
    // or we can track position ourselves
    private int _cursorX;
    private int _cursorY;

    public LinuxMouseSimulator()
    {
        InitializeUinput();
        // Initialize cursor position to screen center (approximate)
        _cursorX = 960;
        _cursorY = 540;
    }

    private void InitializeUinput()
    {
        _uinputFd = open(UinputPath, O_WRONLY | O_NONBLOCK);
        if (_uinputFd < 0)
        {
            throw new InvalidOperationException(
                $"Failed to open {UinputPath}. Make sure you have permission (add user to 'input' group or create udev rules).");
        }

        try
        {
            // Enable event types
            if (ioctl(_uinputFd, UI_SET_EVBIT, EV_KEY) < 0)
                throw new InvalidOperationException("Failed to set EV_KEY");
            if (ioctl(_uinputFd, UI_SET_EVBIT, EV_REL) < 0)
                throw new InvalidOperationException("Failed to set EV_REL");
            if (ioctl(_uinputFd, UI_SET_EVBIT, EV_SYN) < 0)
                throw new InvalidOperationException("Failed to set EV_SYN");

            // Enable mouse buttons
            if (ioctl(_uinputFd, UI_SET_KEYBIT, BTN_LEFT) < 0)
                throw new InvalidOperationException("Failed to set BTN_LEFT");
            if (ioctl(_uinputFd, UI_SET_KEYBIT, BTN_RIGHT) < 0)
                throw new InvalidOperationException("Failed to set BTN_RIGHT");
            if (ioctl(_uinputFd, UI_SET_KEYBIT, BTN_MIDDLE) < 0)
                throw new InvalidOperationException("Failed to set BTN_MIDDLE");

            // Enable relative axes
            if (ioctl(_uinputFd, UI_SET_RELBIT, REL_X) < 0)
                throw new InvalidOperationException("Failed to set REL_X");
            if (ioctl(_uinputFd, UI_SET_RELBIT, REL_Y) < 0)
                throw new InvalidOperationException("Failed to set REL_Y");

            // Set up device info
            var setup = new UinputSetup
            {
                Id = new InputId
                {
                    Bustype = 0x03, // BUS_USB
                    Vendor = 0x1234,
                    Product = 0x5678,
                    Version = 1
                },
                Name = "NES Mouse Virtual Device"
            };

            if (write(_uinputFd, ref setup, Marshal.SizeOf<UinputSetup>()) < 0)
                throw new InvalidOperationException("Failed to write uinput setup");

            // Create the device
            if (ioctl(_uinputFd, UI_DEV_CREATE, 0) < 0)
                throw new InvalidOperationException("Failed to create uinput device");
        }
        catch
        {
            close(_uinputFd);
            _uinputFd = -1;
            throw;
        }
    }

    public (int X, int Y) GetCursorPosition()
    {
        // Note: Getting the actual cursor position on Linux is complex and
        // varies between X11 and Wayland. For now, we track it ourselves.
        // This means the position may drift from actual if user moves mouse directly.
        return (_cursorX, _cursorY);
    }

    public void MoveCursor(int deltaX, int deltaY)
    {
        if (deltaX == 0 && deltaY == 0)
            return;

        if (_uinputFd < 0)
            return;

        // Update tracked position
        _cursorX += deltaX;
        _cursorY += deltaY;

        // Send relative movement events
        SendEvent(EV_REL, REL_X, deltaX);
        SendEvent(EV_REL, REL_Y, deltaY);
        SendEvent(EV_SYN, SYN_REPORT, 0);
    }

    public void MouseDown(MouseButton button)
    {
        if (_uinputFd < 0)
            return;

        ushort code = GetButtonCode(button);
        SendEvent(EV_KEY, code, 1);
        SendEvent(EV_SYN, SYN_REPORT, 0);
    }

    public void MouseUp(MouseButton button)
    {
        if (_uinputFd < 0)
            return;

        ushort code = GetButtonCode(button);
        SendEvent(EV_KEY, code, 0);
        SendEvent(EV_SYN, SYN_REPORT, 0);
    }

    private static ushort GetButtonCode(MouseButton button) => button switch
    {
        MouseButton.Left => BTN_LEFT,
        MouseButton.Right => BTN_RIGHT,
        MouseButton.Middle => BTN_MIDDLE,
        _ => throw new ArgumentOutOfRangeException(nameof(button))
    };

    private void SendEvent(ushort type, ushort code, int value)
    {
        var ev = new InputEvent
        {
            Type = type,
            Code = code,
            Value = value
        };

        write(_uinputFd, ref ev, Marshal.SizeOf<InputEvent>());
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_uinputFd >= 0)
        {
            ioctl(_uinputFd, UI_DEV_DESTROY, 0);
            close(_uinputFd);
            _uinputFd = -1;
        }
    }

    ~LinuxMouseSimulator()
    {
        Dispose();
    }
}
