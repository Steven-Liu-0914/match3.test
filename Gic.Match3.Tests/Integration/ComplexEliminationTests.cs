using Gic.Match3.Domain.Services;
using Gic.Match3.Tests.Integration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Gic.Match3.Tests.Integration
{
    public class ComplexEliminationTests : IntegrationTestBase
    {
        public ComplexEliminationTests() : base(IntegrationTestExtensions.LoadGameOptions()) { }

        [Fact(Timeout = 10000)]
        public async Task Advanced_InvertedT_Intersection_ShouldClearSimultaneously()
        {
            var runner = ServiceProvider.GetRequiredService<IGameRunner>();
            var gameTask = this.StartGameAndVerifyWelcome(runner);

            // 1. Setup the "Valley": 
            // We use two V@^* bricks to construct the left and right walls.
            // Then, we drop one V*** in the middle to form an Inverted-T.
            // H~~~ is the final brick to ensure the game doesn't end immediately after the match.
            this.SendInit(5, 8, "V@^* V@^* V*** H~~~");

            // Frame 1: The first wall brick (V@^*) spawns at the top-center (Col 2).
            var output = GetNewPrint();
            var expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". . @ . ." }, // Top symbol of V@^*
                { 2, ". . ^ . ." }, // Middle symbol of V@^*
                { 3, ". . * . ." }, // Bottom symbol of V@^*
            });
            output.Should().Contain(expected);
            output.Should().Contain(PromptMessages.FrameCommand);

            // Move the first brick to the Left (Col 1) and Drop it to the bottom.
            SendInput("LD");
            output = GetNewPrint();

            // Frame 2: The second wall brick (V@^*) spawns at the top-center (Col 2).
            // The first brick is now settled at the bottom of Col 1, forming the left wall.
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". . @ . ." }, // New brick spawning
                { 2, ". . ^ . ." },
                { 3, ". . * . ." },
                { 6, ". @ . . ." }, // Left wall top
                { 7, ". ^ . . ." }, // Left wall middle
                { 8, ". * . . ." }  // Left wall bottom 
            });
            output.Should().Contain(expected);
            output.Should().Contain(PromptMessages.FrameCommand);

            // Move the second brick to the Right (Col 3) and Drop it to the bottom.
            SendInput("RD");
            output = GetNewPrint();

            // Frame 3: The magic V*** brick spawns at the top-center (Col 2).
            // Both the left and right walls are now perfectly established, creating a "valley" in Col 2.
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". . * . ." }, // The magic V*** brick spawning
                { 2, ". . * . ." },
                { 3, ". . * . ." },
                { 6, ". @ . @ ." }, // Tops of both walls
                { 7, ". ^ . ^ ." }, // Middles of both walls
                { 8, ". * . * ." }  // The bottom of the valley, waiting for the final '*'
            });
            output.Should().Contain(expected);

            // Drop V*** directly into the valley!
            // 1. It lands exactly in Col 2.
            // 2. The bottom '*' of V*** lands on Row 8, completing the Horizontal Match (* * *).
            // 3. The entire V*** completes the Vertical Match (* at Row 6, 7, 8).
            // 4. HashSet processes BOTH lines simultaneously, annihilating the Inverted-T.
            SendInput("D");
            output = GetNewPrint();

            // Frame 4: H~~~ spawns at the top-center.
            // Because EnableAutoDropAfterMatch is false (strict rules), 
            // the '@' and '^' symbols from the walls will HOVER in place.
            // However, the entire T-shape of '*' must be completely cleared.
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". ~ ~ ~ ." }, // The new H~~~ brick
                { 6, ". @ . @ ." }, // Hovering wall symbols
                { 7, ". ^ . ^ ." }, // Hovering wall symbols
                { 8, ". . . . ." }  // Completely empty! The T-shape was annihilated!
            });
            output.Should().Contain(expected);

            // Drop the final H~~~ brick to trigger the game over sequence.
            SendInput("D");
            output = GetNewPrint();

            // Frame 5: H~~~ settles on Row 5, just above the hovering walls.
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 5, ". . . . ." }, // H~~~ trigger 3 matches with itself
                { 6, ". @ . @ ." }, // Hovering
                { 7, ". ^ . ^ ." }, // Hovering
                { 8, ". . . . ." }  // Still empty
            });
            output.Should().Contain(expected);

            // Verify end-game conditions as no bricks are left.
            output.Should().Contain(StatusMessages.GameEndsNoBricks);
            output.Should().Contain(PromptMessages.NextAction);

            // Exit gracefully
            SendInput("Q");
            await gameTask;
            this.VerifyGameExit();
        }
    }
}