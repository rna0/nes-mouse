using NESMouse.Core.Abstractions;
using NESMouse.Core.Models;

namespace NESMouse.Core.Services;

/// <summary>
/// Core controller that translates gamepad input to mouse actions.
/// Extracted from the original Form1.cs logic.
/// </summary>
public class GamepadToMouseController : IDisposable
{
    private readonly IGamepadService _gamepad;
    private readonly IMouseSimulator _mouse;

    private int _velocity = 20;
    private bool _leftButtonDown;
    private bool _rightButtonDown;
    private bool _isRunning;
    private CancellationTokenSource? _cts;

    // NES controller button indices
    private const int ButtonB = 0;      // Left click
    private const int ButtonA = 1;      // Right click
    private const int ButtonSelect = 8; // Slower cursor
    private const int ButtonStart = 9;  // Faster cursor

    private const int MinVelocity = 5;
    private const int MaxVelocity = 100;
    private const int VelocityStep = 5;

    public GamepadToMouseController(IGamepadService gamepad, IMouseSimulator mouse)
    {
        _gamepad = gamepad ?? throw new ArgumentNullException(nameof(gamepad));
        _mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
    }

    /// <summary>
    /// Gets or sets the current velocity (divisor for cursor movement).
    /// Higher values = slower movement.
    /// </summary>
    public int Velocity
    {
        get => _velocity;
        set => _velocity = Math.Clamp(value, MinVelocity, MaxVelocity);
    }

    /// <summary>
    /// Gets whether the controller is currently running.
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Gets the underlying gamepad service.
    /// </summary>
    public IGamepadService GamepadService => _gamepad;

    /// <summary>
    /// Starts the gamepad-to-mouse translation loop.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1));
            while (await timer.WaitForNextTickAsync(_cts.Token))
            {
                ProcessGamepadInput();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        finally
        {
            _isRunning = false;
            ReleaseAllButtons();
        }
    }

    /// <summary>
    /// Stops the gamepad-to-mouse translation loop.
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
    }

    /// <summary>
    /// Processes a single frame of gamepad input.
    /// </summary>
    internal void ProcessGamepadInput()
    {
        var state = _gamepad.Poll();
        if (state == null)
            return;

        ProcessMovement(state);
        ProcessButtons(state);
    }

    private void ProcessMovement(GamepadState state)
    {
        int deltaX = state.XAxis / _velocity;
        int deltaY = state.YAxis / _velocity;

        if (deltaX != 0 || deltaY != 0)
        {
            _mouse.MoveCursor(deltaX, deltaY);
        }
    }

    private void ProcessButtons(GamepadState state)
    {
        // Button A (index 1) -> Right click
        if (state.Buttons.Length > ButtonA)
        {
            HandleMouseButton(state.Buttons[ButtonA], MouseButton.Right, ref _rightButtonDown);
        }

        // Button B (index 0) -> Left click
        if (state.Buttons.Length > ButtonB)
        {
            HandleMouseButton(state.Buttons[ButtonB], MouseButton.Left, ref _leftButtonDown);
        }

        // SELECT (index 8) -> Slower cursor (increase velocity divisor)
        if (state.Buttons.Length > ButtonSelect && state.Buttons[ButtonSelect])
        {
            if (_velocity < MaxVelocity)
            {
                _velocity += VelocityStep;
            }
        }

        // START (index 9) -> Faster cursor (decrease velocity divisor)
        if (state.Buttons.Length > ButtonStart && state.Buttons[ButtonStart])
        {
            if (_velocity > MinVelocity)
            {
                _velocity -= VelocityStep;
            }
        }
    }

    private void HandleMouseButton(bool isPressed, MouseButton button, ref bool wasPressed)
    {
        if (isPressed && !wasPressed)
        {
            _mouse.MouseDown(button);
            wasPressed = true;
        }
        else if (!isPressed && wasPressed)
        {
            _mouse.MouseUp(button);
            wasPressed = false;
        }
    }

    private void ReleaseAllButtons()
    {
        if (_leftButtonDown)
        {
            _mouse.MouseUp(MouseButton.Left);
            _leftButtonDown = false;
        }

        if (_rightButtonDown)
        {
            _mouse.MouseUp(MouseButton.Right);
            _rightButtonDown = false;
        }
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
        _gamepad.Dispose();
    }
}
