using Gic.Match3.Domain.Enums;


namespace Gic.Match3.Domain.Models
{
    public class Brick(Orientation orientation, char[] symbols, Point startPosition)
    {
        public Orientation Orientation { get; } = orientation;
        public char[] Symbols { get; } = symbols;
        public Point Position { get; private set; } = startPosition;

        public void UpdatePosition(int row, int col)
        {
            Position = new Point(row, col);
        }

        /// <summary>
        /// Returns the points on the grid occupied by this brick.
        /// </summary>
        public IEnumerable<Point> GetOccupiedPoints()
        {
            for (int i = 0; i < Symbols.Length; i++)
            {
                yield return Orientation == Orientation.Horizontal
                    ? new Point(Position.Row, Position.Col + i)
                    : new Point(Position.Row + i, Position.Col);
            }
        }
    }
}
