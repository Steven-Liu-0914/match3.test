using Gic.Match3.Domain.Models;

namespace Gic.Match3.Tests.Unit.Models
{
    /// <summary>
    /// Unit tests for the Point model to ensure basic coordinate manipulation works as expected.
    /// </summary>
    public class PointTests
    {
        [Fact]
        public void Constructor_ShouldSetInitialCoordinates()
        {
            // Arrange & Act
            var point = new Point(10, 20);

            // Assert
            point.Row.Should().Be(10);
            point.Col.Should().Be(20);
        }

        [Theory]
        [InlineData(5, 5, 1, 2, 6, 7)]    // Move positive
        [InlineData(10, 10, -3, -4, 7, 6)] // Move negative
        [InlineData(0, 0, 0, 0, 0, 0)]    // No move
        public void MoveBy_ShouldUpdateCoordinatesCorrectly(int startR, int startC, int moveR, int moveC, int expectedR, int expectedC)
        {
            // Arrange
            var point = new Point(startR, startC);

            // Act
            point.MoveBy(moveR, moveC);

            // Assert
            point.Row.Should().Be(expectedR);
            point.Col.Should().Be(expectedC);
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var point = new Point(0, 0)
            {
                // Act
                Row = 100,
                Col = 200
            };

            // Assert
            point.Row.Should().Be(100);
            point.Col.Should().Be(200);
        }
    }
}
