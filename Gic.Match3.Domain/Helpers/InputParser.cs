using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;

namespace Gic.Match3.Domain.Helpers
{
    public static class InputParser
    {
        /// <summary>
        /// Reads the text typed by the player and breaks it down into the game's starting settings (width, height, and bricks).
        /// This method does not check if the rules are followed; it only translates text into game objects.
        /// If the text is invalid, it defaults to 0 or empty values, which are caught later by the validator.
        /// </summary>
        public static GameInitDto ParseInitSetup(string input)
        {
            // Split the typed text into pieces, using spaces as the divider
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Read the width and height from the first two pieces of text.
            // The '_ =' tells the compiler that intentionally ignoring the true/false success result.
            // If the user typed letters instead of numbers, width and height will just default to 0.
            _ = int.TryParse(parts.ElementAtOrDefault(0), out int width);
            _ = int.TryParse(parts.ElementAtOrDefault(1), out int height);

            var bricks = new List<Brick>();

            // Look at all the remaining pieces of text to create the bricks
            for (int i = 2; i < parts.Length; i++)
            {
                string data = parts[i];
                if (string.IsNullOrWhiteSpace(data)) continue;

                // The first letter tells us the direction (e.g., 'H' for Horizontal, 'V' for Vertical)
                Orientation orientation = (Orientation)char.ToUpper(data[0]);

                // The rest of the letters are the symbols drawn on the brick
                // Get the substring starting from the second character, convert it to a char array, and if there are no symbols, use an empty array.
                var symbols = data.Length > 1 ? data[1..].ToCharArray() : [];

                bricks.Add(new Brick(orientation, symbols, new Point(0, 0)));
            }

            return new GameInitDto(width, height, bricks);
        }
    }
}