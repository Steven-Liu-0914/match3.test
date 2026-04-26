using FluentValidation;
using Gic.Match3.Domain.Consts;
using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;

namespace Gic.Match3.Domain.Validators
{
    /// <summary>
    /// This class checks if the movement commands typed by the player 
    /// (like moving left, right, or dropping down) are actually valid before the game tries to use them.
    /// </summary>
    public class FrameCommandValidator : AbstractValidator<FrameCommandDto>
    {
        /// <summary>
        /// Sets up the rules to check the player's input step by step.
        /// </summary>
        public FrameCommandValidator()
        {
            // Look at every single letter the user typed in their command string.
            // Make sure each letter matches one of the allowed game commands
            RuleForEach(x => x.RawInput)
                .Must(c => Commands.AllowedCommandChars.Contains(char.ToUpper(c)) || char.ToUpper(c) == (char)MoveCommand.Continue)
                // If a letter is not allowed, show an error message telling the user exactly which letter was wrong.
                .WithMessage((dto, c) => ValidationMessages.InvalidCommand(c));
        }
    }
}