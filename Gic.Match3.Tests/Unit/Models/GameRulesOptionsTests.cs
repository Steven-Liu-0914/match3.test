using Gic.Match3.Domain.Models;

namespace Gic.Match3.Tests.Unit.Models
{
    /// <summary>
    /// Unit tests for GameRulesOptions to ensure configuration properties are correctly assigned and stored.
    /// </summary>
    public class GameRulesOptionsTests
    {
        [Fact]
        public void Properties_ShouldBeSettableAndRetrievable()
        {
            // Arrange
            var options = new GameRulesOptions();
            var allowedSymbols = new[] { '@', '#', '$' };

            // Act
            options.EmptySymbol = '.';
            options.AllowedSymbols = allowedSymbols;
            options.MaxBricks = 5;
            options.BrickSize = 3;
            options.FirstReadCommands = 10;

            // Assert
            options.EmptySymbol.Should().Be('.');
            options.AllowedSymbols.Should().BeEquivalentTo(allowedSymbols);
            options.MaxBricks.Should().Be(5);
            options.BrickSize.Should().Be(3);
            options.FirstReadCommands.Should().Be(10);
        }

        [Fact]
        public void AllowedSymbols_ShouldInitializeAsEmptyArrayByDefault()
        {
            // Arrange & Act
            var options = new GameRulesOptions();

            // Assert
            // Ensures that the array is initialized to avoid NullReferenceExceptions elsewhere in the app.
            options.AllowedSymbols.Should().NotBeNull();
            options.AllowedSymbols.Should().BeEmpty();
        }

        [Fact]
        public void SectionName_ShouldMatchExpectedConstant()
        {
            // Arrange & Act & Assert
            // Verifies the constant used for appsettings.json binding.
            GameRulesOptions.SectionName.Should().Be("GameRules");
        }
    }
}
