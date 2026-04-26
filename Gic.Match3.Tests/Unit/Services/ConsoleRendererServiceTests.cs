using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Services;

namespace Gic.Match3.Tests.Unit.Services
{
    /// <summary>
    /// Verifies the visual output of the game. 
    /// Uses a StringWriter to capture Console output for assertion.
    /// </summary>
    public class ConsoleRendererServiceTests : IDisposable
    {
        private readonly ConsoleRendererService _renderer;
        private readonly StringWriter _consoleOutput;
        private readonly TextWriter _originalOut;
        private bool _disposed;

        public ConsoleRendererServiceTests()
        {
            _renderer = new ConsoleRendererService();
            _consoleOutput = new StringWriter();

            // Redirect Console.Out to our local StringWriter
            _originalOut = Console.Out;
            Console.SetOut(_consoleOutput);
        }

        [Fact]
        public void DrawFrame_ShouldCombineFieldAndActiveBrick()
        {
            // Arrange
            var field = new GameField(3, 2, '.'); // 3 Width, 2 Height
            var brick = new Brick(Orientation.Horizontal, ['A', 'B'], new Point(0, 0));

            // Act
            _renderer.DrawFrame(1, field, brick);
            var output = _consoleOutput.ToString();

            // Assert
            // Expected Frame 1 Header
            output.Should().Contain("Frame 1");

            // Row 0 should have the brick symbols A and B
            // Row 1 should have empty symbols '.'
            output.Should().Contain("| A B . |");
            output.Should().Contain("| . . . |");
        }

        [Fact]
        public void DrawFinalState_ShouldOnlyDrawFieldContent()
        {
            // Arrange
            var field = new GameField(2, 2, '.');
            field.SetCell(1, 1, '@'); // Set a fixed block

            // Act
            _renderer.DrawFinalState(field);
            var output = _consoleOutput.ToString();

            // Assert
            output.Should().Contain("| . . |");
            output.Should().Contain("| . @ |");
            output.Should().NotContain("Frame"); // Final state shouldn't have frame headers
        }

        [Fact]
        public void DrawFrame_BrickAtBottom_ShouldRenderCorrectly()
        {
            // Arrange
            var field = new GameField(3, 3, '.');
            // Brick is at the very bottom row (Row 2)
            var brick = new Brick(Orientation.Horizontal, ['#'], new Point(2, 1));

            // Act
            _renderer.DrawFrame(99, field, brick);
            var output = _consoleOutput.ToString();

            // Assert
            // Row 0 & 1 should be empty
            output.Should().Contain("| . . . |");
            // Row 2 should have the brick at Col 1
            output.Should().Contain("| . # . |");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Console.SetOut(_originalOut);
                    _consoleOutput.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
