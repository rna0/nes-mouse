using NESMouse.Core.Abstractions;
using NESMouse.Core.Models;
using SDL2;

namespace NESMouse.Platform.macOS;

/// <summary>
/// macOS implementation of IGamepadService using SDL2.
/// </summary>
public class MacOSGamepadService : IGamepadService
{
    private readonly Dictionary<int, IntPtr> _joysticks = new();
    private int _selectedJoystickIndex = -1;
    private GamepadDevice? _selectedDevice;
    private bool _disposed;
    private bool _initialized;

    public MacOSGamepadService()
    {
        if (SDL.SDL_Init(SDL.SDL_INIT_JOYSTICK) < 0)
        {
            throw new InvalidOperationException($"Failed to initialize SDL: {SDL.SDL_GetError()}");
        }
        _initialized = true;
    }

    public GamepadDevice? SelectedDevice => _selectedDevice;

    public event EventHandler<DeviceChangedEventArgs>? DeviceChanged;

    public IReadOnlyList<GamepadDevice> GetConnectedDevices()
    {
        // Process SDL events to update joystick state
        SDL.SDL_JoystickUpdate();

        var devices = new List<GamepadDevice>();
        int numJoysticks = SDL.SDL_NumJoysticks();

        for (int i = 0; i < numJoysticks; i++)
        {
            var name = SDL.SDL_JoystickNameForIndex(i) ?? $"Joystick {i}";
            var id = i.ToString();

            // Open the joystick if not already open
            if (!_joysticks.ContainsKey(i))
            {
                var joystick = SDL.SDL_JoystickOpen(i);
                if (joystick != IntPtr.Zero)
                {
                    _joysticks[i] = joystick;
                }
            }

            devices.Add(new GamepadDevice(id, name));
        }

        return devices;
    }

    public void SelectDevice(GamepadDevice device)
    {
        if (int.TryParse(device.Id, out int index) && _joysticks.ContainsKey(index))
        {
            _selectedJoystickIndex = index;
            _selectedDevice = device;
        }
    }

    public GamepadState? Poll()
    {
        if (_selectedJoystickIndex < 0 || !_joysticks.TryGetValue(_selectedJoystickIndex, out var joystick))
            return null;

        SDL.SDL_JoystickUpdate();

        // Check if joystick is still attached
        if (SDL.SDL_JoystickGetAttached(joystick) == SDL.SDL_bool.SDL_FALSE)
            return null;

        // Get axis values and normalize to -100 to 100 range
        // SDL axes range from -32768 to 32767
        int xRaw = SDL.SDL_JoystickGetAxis(joystick, 0);
        int yRaw = SDL.SDL_JoystickGetAxis(joystick, 1);

        int xAxis = (int)(xRaw / 327.67);  // Scale to -100 to 100
        int yAxis = (int)(yRaw / 327.67);

        // Apply deadzone
        const int deadzone = 10;
        if (Math.Abs(xAxis) < deadzone) xAxis = 0;
        if (Math.Abs(yAxis) < deadzone) yAxis = 0;

        // Get button states
        int numButtons = SDL.SDL_JoystickNumButtons(joystick);
        var buttons = new bool[Math.Max(numButtons, 10)]; // Ensure at least 10 buttons for NES mapping

        for (int i = 0; i < numButtons; i++)
        {
            buttons[i] = SDL.SDL_JoystickGetButton(joystick, i) == 1;
        }

        // Also check D-Pad as HAT
        int numHats = SDL.SDL_JoystickNumHats(joystick);
        if (numHats > 0)
        {
            byte hat = SDL.SDL_JoystickGetHat(joystick, 0);
            // Override axis values if D-Pad is pressed
            if ((hat & SDL.SDL_HAT_LEFT) != 0) xAxis = -100;
            if ((hat & SDL.SDL_HAT_RIGHT) != 0) xAxis = 100;
            if ((hat & SDL.SDL_HAT_UP) != 0) yAxis = -100;
            if ((hat & SDL.SDL_HAT_DOWN) != 0) yAxis = 100;
        }

        return new GamepadState(xAxis, yAxis, buttons);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var joystick in _joysticks.Values)
        {
            if (joystick != IntPtr.Zero)
            {
                SDL.SDL_JoystickClose(joystick);
            }
        }

        _joysticks.Clear();

        if (_initialized)
        {
            SDL.SDL_QuitSubSystem(SDL.SDL_INIT_JOYSTICK);
        }
    }
}
