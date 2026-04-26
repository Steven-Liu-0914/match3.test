using FluentValidation.TestHelper;
using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Validators;

namespace Gic.Match3.Tests.Unit.Validators
{
    /// <summary>
    /// Unit tests for the FrameCommandValidator.
    /// Ensures that player movement strings (e.g., "DLR") are sanitized 
    /// and only contain recognized game commands.
    /// </summary>
    public class FrameCommandValidatorTests
    {
        private readonly FrameCommandValidator _validator = new();

        [Theory]
        [InlineData("D")]
        [InlineData("L")]
        [InlineData("R")]
        [InlineData("DLR")]
        [InlineData("dlr")] // Testing case-insensitivity
        [InlineData("")]    // Testing "Continue" (Enter key)
        public void Validate_ValidCommands_ShouldNotHaveAnyErrors(string input)
        {
            // Arrange
            var dto = new FrameCommandDto(input);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("X")]
        [InlineData("123")]
        [InlineData("D L")] // Testing invalid spaces
        [InlineData("!")]
        public void Validate_InvalidCommands_ShouldHaveValidationError(string input)
        {
            // Arrange
            var dto = new FrameCommandDto(input);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            // Error is associated with the RawInput property
            result.ShouldHaveValidationErrorFor(x => x.RawInput);
        }

        [Fact]
        public void Validate_MixedValidAndInvalid_ShouldHaveValidationError()
        {
            // Arrange: 'D' and 'L' are valid, but '?' is not
            var dto = new FrameCommandDto("DL?R");

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RawInput);
        }
    }
}
