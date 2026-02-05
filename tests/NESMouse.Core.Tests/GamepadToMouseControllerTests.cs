using Moq;
using NESMouse.Core.Abstractions;
using NESMouse.Core.Models;
using NESMouse.Core.Services;
using Xunit;

namespace NESMouse.Core.Tests;

public class GamepadToMouseControllerTests
{
    private readonly Mock<IGamepadService> _mockGamepad;
    private readonly Mock<IMouseSimulator> _mockMouse;
    private readonly GamepadToMouseController _controller;

    public GamepadToMouseControllerTests()
    {
        _mockGamepad = new Mock<IGamepadService>();
        _mockMouse = new Mock<IMouseSimulator>();
        _controller = new GamepadToMouseController(_mockGamepad.Object, _mockMouse.Object);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenGamepadIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GamepadToMouseController(null!, _mockMouse.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenMouseIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GamepadToMouseController(_mockGamepad.Object, null!));
    }

    [Fact]
    public void Velocity_DefaultsTo20()
    {
        Assert.Equal(20, _controller.Velocity);
    }

    [Fact]
    public void Velocity_CanBeSetWithinRange()
    {
        _controller.Velocity = 50;
        Assert.Equal(50, _controller.Velocity);
    }

    [Fact]
    public void Velocity_ClampsToMinimum()
    {
        _controller.Velocity = 1;
        Assert.Equal(5, _controller.Velocity);
    }

    [Fact]
    public void Velocity_ClampsToMaximum()
    {
        _controller.Velocity = 150;
        Assert.Equal(100, _controller.Velocity);
    }

    [Fact]
    public void ProcessGamepadInput_DoesNothing_WhenPollReturnsNull()
    {
        _mockGamepad.Setup(g => g.Poll()).Returns((GamepadState?)null);

        _controller.ProcessGamepadInput();

        _mockMouse.Verify(m => m.MoveCursor(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _mockMouse.Verify(m => m.MouseDown(It.IsAny<MouseButton>()), Times.Never);
        _mockMouse.Verify(m => m.MouseUp(It.IsAny<MouseButton>()), Times.Never);
    }

    [Fact]
    public void ProcessGamepadInput_MovesCursor_WhenAxisValuesNonZero()
    {
        var state = new GamepadState(40, 60, new bool[10]);
        _mockGamepad.Setup(g => g.Poll()).Returns(state);

        _controller.ProcessGamepadInput();

        // With default velocity of 20: deltaX = 40/20 = 2, deltaY = 60/20 = 3
        _mockMouse.Verify(m => m.MoveCursor(2, 3), Times.Once);
    }

    [Fact]
    public void ProcessGamepadInput_DoesNotMoveCursor_WhenAxisValuesZero()
    {
        var state = new GamepadState(0, 0, new bool[10]);
        _mockGamepad.Setup(g => g.Poll()).Returns(state);

        _controller.ProcessGamepadInput();

        _mockMouse.Verify(m => m.MoveCursor(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void ProcessGamepadInput_TriggersLeftClick_WhenButtonBPressed()
    {
        var buttons = new bool[10];
        buttons[0] = true; // Button B
        var state = new GamepadState(0, 0, buttons);
        _mockGamepad.Setup(g => g.Poll()).Returns(state);

        _controller.ProcessGamepadInput();

        _mockMouse.Verify(m => m.MouseDown(MouseButton.Left), Times.Once);
    }

    [Fact]
    public void ProcessGamepadInput_ReleasesLeftClick_WhenButtonBReleased()
    {
        // First press the button
        var pressedButtons = new bool[10];
        pressedButtons[0] = true;
        var pressedState = new GamepadState(0, 0, pressedButtons);
        _mockGamepad.Setup(g => g.Poll()).Returns(pressedState);
        _controller.ProcessGamepadInput();

        // Then release
        var releasedButtons = new bool[10];
        var releasedState = new GamepadState(0, 0, releasedButtons);
        _mockGamepad.Setup(g => g.Poll()).Returns(releasedState);
        _controller.ProcessGamepadInput();

        _mockMouse.Verify(m => m.MouseUp(MouseButton.Left), Times.Once);
    }

    [Fact]
    public void ProcessGamepadInput_TriggersRightClick_WhenButtonAPressed()
    {
        var buttons = new bool[10];
        buttons[1] = true; // Button A
        var state = new GamepadState(0, 0, buttons);
        _mockGamepad.Setup(g => g.Poll()).Returns(state);

        _controller.ProcessGamepadInput();

        _mockMouse.Verify(m => m.MouseDown(MouseButton.Right), Times.Once);
    }

    [Fact]
    public void ProcessGamepadInput_IncreasesVelocity_WhenSelectPressed()
    {
        var initialVelocity = _controller.Velocity;
        var buttons = new bool[10];
        buttons[8] = true; // SELECT
        var state = new GamepadState(0, 0, buttons);
        _mockGamepad.Setup(g => g.Poll()).Returns(state);

        _controller.ProcessGamepadInput();

        Assert.Equal(initialVelocity + 5, _controller.Velocity);
    }

    [Fact]
    public void ProcessGamepadInput_DecreasesVelocity_WhenStartPressed()
    {
        var initialVelocity = _controller.Velocity;
        var buttons = new bool[10];
        buttons[9] = true; // START
        var state = new GamepadState(0, 0, buttons);
        _mockGamepad.Setup(g => g.Poll()).Returns(state);

        _controller.ProcessGamepadInput();

        Assert.Equal(initialVelocity - 5, _controller.Velocity);
    }

    [Fact]
    public void ProcessGamepadInput_DoesNotIncreaseVelocityAboveMax()
    {
        _controller.Velocity = 100;
        var buttons = new bool[10];
        buttons[8] = true; // SELECT
        var state = new GamepadState(0, 0, buttons);
        _mockGamepad.Setup(g => g.Poll()).Returns(state);

        _controller.ProcessGamepadInput();

        Assert.Equal(100, _controller.Velocity);
    }

    [Fact]
    public void ProcessGamepadInput_DoesNotDecreaseVelocityBelowMin()
    {
        _controller.Velocity = 5;
        var buttons = new bool[10];
        buttons[9] = true; // START
        var state = new GamepadState(0, 0, buttons);
        _mockGamepad.Setup(g => g.Poll()).Returns(state);

        _controller.ProcessGamepadInput();

        Assert.Equal(5, _controller.Velocity);
    }

    [Fact]
    public void Dispose_DisposesGamepadService()
    {
        _controller.Dispose();

        _mockGamepad.Verify(g => g.Dispose(), Times.Once);
    }

    [Fact]
    public void IsRunning_ReturnsFalse_Initially()
    {
        Assert.False(_controller.IsRunning);
    }

    [Fact]
    public async Task StartAsync_SetsIsRunningTrue()
    {
        var cts = new CancellationTokenSource();
        var task = _controller.StartAsync(cts.Token);

        // Give it a moment to start
        await Task.Delay(10);

        Assert.True(_controller.IsRunning);

        cts.Cancel();
        await task;
    }

    [Fact]
    public async Task Stop_StopsTheController()
    {
        var cts = new CancellationTokenSource();
        var task = _controller.StartAsync(cts.Token);

        await Task.Delay(10);
        _controller.Stop();

        await task;

        Assert.False(_controller.IsRunning);
    }
}
