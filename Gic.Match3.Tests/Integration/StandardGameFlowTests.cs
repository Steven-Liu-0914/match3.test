using Gic.Match3.Domain.Services;
using Gic.Match3.Tests.Integration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Gic.Match3.Tests.Integration
{
    public class StandardGameFlowTests : IntegrationTestBase
    {
        public StandardGameFlowTests() : base(IntegrationTestExtensions.LoadGameOptions()) { }

        [Fact(Timeout = 10000)]
        public async Task Completed_Story_Core_Gameplay_Loop()
        {
            var runner = ServiceProvider.GetRequiredService<IGameRunner>();
            var gameTask = this.StartGameAndVerifyWelcome(runner);

            // Initialize a 5x8 board with two bricks
            this.SendInit(5, 8, "H^^* V*@^");

            // Frame 1: Initial spawn of H^^* at the top center
            var output = GetNewPrint();
            var expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". ^ ^ * ." }
            });
            output.Should().Contain(expected);
            output.Should().Contain(PromptMessages.FrameCommand);

            // Frame 2: Move Left twice (LL)
            SendInput("LL");
            output = GetNewPrint();
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 2, "^ ^ * . ." }
            });
            output.Should().Contain(expected);

            // Frame 3: Move Right once (R)
            SendInput("R");
            output = GetNewPrint();
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 3, ". ^ ^ * ." }
            });
            output.Should().Contain(expected);

            // Frame 4: Drop immediately, then start next brick.
            // Next brick (V*@^) spawns at the top center.
            SendInput("DR");
            output = GetNewPrint();
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". . * . ." }, // V*@^ spawning
                { 2, ". . @ . ." },
                { 3, ". . ^ . ." },
                { 8, ". . ^ ^ *" }  // H^^* settled at the bottom
            });
            output.Should().Contain(expected);

            // Frame 5: Move Left, Left, Right (LLR). 
            // The 'R' cancels one 'L', net movement is one Left.
            SendInput("LLR");
            output = GetNewPrint();
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 2, "* . . . ." },
                { 3, "@ . . . ." },
                { 4, "^ . . . ." },
                { 8, ". . ^ ^ *" }
            });
            output.Should().Contain(expected);

            // Frame 6: Empty input (Continue). Brick drops by 1 row due to natural gravity.
            SendInput("");
            output = GetNewPrint();
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 3, "* . . . ." },
                { 4, "@ . . . ." },
                { 5, "^ . . . ." },
                { 8, ". . ^ ^ *" }
            });
            output.Should().Contain(expected);

            // Frame 7: Move Right (R)
            SendInput("R");
            output = GetNewPrint();
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 4, ". * . . ." },
                { 5, ". @ . . ." },
                { 6, ". ^ . . ." },
                { 8, ". . ^ ^ *" }
            });
            output.Should().Contain(expected);

            // Frame 8: Drop (D). This triggers a match and clears the bottom row.
            // Strict Assignment Rules: No Auto-Drop after match. Remaining blocks hover.
            SendInput("DR");
            output = GetNewPrint();
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 6, ". * . . ." }, // Hovering
                { 7, ". @ . . ." }, // Hovering
                { 8, ". . . . *" }  // Bottom matched and cleared
            });
            output.Should().Contain(expected);

            // Verify end game conditions
            output.Should().Contain(StatusMessages.GameEndsNoBricks);
            output.Should().Contain(PromptMessages.NextAction);

            // Quit the game session
            SendInput("Q");
            await gameTask;
            this.VerifyGameExit();
        }
    }
}