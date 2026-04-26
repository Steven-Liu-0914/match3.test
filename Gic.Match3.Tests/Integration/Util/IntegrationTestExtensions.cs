using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Services;
using System.Text;

namespace Gic.Match3.Tests.Integration.Util
{
    public static class IntegrationTestExtensions
    {
        public static GameRulesOptions LoadGameOptions(GameRulesOptions? options = null)
        {
            if (options == null)
            {
                // Provide standard options for testing if none are supplied
                return new GameRulesOptions
                {
                    BrickSize = 3,
                    MaxBricks = 5,
                    EmptySymbol = '.',
                    AllowedSymbols = ['~', '^', '*', '@'],
                    FirstReadCommands = 2,
                    EnableAutoDropAfterMatch = false
                };
            }

            return options;
        }

        public static void SendInit(this IntegrationTestBase test, int width, int height, string bricks)
        {
            test.SetDimensions(width, height);
            test.SendInput($"{width} {height} {bricks}");
        }
        /// <summary>
        /// Starts the game on a background thread and verifies the welcome messages.
        /// Returns the Task so the test can await it at the end.
        /// </summary>
        public static Task StartGameAndVerifyWelcome(this IntegrationTestBase test, IGameRunner runner)
        {
            var gameTask = Task.Run(() => runner.StartExecute());

            // Use the instance's GetNewPrint via the 'test' parameter
            var output = test.GetNewPrint();
            output.Should().Contain(PromptMessages.Welcome);
            output.Should().Contain(PromptMessages.Initialization);

            return gameTask;
        }

        /// <summary>
        /// Verifies the final state and clean exit.
        /// </summary>
        public static void VerifyGameExit(this IntegrationTestBase test)
        {
            var output = test.GetNewPrint();
            output.Should().Contain(StatusMessages.ThankYou);
        }

        /// <summary>
        /// Constructs the visual grid using the dimensions stored in the test base.
        /// </summary>
        public static string BuildExpectedGrid(this IntegrationTestBase test, Dictionary<int, string> customLines)
        {
            var sb = new StringBuilder();

            // Use the tracked dimensions from the base class
            int width = test.CurrentWidth;
            int height = test.CurrentHeight;

            string emptyRowContent = string.Join(" ", Enumerable.Repeat(".", width));

            for (int r = 1; r <= height; r++)
            {
                string content = customLines.TryGetValue(r, out string? value) ? value : emptyRowContent;
                sb.AppendLine($"| {content.Trim()} |");
            }

            return sb.ToString().Trim();
        }
    }
}
