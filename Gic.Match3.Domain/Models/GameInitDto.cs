namespace Gic.Match3.Domain.Models
{
    /// <summary>
    /// A raw data container for initial setup before validation.
    /// </summary>
    public record GameInitDto(int Width, int Height, List<Brick> Bricks);
}
