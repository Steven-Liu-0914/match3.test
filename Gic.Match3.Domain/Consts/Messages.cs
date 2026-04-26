using Gic.Match3.Domain.Models;

namespace Gic.Match3.Domain.Consts
{
    /// <summary>
    /// Holds all the text messages, prompts, and errors used in the game.
    /// Keeping them in one place makes it easy to change what the game says.
    /// </summary>
    public static class Messages
    {
        public static GameRulesOptions Options { get; set; } = new();

        private static string AllowedGameCommands { get; set; } = string.Empty;

        private static string AllowedGameSymbols { get; set; } = string.Empty;

        private static string AllowedGameoOrientation { get; set; } = string.Empty;

        /// <summary>
        /// The error messages shown when a player types something invalid or breaks a rule.
        /// </summary>
        public static class ValidationMessages
        {
            public const string InvalidFieldSize = "Invalid field dimensions. Please provide positive integers for width and height.";
            public static readonly string InvalidBrickSize = $"Each brick must consist of exactly {Options.BrickSize} blocks.";
            public static string InvalidSymbol(char symbol) => $"The symbol '{symbol}' is not allowed. Allowed symbols are: {AllowedGameSymbols}";
            public static readonly string MaxBricksExceeded = $"A maximum of {Options.MaxBricks} bricks can be provided.";
            public static string InvalidCommand(char cmd) => $"Invalid move: '{cmd}' is not a recognized command. Valid commands are {AllowedGameCommands} or Press Enter to direct continue.";
            public static string InvalidOrientation(char input) => $"Invalid orientation: '{input}'. Allowed orientations are {AllowedGameoOrientation}.";
            public static readonly string FieldTooSmallForHorizontal = $"Field width must be at least {Options.BrickSize} to accommodate horizontal bricks.";
            public static readonly string FieldTooSmallForVertical = $"Field height must be at least {Options.BrickSize} to accommodate vertical bricks.";

            public static string InvalidNextAction(string input) => $"Invalid Action: '{input}'. " + PromptMessages.NextAction.TrimEnd(':');

        }

        /// <summary>
        /// The text used to ask the player for input, like asking for the board size or movement commands.
        /// </summary>
        public static class PromptMessages
        {
            public const string Welcome = "Welcome to Match-3 game!";
            public static string Initialization =>
                $"Please enter field size (width height) and up to {Options.MaxBricks} bricks " +
                $"(e.g., 5 10 H{SymbolExample} V{SymbolExample}):";

            public static readonly string FrameCommand = $"{Environment.NewLine}Enter commands to process before moving to the next frame (valid commands are {AllowedGameCommands} or Press Enter to direct continue):";
            public static readonly string NextAction = $"Enter {Commands.StartOver} to start over or {Commands.Quit} to quit:";
        }

        /// <summary>
        /// The text used to tell the player what is happening in the game, like when the game is over.
        /// </summary>
        public static class StatusMessages
        {
            public const string FrameHeader = "Frame {0}";
            public const string GameEndsBlocked = "Starting position of the current brick is blocked. Game ends immediately.";
            public const string GameEndsNoBricks = "No more bricks. Game ends immediately.";
            public const string ThankYou = "Thank you for playing Match-3!";
        }

        /// <summary>
        /// Updates the messages to match the current game rules.
        /// </summary>
        public static void LoadGameRulesOptions(GameRulesOptions newOptions)
        {
            Options = newOptions;
            AllowedGameCommands = string.Join(", ", Commands.AllowedCommandChars);
            AllowedGameSymbols = string.Join(", ", Options.AllowedSymbols);
            AllowedGameoOrientation = string.Join(", ", Commands.AllowedOrientationChars);
        }

        /// <summary>
        /// Generates a dynamic symbol example (e.g., "H@^#" or "V@^#") based on current options.
        /// Includes defensive checks for null or empty allowed symbol sets.
        /// </summary>
        private static string SymbolExample
        {
            get
            {
                // Fallback to 'X' if symbols are not configured.
                var symbols = Options.AllowedSymbols?.Length > 0
                    ? Options.AllowedSymbols
                    : ['x'];

                // Safely take characters up to the configured brick size.
                var sampleChars = symbols.Take(Options.BrickSize);
                return new string([.. sampleChars]);
            }
        }
    }
}