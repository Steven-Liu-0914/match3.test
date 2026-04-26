namespace Gic.Match3.Domain.Models;

/// <summary>
/// Represents the global configuration settings for the Match-3 game.
/// </summary>
public class GameRulesOptions
{
    /// <summary>
    /// The section name in the configuration file (e.g., appsettings.json) used to bind these options.
    /// </summary>
    public const string SectionName = "GameRules";

    /// <summary>
    /// The character used to represent an empty cell on the game board (e.g., '.').
    /// </summary>
    public char EmptySymbol { get; set; }

    /// <summary>
    /// The collection of characters allowed to be used as game symbols (e.g., ['@', '#', '~']).
    /// Bricks containing symbols outside this list will be rejected by the validator.
    /// </summary>
    public char[] AllowedSymbols { get; set; } = [];

    /// <summary>
    /// The maximum number of bricks permitted for a single game session. 
    /// This acts as a safety limit to prevent infinite loops or memory issues.
    /// </summary>
    public int MaxBricks { get; set; }

    /// <summary>
    /// The fixed number of symbols that make up a single brick (e.g., 3).
    /// </summary>
    public int BrickSize { get; set; }

    /// <summary>
    /// Defines the initial count of command characters to read from the user input stream 
    /// during the first frame of the game.
    /// </summary>
    public int FirstReadCommands { get; set; }

    /// <summary>
    /// Flag to control whether blocks should fall after a match is cleared.
    /// </summary>
    public bool EnableAutoDropAfterMatch { get; set; }
}