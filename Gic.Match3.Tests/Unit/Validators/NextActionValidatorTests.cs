using FluentValidation.TestHelper;
using Gic.Match3.Domain.Consts;
using Gic.Match3.Domain.Validators;

namespace Gic.Match3.Tests.Unit.Validators
{
    /// <summary>
    /// Unit tests for the NextActionValidator.
    /// Ensures that the game correctly transitions or terminates based on specific control commands.
    /// </summary>
    public class NextActionValidatorTests
    {
        private readonly NextActionValidator _validator = new();

        [Theory]
        [InlineData(Commands.StartOver)]
        [InlineData(Commands.Quit)]
        public void Validate_ValidTerminalCommands_ShouldNotHaveErrors(string input)
        {
            // Act
            var result = _validator.TestValidate(input);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("X")]    
        [InlineData("QUIT")]   // Testing case-sensitivity if the constant is lowercase
        [InlineData("Continue")]
        [InlineData("1")]
        [InlineData("")]
        public void Validate_InvalidTerminalCommands_ShouldHaveValidationError(string input)
        {
            // Act
            var result = _validator.TestValidate(input);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage(ValidationMessages.InvalidNextAction(input));
        }   
    }
}
