using Gic.Match3.Domain.Services;
using Gic.Match3.Tests.Integration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Gic.Match3.Tests.Integration
{
    public class BoundaryAndValidationTests : IntegrationTestBase
    {
        public BoundaryAndValidationTests() : base(IntegrationTestExtensions.LoadGameOptions()) { }

        [Fact(Timeout = 10000)]
        public async Task EdgeCase_ValidationFailure_And_WallCollision()
        {
            var runner = ServiceProvider.GetRequiredService<IGameRunner>();
            var gameTask = this.StartGameAndVerifyWelcome(runner);

            // 1. Trigger Validation Error: Send invalid init string
            SendInput("INVALID_STRING_NO_NUMBERS");
            var output = GetNewPrint();

            // Assert: System catches it and reprompts
            output.Should().Contain("[Validation Error]");
            output.Should().Contain(PromptMessages.Initialization);

            // 2. Valid Setup: 5x8 board
            this.SendInit(5, 8, "H@@@");
            output = GetNewPrint();

            // Frame 1: Brick centered with dots on the sides
            var expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". @ @ @ ." }
            });
            output.Should().Contain(expected);

            // 3. Wall Collision: Try to move right 3 times. 
            // It should hit the wall and ignore the 3rd command.
            SendInput("RRR");
            output = GetNewPrint();

            // Assert: Brick moved fully to the right wall, and dropped 1 row (to row 2)
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 2, ". . @ @ @" }
            });
            output.Should().Contain(expected);

            // 4. Drop to bottom and trigger game end.
            SendInput("D");
            output = GetNewPrint();

            // Assert: Drop to bottom row (row 8) -> trigger match 3 clearn 
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 8, ". . . . ." }
            });
            output.Should().Contain(expected);

            //Game ends due to no more bricks.
            output.Should().Contain(StatusMessages.GameEndsNoBricks);
            output.Should().Contain(PromptMessages.NextAction);

            // Quit the game session
            SendInput("Q");
            await gameTask;
            this.VerifyGameExit();
        }
    }
}