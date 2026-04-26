using FluentValidation;
using Gic.Match3.Domain.Consts;

namespace Gic.Match3.Domain.Validators
{
    /// <summary>
    /// Validates the user's input when prompted for a post-game action.
    /// This ensures the game lifecycle only proceeds with explicit 'Start Over' or 'Quit' commands.
    /// </summary>
    public class NextActionValidator : AbstractValidator<string>
    {
        public NextActionValidator()
        {
            // Validates that the input string matches one of the two allowed terminal state commands.
            RuleFor(x => x)
                .Must(s => s == Commands.StartOver || s == Commands.Quit)
                .WithMessage(ValidationMessages.InvalidNextAction);
        }
    }
}
