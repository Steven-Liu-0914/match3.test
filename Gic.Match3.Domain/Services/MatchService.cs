using Gic.Match3.Domain.Models;

namespace Gic.Match3.Domain.Services
{
    public interface IMatchService : IBaseService
    {
        void ProcessMatches(GameField field);
    }

    /// <summary>
    /// Service responsible for identifying and processing match-3 patterns on the game field.
    /// </summary>
    public class MatchService(GameRulesOptions options) : IMatchService
    {
        private readonly GameRulesOptions _options = options;

        /// <summary>
        /// The main entry point for elimination logic. 
        /// Continuously scans and clears matches until the board state is stable (no more matches).
        /// </summary>
        /// <param name="field">The current game board.</param>
        public void ProcessMatches(GameField field)
        {
            // If EnableAutoDrop is true, falling blocks might create new combos, keeping the loop going.
            // If EnableAutoDrop is false, it clears everything at once and stops after 1 iteration.
            while (TryFindAndProcessAllMatches(field))
            {
                // The loop continues until TryFindAndProcessFirstMatch returns false.
            }
        }

        /// <summary>
        /// Scans the entire board for ALL matches (handling crosses and L-shapes).
        /// Clears them simultaneously, and applies gravity if configured.
        /// </summary>
        private bool TryFindAndProcessAllMatches(GameField field)
        {
            // Use a HashSet to store unique coordinates of all matched cells.
            var matchedCells = new HashSet<(int Row, int Col)>();

            // Scan the ENTIRE board and collect all matches FIRST
            ScanHorizontalMatches(field, matchedCells);
            ScanVerticalMatches(field, matchedCells);

            if (matchedCells.Count > 0)
            {
                // 1. Clear ALL matched cells at the exact same time
                foreach (var (r, c) in matchedCells)
                {
                    field.SetCell(r, c, _options.EmptySymbol);
                }

                // 2. Feature Toggle: Apply gravity only if configured to do so
                if (_options.EnableAutoDropAfterMatch)
                {
                    ApplyAutoDropForAllCells(field);
                }

                return true; // Found and processed matches, loop should check again
            }

            return false; // Board is stable, no more matches
        }

        /// <summary>
        /// Scans the field row-by-row for consecutive identical symbols and records their coordinates.
        /// </summary>
        private void ScanHorizontalMatches(GameField field, HashSet<(int, int)> matchedCells)
        {
            for (int v = 0; v < field.Height; v++)
            {
                for (int h = 0; h < field.Width - 2; h++)
                {
                    var cell = field.GetCell(v, h);
                    if (cell == _options.EmptySymbol) continue;

                    var nextCell = field.GetCell(v, h + 1);
                    var nextNextCell = field.GetCell(v, h + 2);

                    if (cell == nextCell && cell == nextNextCell)
                    {
                        // Record coordinates instead of clearing immediately.
                        // DO NOT 'return' here, keep scanning the rest of the row
                        matchedCells.Add((v, h));
                        matchedCells.Add((v, h + 1));
                        matchedCells.Add((v, h + 2));
                    }
                }
            }
        }

        /// <summary>
        /// Scans the field column-by-column for consecutive identical symbols and records their coordinates.
        /// </summary>
        private void ScanVerticalMatches(GameField field, HashSet<(int, int)> matchedCells)
        {
            for (int h = 0; h < field.Width; h++)
            {
                for (int v = 0; v < field.Height - 2; v++)
                {
                    var cell = field.GetCell(v, h);
                    if (cell == _options.EmptySymbol) continue;

                    var nextCell = field.GetCell(v + 1, h);
                    var nextNextCell = field.GetCell(v + 2, h);

                    if (cell == nextCell && cell == nextNextCell)
                    {
                        // Record coordinates instead of clearing immediately.
                        matchedCells.Add((v, h));
                        matchedCells.Add((v + 1, h));
                        matchedCells.Add((v + 2, h));
                    }
                }
            }
        }

        /// <summary>
        /// Iterates from bottom to top, moving every non-empty block down as far as possible.
        /// </summary>
        private void ApplyAutoDropForAllCells(GameField field)
        {
            for (int r = field.Height - 2; r >= 0; r--)
            {
                for (int c = 0; c < field.Width; c++)
                {
                    char cell = field.GetCell(r, c);
                    if (cell == _options.EmptySymbol) continue;

                    int dropDistance = field.GetMaxDropDistance(r, c);

                    if (dropDistance > 0)
                    {
                        field.SetCell(r + dropDistance, c, cell);
                        field.SetCell(r, c, _options.EmptySymbol);
                    }
                }
            }
        }
    }
}
