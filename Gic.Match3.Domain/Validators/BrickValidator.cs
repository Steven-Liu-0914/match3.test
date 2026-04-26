using FluentValidation;
using Gic.Match3.Domain.Models;

namespace Gic.Match3.Domain.Validators;

/// <summary>
/// This class checks if a single falling brick is built correctly 
/// before it is allowed to enter the game board.
/// </summary>
public class BrickValidator : AbstractValidator<Brick>
{
    /// <summary>
    /// Sets up the rules for a brick based on the current game settings.
    /// </summary>
    public BrickValidator(GameRulesOptions options)
    {
        // Make sure the brick is facing a valid direction (like horizontal or vertical).
        // If it is an unknown direction, show an error.
        RuleFor(x => x.Orientation)
               .IsInEnum()
               .WithMessage(x => ValidationMessages.InvalidOrientation((char)x.Orientation));

        // Check that the brick has the exact number of symbols required by the game settings.
        // For example, if the rules say a brick must have 3 blocks, it cannot have 2 or 4.
        RuleFor(x => x.Symbols)
            .Must(s => s != null && s.Length == options.BrickSize)
            .WithMessage(ValidationMessages.InvalidBrickSize);

        // Look at every single block inside the brick one by one.
        // Make sure the symbol drawn on it is one of the allowed characters (like '@' or '~').
        RuleForEach(x => x.Symbols)
            .Must(s => options.AllowedSymbols.Contains(s))
            .WithMessage((brick, symbol) => string.Format(ValidationMessages.InvalidSymbol(symbol), symbol));
    }
}