using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;


namespace Gic.Match3.Tests.Unit.Models
{
    // <summary>
    /// Unit tests for the Brick model to ensure coordinate calculations and state management are correct.
    /// </summary>
    public class BrickTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var symbols = new[] { '@', '@', '@' };
            var startPosition = new Point(0, 0);
            var orientation = Orientation.Horizontal;

            // Act
            var brick = new Brick(orientation, symbols, startPosition);

            // Assert
            brick.Orientation.Should().Be(orientation);
            brick.Symbols.Should().HaveCount(3);
            brick.Symbols.Should().ContainInOrder('@', '@', '@');
            brick.Position.Should().Be(startPosition);
        }

        [Fact]
        public void UpdatePosition_ShouldUpdateToNewCoordinates()
        {
            // Arrange
            var brick = new Brick(Orientation.Horizontal, ['#'], new Point(0, 0));

            // Act
            brick.UpdatePosition(5, 10);

            // Assert
            brick.Position.Row.Should().Be(5);
            brick.Position.Col.Should().Be(10);
        }

        [Theory]
        [InlineData(1, 1, 3)] // Row 1, Col 1, Length 3
        public void GetOccupiedPoints_WhenHorizontal_ShouldReturnIncreasingColumns(int row, int col, int length)
        {
            // Arrange
            var symbols = new char[length]; // Symbols themselves don't affect coordinates
            var brick = new Brick(Orientation.Horizontal, symbols, new Point(row, col));

            // Act
            var result = brick.GetOccupiedPoints().ToList();

            // Assert
            result.Should().HaveCount(length);
            for (int i = 0; i < length; i++)
            {
                result[i].Row.Should().Be(row);
                result[i].Col.Should().Be(col + i);
            }
        }

        [Theory]
        [InlineData(2, 2, 4)] // Row 2, Col 2, Length 4
        public void GetOccupiedPoints_WhenVertical_ShouldReturnIncreasingRows(int row, int col, int length)
        {
            // Arrange
            var symbols = new char[length];
            var brick = new Brick(Orientation.Vertical, symbols, new Point(row, col));

            // Act
            var result = brick.GetOccupiedPoints().ToList();

            // Assert
            result.Should().HaveCount(length);
            for (int i = 0; i < length; i++)
            {
                result[i].Row.Should().Be(row + i);
                result[i].Col.Should().Be(col);
            }
        }

        [Fact]
        public void GetOccupiedPoints_SingleSymbolBrick_ShouldReturnCurrentPosition()
        {
            // Arrange
            var startPos = new Point(3, 3);
            var brick = new Brick(Orientation.Horizontal, ['*'], startPos);

            // Act
            var result = brick.GetOccupiedPoints().ToList();

            // Assert
            result.Should().ContainSingle();
            result[0].Should().Be(startPos);
        }
    }
}
