namespace Gic.Match3.Domain.Models
{
    /// <summary>
    /// Represents a coordinate in the grid.
    /// </summary>
    public record Point
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public Point(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public void MoveBy(int rowMove, int colMove)
        {
            Row += rowMove;
            Col += colMove;
        }
    }
}
