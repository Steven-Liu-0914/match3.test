using Gic.Match3.Domain.Services;
using Gic.Match3.Tests.Integration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Gic.Match3.Tests.Integration
{
    public class GameEndsBlockedTests : IntegrationTestBase
    {
        public GameEndsBlockedTests() : base(IntegrationTestExtensions.LoadGameOptions()) { }

        [Fact(Timeout = 10000)]
        public async Task Spawn_WhenPositionIsOccupied_ShouldEndGame_WithBlockedMessage()
        {
            var runner = ServiceProvider.GetRequiredService<IGameRunner>();
            var gameTask = this.StartGameAndVerifyWelcome(runner);

            // 1. Setup a compact 5x4 board.
            // We use non-matching alternating symbols (@ and *) so they don't accidentally clear each other.
            // Brick 1 (H@*@): Will settle at Row 4.
            // Brick 2 (H*@*): Will settle at Row 3.
            // Brick 3 (V~~~): Requires Rows 1, 2, and 3 to spawn. It will collide with Row 3!
            this.SendInit(5, 4, "H@*@ H*@* V~~~");

            // Frame 1: First brick spawns at the top
            var output = GetNewPrint();
            var expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". @ * @ ." }
            });
            output.Should().Contain(expected);
            output.Should().Contain(PromptMessages.FrameCommand);

            // Drop the first brick to the bottom
            SendInput("D");
            output = GetNewPrint();

            // Frame 2: Second brick spawns at the top. 
            // The first brick is safely at the bottom (Row 4).
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". * @ * ." }, // Spawning
                { 4, ". @ * @ ." }  // Settled
            });
            output.Should().Contain(expected);
            output.Should().Contain(PromptMessages.FrameCommand);

            // Drop the second brick. It will settle on Row 3, right on top of the first brick.
            SendInput("D");
            output = GetNewPrint();

            // Frame 3: The system attempts to spawn the third brick (V~~~).
            // A vertical brick of size 3 needs Rows 1, 2, and 3 to spawn.
            // However, Row 3 is currently occupied by the second brick!
            // SPAWN BLOCKED! The game must immediately terminate.

            output.Should().Contain(StatusMessages.GameEndsBlocked);
            output.Should().Contain(PromptMessages.NextAction);

            // Exit gracefully using the 'Q' command from the "Start over or Quit" prompt
            SendInput("Q");
            await gameTask;
            this.VerifyGameExit();
        }
    }
}