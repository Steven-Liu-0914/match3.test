using Gic.Match3.Domain.Models;
using System.Text;

namespace Gic.Match3.Domain.Services
{
    public interface IConsoleRendererService : IBaseService
    {
        void DrawFrame(int frameNumber, GameField field, Brick activeBrick);

        void DrawFinalState(GameField field);
    }

    public class ConsoleRendererService : IConsoleRendererService
    {
        /// <summary>
        /// Draws the current state of the game board onto the screen.
        /// It combines the stationary blocks on the board with the moving brick, 
        /// showing the moving brick exactly where it is currently falling.
        /// Format: | . . . . . |.
        /// </summary>
        public void DrawFrame(int frameNumber, GameField field, Brick activeBrick)
        {
            // Print the title for the current turn
            Console.WriteLine($"Frame {frameNumber}");

            // Get the exact grid positions of the falling brick
            var activePoints = activeBrick.GetOccupiedPoints().ToList();

            // Go through the board row by row
            for (int r = 0; r < field.Height; r++)
            {
                var sb = new StringBuilder("| "); // Start the row with a left border

                // Go through each column in the current row
                for (int c = 0; c < field.Width; c++)
                {
                    // Check if the falling brick has a block at this exact row and column
                    var brickBlock = activePoints.FirstOrDefault(p => p.Row == r && p.Col == c);

                    if (brickBlock != null)
                    {
                        // If the falling brick is here, draw its symbol
                        int index = activePoints.IndexOf(brickBlock);
                        sb.Append(activeBrick.Symbols[index] + " ");
                    }
                    else
                    {
                        // If the falling brick is not here, draw whatever is on the background board
                        sb.Append(field.GetCell(r, c) + " ");
                    }
                }

                sb.Append('|'); // End the row with a right border
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Draws the game board when nothing is moving.
        /// It only shows the blocks that are permanently locked into the grid and the empty spaces.
        /// </summary>
        public void DrawFinalState(GameField field)
        {
            // Go through the board row by row
            for (int row = 0; row < field.Height; row++)
            {
                // Start the row with a left border
                Console.Write("|");

                // Go through each column in the current row
                for (int col = 0; col < field.Width; col++)
                {
                    // Draw the symbol currently saved in the board's grid
                    Console.Write($" {field.GetCell(row, col)}");
                }

                // End the row with a right border
                Console.WriteLine(" |");
            }
        }
    }
}