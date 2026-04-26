using FluentValidation.TestHelper;
using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Validators;

namespace Gic.Match3.Tests.Unit.Validators
{
    /// <summary>
    /// Comprehensive tests for the game setup validator. 
    /// Covers dimension logic, brick count limits, and orientation constraints.
    /// </summary>
    public class GameInitializationValidatorTests
    {
        private readonly GameRulesOptions _options;
        private readonly GameInitializationValidator _validator;

        public GameInitializationValidatorTests()
        {
            // Standard rules for testing: Max 3 bricks, bricks are size 3.
            _options = new GameRulesOptions
            {
                MaxBricks = 3,
                BrickSize = 3,
                AllowedSymbols = ['@', '#', '~'],
                EmptySymbol = '.'
            };
            _validator = new GameInitializationValidator(_options);
        }

        [Fact]
        public void Validate_ValidInitialization_ShouldNotHaveErrors()
        {
            // Arrange
            var bricks = new List<Brick>
        {
            new(Orientation.Horizontal, ['@', '@', '@'], new Point(0, 0))
        };
            var dto = new GameInitDto(5, 5, bricks);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0, 5)]  // Width is zero
        [InlineData(5, 0)]  // Height is zero
        [InlineData(-1, 5)] // Width is negative
        public void Validate_InvalidFieldSize_ShouldHaveError(int width, int height)
        {
            // Arrange
            var dto = new GameInitDto(width, height, [new(Orientation.Horizontal, ['@', '@', '@'], new Point(0, 0))]);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage(ValidationMessages.InvalidFieldSize);
        }

        [Fact]
        public void Validate_TooManyBricks_ShouldHaveError()
        {
            // Arrange: Max is 3, we send 4.
            var brick = new Brick(Orientation.Horizontal, ['@', '@', '@'], new Point(0, 0));
            var bricks = new List<Brick> { brick, brick, brick, brick };
            var dto = new GameInitDto(10, 10, bricks);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Bricks.Count)
                  .WithErrorMessage(ValidationMessages.MaxBricksExceeded);
        }

        [Fact]
        public void Validate_HorizontalBrickOnNarrowField_ShouldHaveError()
        {
            // Arrange: Horizontal brick (needs 3 cols) on a field with Width=2.
            var bricks = new List<Brick> { new(Orientation.Horizontal, ['@', '@', '@'], new Point(0, 0)) };
            var dto = new GameInitDto(2, 10, bricks);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage(ValidationMessages.FieldTooSmallForHorizontal);
        }

        [Fact]
        public void Validate_VerticalBrickOnShortField_ShouldHaveError()
        {
            // Arrange: Vertical brick (needs 3 rows) on a field with Height=2.
            var bricks = new List<Brick> { new(Orientation.Vertical, ['@', '@', '@'], new Point(0, 0)) };
            var dto = new GameInitDto(10, 2, bricks);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage(ValidationMessages.FieldTooSmallForVertical);
        }
    }
}
