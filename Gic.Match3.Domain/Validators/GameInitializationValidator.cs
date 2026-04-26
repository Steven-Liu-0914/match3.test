using FluentValidation;
using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;

namespace Gic.Match3.Domain.Validators
{
    /// <summary>
    /// This class checks if the initial game settings (like board size and number of blocks) 
    /// entered by the user are valid before the game starts.
    /// </summary>
    public class GameInitializationValidator : AbstractValidator<GameInitDto>
    {
        /// <summary>
        /// Sets up all the rules for checking the initial game setup.
        /// </summary>
        public GameInitializationValidator(GameRulesOptions options)
        {
            // Make sure the board has an actual size (both width and height must be greater than zero).
            RuleFor(x => x)
                .Must(dto => dto.Width > 0 && dto.Height > 0)
                .WithMessage(ValidationMessages.InvalidFieldSize);

            // Check that the user provided at least one brick, but not more than the maximum allowed limit.
            RuleFor(x => x.Bricks.Count)
                .Must(count => count >= 1 && count <= options.MaxBricks)
                .WithMessage(ValidationMessages.MaxBricksExceeded);

            // Check every single brick provided by the user to make sure its individual shape and symbols are correct.
            RuleForEach(x => x.Bricks)
                .SetValidator(new BrickValidator(options));

            // If the user wants to drop a horizontal brick, the board must be wide enough (at least 3 columns) to fit it.
            RuleFor(x => x)
               .Must(dto => !dto.Bricks.Any(b => b.Orientation == Orientation.Horizontal) || dto.Width >= 3)
               .WithMessage(ValidationMessages.FieldTooSmallForHorizontal);

            // If the user wants to drop a vertical brick, the board must be tall enough (at least 3 rows) to fit it.
            RuleFor(x => x)
               .Must(dto => !dto.Bricks.Any(b => b.Orientation == Orientation.Vertical) || dto.Height >= 3)
               .WithMessage(ValidationMessages.FieldTooSmallForVertical);
        }
    }
}