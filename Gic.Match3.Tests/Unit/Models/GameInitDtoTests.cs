using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;

namespace Gic.Match3.Tests.Unit.Models
{
    /// <summary>
    /// Unit tests for GameInitDto to verify initialization and record equality behavior, 
    /// especially handling reference types like List.
    /// </summary>
    public class GameInitDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var bricks = new List<Brick>
        {
            new(Orientation.Horizontal, ['@', '@'], new Point(0, 0))
        };
            int width = 10;
            int height = 20;

            // Act
            var dto = new GameInitDto(width, height, bricks);

            // Assert
            dto.Width.Should().Be(width);
            dto.Height.Should().Be(height);
            dto.Bricks.Should().BeSameAs(bricks); // Verifies the reference is stored correctly
        }

        [Fact]
        public void Equality_SameValuesAndSameListReference_ShouldBeEqual()
        {
            // Arrange
            var bricks = new List<Brick>();
            var dto1 = new GameInitDto(5, 5, bricks);
            var dto2 = new GameInitDto(5, 5, bricks);

            // Act & Assert
            // Since the List reference is the same, the record equality will pass.
            dto1.Should().Be(dto2);
            (dto1 == dto2).Should().BeTrue();
        }

        [Fact]
        public void Equality_SameValuesButDifferentListInstances_ShouldNotBeEqual()
        {
            // Arrange
            // Important: Records do NOT perform deep equality on collections by default.
            var dto1 = new GameInitDto(5, 5, []);
            var dto2 = new GameInitDto(5, 5, []);

            // Act & Assert
            // This confirms your understanding of how C# records handle reference types.
            dto1.Should().NotBe(dto2);
            (dto1 == dto2).Should().BeFalse();
        }

        [Fact]
        public void Deconstruction_ShouldRetrieveAllValues()
        {
            // Arrange
            var bricks = new List<Brick>();
            var dto = new GameInitDto(8, 8, bricks);

            // Act
            var (w, h, b) = dto;

            // Assert
            w.Should().Be(8);
            h.Should().Be(8);
            b.Should().BeSameAs(bricks);
        }
    }
}
