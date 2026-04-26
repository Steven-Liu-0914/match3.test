using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;

namespace Gic.Match3.Tests.Unit.Models
{
    /// <summary>
    /// Unit tests for GameField to verify grid management, boundary detection, 
    /// and the physics-based drop distance calculations.
    /// </summary>
    public class GameFieldTests
    {
        private const char Empty = '.';
        private const int Width = 5;
        private const int Height = 5;

        [Fact]
        public void Constructor_ShouldInitializeWithEmptySymbols()
        {
            // Arrange & Act
            var field = new GameField(Width, Height, Empty);

            // Assert
            field.Width.Should().Be(Width);
            field.Height.Should().Be(Height);

            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    field.GetCell(r, c).Should().Be(Empty);
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(4, 4, true)]
        [InlineData(-1, 0, false)]
        [InlineData(0, 5, false)]
        [InlineData(5, 0, false)]
        public void IsWithinBounds_ShouldValidateCoordinatesCorrectly(int r, int c, bool expected)
        {
            // Arrange
            var field = new GameField(Width, Height, Empty);

            // Act
            var result = field.IsWithinBounds(r, c);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void GetMaxDropDistance_InEmptyColumn_ShouldReturnDistanceToBottom()
        {
            // Arrange
            var field = new GameField(Width, Height, Empty);
            int startRow = 1;
            int col = 2;

            // Act: From Row 1 in a 5-row field (0-4), it should drop 3 rows to reach Row 4
            int distance = field.GetMaxDropDistance(startRow, col);

            // Assert
            distance.Should().Be(3);
        }

        [Fact]
        public void GetMaxDropDistance_WithObstacle_ShouldReturnDistanceUntilObstacle()
        {
            // Arrange
            var field = new GameField(Width, Height, Empty);
            field.SetCell(4, 2, 'X'); // Obstacle at the bottom
            field.SetCell(3, 2, 'O'); // Another obstacle at Row 3

            // Act: From Row 0, it should stop at Row 2 (distance of 2) because Row 3 is blocked
            int distance = field.GetMaxDropDistance(0, 2);

            // Assert
            distance.Should().Be(2);
        }

        [Fact]
        public void GetMaxDropDistance_WhenAlreadyAtBottom_ShouldReturnZero()
        {
            // Arrange
            var field = new GameField(Width, Height, Empty);

            // Act
            int distance = field.GetMaxDropDistance(Height - 1, 0);

            // Assert
            distance.Should().Be(0);
        }

        [Fact]
        public void FixBrick_ShouldTransferSymbolsToGrid()
        {
            // Arrange
            var field = new GameField(Width, Height, Empty);
            var symbols = new[] { 'A', 'B', 'C' };
            var brick = new Brick(Orientation.Horizontal, symbols, new Point(0, 0));

            // Act
            field.FixBrick(brick);

            // Assert
            field.GetCell(0, 0).Should().Be('A');
            field.GetCell(0, 1).Should().Be('B');
            field.GetCell(0, 2).Should().Be('C');
            field.GetCell(0, 3).Should().Be(Empty);
        }

        [Fact]
        public void FixBrick_WhenPartiallyOutOfBounds_ShouldOnlyFixValidPoints()
        {
            // Arrange
            var field = new GameField(Width, Height, Empty);
            var symbols = new[] { 'X', 'Y', 'Z' };
            // Place a horizontal brick at the very edge so 'Z' falls out of bounds (Col 5)
            var brick = new Brick(Orientation.Horizontal, symbols, new Point(0, 3));

            // Act
            field.FixBrick(brick);

            // Assert
            field.GetCell(0, 3).Should().Be('X');
            field.GetCell(0, 4).Should().Be('Y');
            // No exception should be thrown, and grid remains unchanged for the invalid point
        }
    }
}
