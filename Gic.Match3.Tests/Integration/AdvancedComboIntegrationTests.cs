using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Services;
using Gic.Match3.Tests.Integration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Gic.Match3.Tests.Integration
{
    public class AdvancedComboIntegrationTests : IntegrationTestBase
    {
        public AdvancedComboIntegrationTests() : base(new GameRulesOptions
        {
            BrickSize = 3,
            MaxBricks = 5,
            EmptySymbol = '.',
            AllowedSymbols = ['@', '~', '*'],
            FirstReadCommands = 2,
            EnableAutoDropAfterMatch = true // Turn ON the Auto-Drop After Match feature for this test to verify the chain reaction properly
        })
        { }

        [Fact(Timeout = 10000)]
        public async Task FeatureToggle_AutoDrop_ShouldTrigger_MultiStep_ChainReaction()
        {
            var runner = ServiceProvider.GetRequiredService<IGameRunner>();
            var gameTask = this.StartGameAndVerifyWelcome(runner);

            // 1. Setup the "Chain Reaction" scenario on a 5x8 board.
            // Brick 1 (H@*@): The foundation.
            // Brick 2 (V@**): The detonator.
            // Brick 3 (H~~~): The self-destructing finale.
            this.SendInit(5, 8, "H@*@ V@** H~~~");

            // Frame 1: Base brick (H@*@) appears at top center.
            var output = GetNewPrint();
            var expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". @ * @ ." }
            });
            output.Should().Contain(expected);
            output.Should().Contain(PromptMessages.FrameCommand);

            // Drop base brick to the very bottom (Row 8)
            SendInput("D");
            output = GetNewPrint();

            // Frame 2: Vertical detonator brick (V@**) appears. 
            // Bottom row (Row 8) is currently waiting with: `. @ * @ .`
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". . @ . ." }, // V-brick Spawns at Col 2
                { 2, ". . * . ." },
                { 3, ". . * . ." },
                { 8, ". @ * @ ." }  // Base brick settled at the bottom
            });
            output.Should().Contain(expected);

            // THE COMBO MOMENT: Drop the Vertical brick directly down.
            // Step-by-step resolution inside MatchService:
            // 1. V@** lands on Col 2. Its bottom '*' hits Row 8's '*'. It occupies Rows 5, 6, 7.
            // 2. Vertical Match Detected: '*' at Rows 6, 7, 8.
            // 3. Match clears! Rows 6, 7, 8 in Col 2 become empty.
            // 4. AUTO-DROP TRIGGERS: The '@' at Row 5 loses its support and drops down 3 spaces to Row 8.
            // 5. Row 8 becomes `. @ @ @ .`.
            // 6. Loop continues: Horizontal Match Detected for '@' at Row 8!
            // 7. Match clears! Row 8 is now completely empty.
            SendInput("D");
            output = GetNewPrint();

            // Frame 3: Final brick (H~~~) appears. 
            // The entire board below it MUST BE WIPED CLEAN by the multi-step combo.
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". ~ ~ ~ ." }, // The new brick
                { 2, ". . . . ." },
                { 3, ". . . . ." },
                { 8, ". . . . ." }  // The entire foundation and detonator vanished!
            });
            output.Should().Contain(expected);

            // Verify no remnants of '@' or '*' anywhere on the board
            output.Should().NotContain("@");
            output.Should().NotContain("*");

            // It will land on Row 8, form a horizontal "~ ~ ~" match, and annihilate ITSELF!
            SendInput("D");
            output = GetNewPrint();

            // Frame 4: Verify complete board emptiness after the self-destruct.
            expected = this.BuildExpectedGrid(new Dictionary<int, string>
            {
                { 1, ". . . . ." },
                { 2, ". . . . ." },
                { 3, ". . . . ." },
                { 8, ". . . . ." }  // Completely empty!
            });
            output.Should().Contain(expected);

            // Verify end game conditions
            output.Should().Contain(StatusMessages.GameEndsNoBricks);
            output.Should().Contain(PromptMessages.NextAction);

            // Exit gracefully
            SendInput("Q");
            await gameTask;
            this.VerifyGameExit();
        }
    }
}