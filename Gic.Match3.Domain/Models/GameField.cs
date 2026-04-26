namespace Gic.Match3.Domain.Models
{
    public class GameField
    {
        public int Width { get; }
        public int Height { get; }
        private readonly char[,] _grid;
        private readonly char _emptySymbol;

        public GameField(int width, int height, char emptySymbol)
        {
            Width = width;
            Height = height;
            _emptySymbol = emptySymbol;
            _grid = new char[height, width];

            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    _grid[r, c] = _emptySymbol;
        }

        public char GetCell(int row, int col) => _grid[row, col];

        public void SetCell(int row, int col, char value) => _grid[row, col] = value;

        public bool IsEmpty(int row, int col) => _grid[row, col] == _emptySymbol;

        public bool IsWithinBounds(int row, int col) =>
            row >= 0 && row < Height && col >= 0 && col < Width;

        /// <summary>
        /// Transfers the symbols of a moving brick onto the static grid once it settles.
        /// </summary>
        public void FixBrick(Brick brick)
        {
            // Get the exact coordinates of all blocks in the brick
            var points = brick.GetOccupiedPoints().ToList();

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];

                // Defensive check: only write if the point is within the field boundaries
                if (IsWithinBounds(p.Row, p.Col))
                {
                    // Map the corresponding symbol to the grid cell
                    _grid[p.Row, p.Col] = brick.Symbols[i];
                }
            }
        }

        public int GetMaxDropDistance(int row, int col)
        {
            int dropDistance = 0;
            // if the next row is within bounds and empty, we can drop one more row
            while (IsWithinBounds(row + dropDistance + 1, col) &&
                   IsEmpty(row + dropDistance + 1, col))
            {
                dropDistance++;
            }
            return dropDistance;
        }
    }
}
