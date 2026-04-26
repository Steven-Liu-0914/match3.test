using Gic.Match3.Domain.Helpers;
using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gic.Match3.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for the GameRunner orchestrator.
    /// Verifies the high-level game loop, including initialization and exit conditions.
    /// </summary>
    public class GameRunnerTests : IDisposable
    {
        private readonly GameRulesOptions _options;
        private readonly ServiceProvider _serviceProvider;
        private readonly IGameRunner _runner;

        private readonly StringWriter _consoleOutput;
        private readonly TextWriter _originalOut;
        private readonly TextReader _originalIn;
        private bool _disposedValue; 

        public GameRunnerTests()
        {
            // 1. Setup real dependencies for a "Functional" Unit Test
            _options = new GameRulesOptions
            {
                BrickSize = 3,
                EmptySymbol = '.',
                MaxBricks = 5,
                FirstReadCommands = 10,
                AllowedSymbols = ['@', '#', '~']
            };

            var services = new ServiceCollection();
            // Use your existing Extension method to register everything correctly
            services.AddServices(_options);
            _serviceProvider = services.BuildServiceProvider();
            _runner = _serviceProvider.GetRequiredService<IGameRunner>();

            // 2. Redirect Console I/O
            _consoleOutput = new StringWriter();
            _originalOut = Console.Out;
            _originalIn = Console.In;
            Console.SetOut(_consoleOutput);
        }

        [Fact]
        public void StartExecute_QuitImmediately_ShouldShowWelcomeAndThankYou()
        {
            // Arrange: 
            // 1st input: Init string "3 3 H@@@"
            // 2nd input: Command "D" (to finish the brick)
            // 3rd input: Next action "Q" (to quit)
            var input = new StringReader("3 3 H@@@\nD\nQ\n");
            Console.SetIn(input);

            // Act
            _runner.StartExecute();

            // Assert
            var output = _consoleOutput.ToString();
            output.Should().Contain(PromptMessages.Welcome);
            output.Should().Contain(StatusMessages.ThankYou);
        }

        [Fact]
        public void StartExecute_WhenInitialPositionBlocked_ShouldEndGameSession()
        {
            // Arrange
            // Line 1: Setup 3x3 board with two bricks
            // Line 2: Drop the first brick (D) so it settles at the bottom row
            // Line 3: Quit (Q) after the game ends due to the blockage
            var input = new StringReader("3 3 H@~~ V@~~\nD\nQ\n");
            Console.SetIn(input);

            // Act
            _runner.StartExecute();

            // Assert
            var output = _consoleOutput.ToString();

            // Check if the specific blocking message was displayed
            output.Should().Contain(StatusMessages.GameEndsBlocked);

            // Ensure the game moved to the "Next Action" prompt and handled the 'Q' command
            output.Should().Contain(StatusMessages.ThankYou);
        }

        [Fact]
        public void StartExecute_InvalidInputThenCorrectInput_ShouldShowValidationError()
        {
            // Arrange: 
            // 1st input: Invalid init "abc"
            // 2nd input: Valid init "3 3 H@@@"
            // 3rd input: Command "D"
            // 4th input: Quit "Q"
            var input = new StringReader("abc\n3 3 H@@@\nD\nQ\n");
            Console.SetIn(input);

            // Act
            _runner.StartExecute();

            // Assert
            var output = _consoleOutput.ToString();
            output.Should().Contain("[Validation Error]");
            output.Should().Contain(StatusMessages.ThankYou);
        }

        #region Standard Dispose Pattern

        /// <summary>
        /// Implements the full Dispose
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // 1. Restore global state - critical for Console tests
                    Console.SetOut(_originalOut);
                    Console.SetIn(_originalIn);

                    // 2. Dispose managed objects
                    _consoleOutput.Dispose();
                    _serviceProvider.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Entry point for resource cleanup.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}