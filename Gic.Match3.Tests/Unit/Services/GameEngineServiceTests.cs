using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Services;

namespace Gic.Match3.Tests.Unit.Services
{
    /// <summary>
    /// Comprehensive tests for GameEngineService.
    /// Covers physics, movement constraints, gravity, and collision logic.
    /// </summary>
    public class GameEngineServiceTests
    {
        private readonly GameRulesOptions _options;
        private readonly GameEngineService _service;

        public GameEngineServiceTests()
        {
            // Standard setup for testing physics
            _options = new GameRulesOptions { BrickSize = 3, EmptySymbol = '.' };
            _service = new GameEngineService(_options);
        }

        #region Initial Position Tests

        [Theory]
        [InlineData(10, 3)] // Width 10, spans 3: (10-3)/2 = 3
        [InlineData(5, 1)]  // Width 5, spans 3: (5-3)/2 = 1
        public void SetInitialPosition_HorizontalBrick_ShouldCenterMathematically(int width, int expectedCol)
        {
            // Arrange
            var brick = new Brick(Orientation.Horizontal, ['@', '@', '@'], new Point(0, 0));

            // Act
            _service.SetInitialPosition(brick, width);

            // Assert
            brick.Position.Col.Should().Be(expectedCol);
            brick.Position.Row.Should().Be(0);
        }

        #endregion

        #region Movement and Collision Tests

        [Fact]
        public void TryMove_ValidLeftRight_ShouldUpdateCoordinates()
        {
            // Arrange: 5x5 field, vertical brick at center
            var field = new GameField(5, 5, '.');
            var brick = new Brick(Orientation.Vertical, ['@', '@', '@'], new Point(0, 2));

            // Act & Assert: Move Left
            _service.TryMove(brick, field, MoveCommand.Left, isLastCommand: false);
            brick.Position.Col.Should().Be(1);

            // Act & Assert: Move Right
            _service.TryMove(brick, field, MoveCommand.Right, isLastCommand: false);
            brick.Position.Col.Should().Be(2);
        }

        [Fact]
        public void TryMove_HitBoundary_ShouldResetCommandToContinueAndStayInPlace()
        {
            // Arrange: Brick at far left wall
            var field = new GameField(5, 5, '.');
            var brick = new Brick(Orientation.Vertical, ['@', '@', '@'], new Point(0, 0));

            // Act: Attempt to move left into the wall
            _service.TryMove(brick, field, MoveCommand.Left, isLastCommand: false);

            // Assert: Command is internally ignored, position remains 0
            brick.Position.Col.Should().Be(0);
        }

        [Fact]
        public void TryMove_HitExistingBlock_ShouldBeBlockedHorizontally()
        {
            // Arrange: Place a fixed block to the right of the brick
            var field = new GameField(5, 5, '.');
            field.SetCell(0, 3, '#');
            var brick = new Brick(Orientation.Vertical, ['@', '@', '@'], new Point(0, 2));

            // Act: Try to move right into the '#' block
            _service.TryMove(brick, field, MoveCommand.Right, isLastCommand: false);

            // Assert: Movement blocked
            brick.Position.Col.Should().Be(2);
        }

        [Fact]
        public void TryMove_DownCommand_ShouldDropUntilItHitsFloor()
        {
            // Arrange: 10 rows deep empty field
            var field = new GameField(5, 10, '.');
            var brick = new Brick(Orientation.Horizontal, ['@', '@', '@'], new Point(0, 0));

            // Act: User sends 'D' command
            _service.TryMove(brick, field, MoveCommand.Down, isLastCommand: false);

            // Assert: Horizontal brick is 1 row high, stops at last valid row index (9)
            brick.Position.Row.Should().Be(9);
        }

        #endregion

        #region Auto Drop Tests

        [Fact]
        public void TryMove_IsLastCommand_ShouldApplyAutomaticOneRowDrop()
        {
            // Arrange
            var field = new GameField(5, 5, '.');
            var brick = new Brick(Orientation.Horizontal, ['@', '@', '@'], new Point(0, 1));

            // Act: Move Right, and it's the last command of the frame
            _service.TryMove(brick, field, MoveCommand.Right, isLastCommand: true);

            // Assert: Col 1 -> 2 (Move), Row 0 -> 1 (Auto-Drop)
            brick.Position.Col.Should().Be(2);
            brick.Position.Row.Should().Be(1);
        }

        [Fact]
        public void TryMove_IsLastCommand_ButBlockedBelow_ShouldNotDrop()
        {
            // Arrange: Brick is already sitting on the floor
            var field = new GameField(5, 5, '.');
            var brick = new Brick(Orientation.Horizontal, ['@', '@', '@'], new Point(4, 0));

            // Act: Last command is processed
            _service.TryMove(brick, field, MoveCommand.Continue, isLastCommand: true);

            // Assert: Stays at Row 4 because Row 5 is the floor
            brick.Position.Row.Should().Be(4);
        }

        #endregion

        #region State Detection Tests

        [Fact]
        public void IsSettled_WhenTouchingAnotherBlock_ShouldReturnTrue()
        {
            // Arrange: Vertical brick at Row 0, occupying 0,1,2. Fixed block at Row 3.
            var field = new GameField(5, 5, '.');
            field.SetCell(3, 2, '#');
            var brick = new Brick(Orientation.Vertical, ['@', '@', '@'], new Point(0, 2));

            // Act
            var result = _service.IsSettled(brick, field);

            // Assert: Brick should be settled because it's resting on Row 3
            result.Should().BeTrue();
        }

        [Fact]
        public void IsInitialPositionBlocked_WhenSpawnPointIsOccupied_ShouldReturnTrue()
        {
            // Arrange: Fixed block right at the top-center spawn point
            var field = new GameField(5, 5, '.');
            field.SetCell(0, 2, '#');
            var brick = new Brick(Orientation.Vertical, ['@', '@', '@'], new Point(0, 2));

            // Act
            var result = _service.IsInitialPositionBlocked(brick, field);

            // Assert
            result.Should().BeTrue();
        }

        #endregion
    }
}
