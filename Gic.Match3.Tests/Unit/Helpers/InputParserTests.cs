using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Helpers;

namespace Gic.Match3.Tests.Unit.Helpers
{
    /// <summary>
    /// Unit tests for InputParser to ensure raw console strings are correctly 
    /// transformed into GameInitDto objects.
    /// </summary>
    public class InputParserTests
    {
        [Theory]
        [InlineData("5 5 H@@@ V###", 5, 5, 2)]
        [InlineData("10 20 H~~~", 10, 20, 1)]
        public void ParseInitSetup_ValidInput_ShouldReturnCorrectDto(
            string input, int expectedW, int expectedH, int expectedBrickCount)
        {
            // Act
            var result = InputParser.ParseInitSetup(input);

            // Assert
            result.Width.Should().Be(expectedW);
            result.Height.Should().Be(expectedH);
            result.Bricks.Should().HaveCount(expectedBrickCount);
        }

        [Fact]
        public void ParseInitSetup_BrickDetails_ShouldParseOrientationAndSymbols()
        {
            // Arrange
            string input = "5 5 H@#~ V!@#";

            // Act
            var result = InputParser.ParseInitSetup(input);

            // Assert
            var firstBrick = result.Bricks[0];
            firstBrick.Orientation.Should().Be(Orientation.Horizontal);
            char[] elements = ['@', '#', '~'];
            firstBrick.Symbols.Should().Equal(elements);

            var secondBrick = result.Bricks[1];
            secondBrick.Orientation.Should().Be(Orientation.Vertical);
            elements = ['!', '@', '#'];
            secondBrick.Symbols.Should().Equal(elements);
        }

        [Theory]
        [InlineData("   5    5    H@@@   ", 5, 5)] // Extra spaces
        [InlineData("5 5", 5, 5)]               // No bricks
        public void ParseInitSetup_IrregularSpacing_ShouldStillParseCorrectly(string input, int expectedW, int expectedH)
        {
            // Act
            var result = InputParser.ParseInitSetup(input);

            // Assert
            result.Width.Should().Be(expectedW);
            result.Height.Should().Be(expectedH);
        }

        [Fact]
        public void ParseInitSetup_InvalidNumbers_ShouldDefaultToZero()
        {
            // Arrange: User typed letters instead of numbers
            string input = "abc def H@@@";

            // Act
            var result = InputParser.ParseInitSetup(input);

            // Assert
            result.Width.Should().Be(0);
            result.Height.Should().Be(0);
            result.Bricks.Should().ContainSingle();
        }

        [Fact]
        public void ParseInitSetup_EmptyOrNullInput_ShouldReturnEmptyDto()
        {
            // Act
            var result = InputParser.ParseInitSetup("");

            // Assert
            result.Width.Should().Be(0);
            result.Height.Should().Be(0);
            result.Bricks.Should().BeEmpty();
        }

        [Fact]
        public void ParseInitSetup_BrickWithNoSymbols_ShouldReturnEmptySymbolsArray()
        {
            // Arrange: Only orientation 'H' is provided without symbols
            string input = "5 5 H";

            // Act
            var result = InputParser.ParseInitSetup(input);

            // Assert
            result.Bricks[0].Symbols.Should().BeEmpty();
        }
    }
}
