using Gic.Match3.Domain.Enums;

namespace Gic.Match3.Domain.Consts
{
    public static class Commands
    {
        public const string Quit = "Q";

        public const string StartOver = "S";

        public static readonly IEnumerable<char> AllowedCommandChars = Enum.GetNames<MoveCommand>().Where(x => x != nameof(MoveCommand.Continue)).Select(name => name.ToUpper()[0]); // Extracts Valid Commands from enum names
        public static readonly IEnumerable<char> AllowedOrientationChars = Enum.GetNames<Orientation>().Where(x => x != nameof(Orientation.Invalid)).Select(name => name.ToUpper()[0]); // Extracts Valid Commands from enum names
    }
}
