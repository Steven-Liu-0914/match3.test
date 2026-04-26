using Gic.Match3.Domain.Models;

namespace Gic.Match3.Tests.Unit.Models
{
    /// <summary>
    /// Unit tests for FrameCommandDto to verify record-based value equality and initialization.
    /// </summary>
    public class FrameCommandDtoTests
    {
        [Fact]
        public void Constructor_ShouldSetRawInputCorrectly()
        {
            // Arrange
            var input = "LLD";

            // Act
            var dto = new FrameCommandDto(input);

            // Assert
            dto.RawInput.Should().Be(input);
        }

        [Fact]
        public void ValueEquality_TwoRecordsWithSameData_ShouldBeEqual()
        {
            // Arrange
            var input = "RR";
            var dto1 = new FrameCommandDto(input);
            var dto2 = new FrameCommandDto(input);

            // Act & Assert
            // In C#, records compare values, not memory addresses.
            dto1.Should().Be(dto2);
            (dto1 == dto2).Should().BeTrue();
        }

        [Fact]
        public void ValueEquality_TwoRecordsWithDifferentData_ShouldNotBeEqual()
        {
            // Arrange
            var dto1 = new FrameCommandDto("L");
            var dto2 = new FrameCommandDto("R");

            // Act & Assert
            dto1.Should().NotBe(dto2);
            (dto1 == dto2).Should().BeFalse();
        }   
    }
}
