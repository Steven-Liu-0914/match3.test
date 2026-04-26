using FluentValidation.TestHelper;
using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Validators;

namespace Gic.Match3.Tests.Unit.Validators
{
    /// <summary>
    /// Unit tests for the BrickValidator to ensure only valid bricks 
    /// are processed by the game engine.
    /// </summary>
    public class BrickValidatorTests
    {
        private readonly GameRulesOptions _options;
        private readonly BrickValidator _validator;

        public BrickValidatorTests()
        {
            // Setup a standard ruleset for testing
            _options = new GameRulesOptions
            {
                BrickSize = 3,
                AllowedSymbols = ['@', '#', '~'],
                EmptySymbol = '.'
            };
            _validator = new BrickValidator(_options);
        }

        [Fact]
        public void Validate_ValidBrick_ShouldNotHaveAnyErrors()
        {
            // Arrange
            var brick = new Brick(Orientation.Horizontal, ['@', '#', '~'], new Point(0, 0));

            // Act
            var result = _validator.TestValidate(brick);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_InvalidOrientation_ShouldHaveError()
        {
            // Arrange: Cast an undefined integer to the enum
            var brick = new Brick((Orientation)99, ['@', '@', '@'], new Point(0, 0));

            // Act
            var result = _validator.TestValidate(brick);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Orientation);
        }

        [Theory]
        [InlineData(2)] // Too short
        [InlineData(4)] // Too long
        public void Validate_WrongBrickSize_ShouldHaveError(int wrongSize)
        {
            // Arrange
            var symbols = new char[wrongSize];
            Array.Fill(symbols, '@');
            var brick = new Brick(Orientation.Horizontal, symbols, new Point(0, 0));

            // Act
            var result = _validator.TestValidate(brick);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Symbols)
                  .WithErrorMessage(ValidationMessages.InvalidBrickSize);
        }

        [Fact]
        public void Validate_ForbiddenSymbol_ShouldHaveError()
        {
            // Arrange: '?' is not in our AllowedSymbols list
            var brick = new Brick(Orientation.Horizontal, ['@', '?', '@'], new Point(0, 0));

            // Act
            var result = _validator.TestValidate(brick);

            // Assert
            // RuleForEach checks every element; we expect a failure for the symbols collection
            result.ShouldHaveValidationErrorFor(x => x.Symbols);
        }

        [Fact]
        public void Validate_NullSymbols_ShouldHaveError()
        {
            // Arrange
            var brick = new Brick(Orientation.Horizontal, null!, new Point(0, 0));

            // Act
            var result = _validator.TestValidate(brick);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Symbols);
        }
    }
}
