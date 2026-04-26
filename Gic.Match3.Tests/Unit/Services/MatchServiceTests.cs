using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Services;

namespace Gic.Match3.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for the Match-3 elimination and gravity algorithm.
    /// Verifies horizontal/vertical clearing, gravity drops, and recursive chain reactions.
    /// </summary>
    public class MatchServiceTests
    {
        private static GameRulesOptions GetDefaultOptions()
        {
            return new GameRulesOptions
            {
                EmptySymbol = '.',
                BrickSize = 3,
                EnableAutoDropAfterMatch = false // Default to strict assignment rules
            };
        }

        #region Pattern Recognition Tests (HashSet Logic)

        [Fact]
        public void ProcessMatches_CrossShape_ShouldClearAllWithoutMissing()
        {
            // Arrange: A cross shape of '@'
            // . @ .
            // @ @ @
            // . @ .
            var options = GetDefaultOptions();
            var service = new MatchService(options);
            var field = new GameField(3, 3, '.');

            field.SetCell(0, 1, '@');
            field.SetCell(1, 0, '@');
            field.SetCell(1, 1, '@'); // Intersection point
            field.SetCell(1, 2, '@');
            field.SetCell(2, 1, '@');

            // Act
            service.ProcessMatches(field);

            // Assert: The entire cross should be cleared simultaneously
            field.IsEmpty(0, 1).Should().BeTrue();
            field.IsEmpty(1, 0).Should().BeTrue();
            field.IsEmpty(1, 1).Should().BeTrue(); // Center must be gone
            field.IsEmpty(1, 2).Should().BeTrue();
            field.IsEmpty(2, 1).Should().BeTrue();
        }

        [Fact]
        public void ProcessMatches_Match4_ShouldClearAllFourCells()
        {
            // Arrange: 4 consecutive '*'
            var options = GetDefaultOptions();
            var service = new MatchService(options);
            var field = new GameField(4, 4, '.');

            field.SetCell(0, 0, '*');
            field.SetCell(0, 1, '*');
            field.SetCell(0, 2, '*');
            field.SetCell(0, 3, '*');

            // Act
            service.ProcessMatches(field);

            // Assert: HashSet should collect all 4 points without index errors
            field.IsEmpty(0, 0).Should().BeTrue();
            field.IsEmpty(0, 1).Should().BeTrue();
            field.IsEmpty(0, 2).Should().BeTrue();
            field.IsEmpty(0, 3).Should().BeTrue();
        }

        #endregion

        #region Feature Toggle Tests (Gravity Logic)

        [Fact]
        public void ProcessMatches_WhenAutoDropIsFalse_BlocksShouldHover()
        {
            // Arrange: Match at the bottom, block on top
            // . X .
            // ^ ^ ^
            var options = GetDefaultOptions();
            options.EnableAutoDropAfterMatch = false; // STRCIT MODE

            var service = new MatchService(options);
            var field = new GameField(3, 2, '.');

            field.SetCell(0, 1, 'X');
            field.SetCell(1, 0, '^');
            field.SetCell(1, 1, '^');
            field.SetCell(1, 2, '^');

            // Act
            service.ProcessMatches(field);

            // Assert: The '^' match is gone, but 'X' stays at Row 0 (Hovering)
            field.IsEmpty(1, 1).Should().BeTrue();
            field.GetCell(0, 1).Should().Be('X');
        }

        [Fact]
        public void ProcessMatches_WhenAutoDropIsTrue_BlocksShouldFallAndCombo()
        {
            // Arrange: A scenario that requires gravity AND a combo
            // Initial state:
            // C . .  <- Row 0
            // C . .  <- Row 1
            // A A A  <- Row 2 (Will clear first)
            // C . .  <- Row 3
            var options = GetDefaultOptions();
            options.EnableAutoDropAfterMatch = true; // ADVANCED MODE

            var service = new MatchService(options);
            var field = new GameField(3, 4, '.');

            field.SetCell(2, 0, 'A');
            field.SetCell(2, 1, 'A');
            field.SetCell(2, 2, 'A');

            field.SetCell(0, 0, 'C');
            field.SetCell(1, 0, 'C');
            field.SetCell(3, 0, 'C');

            // Act
            service.ProcessMatches(field);

            // Assert
            // 1. 'A's clear.
            // 2. 'C's drop down forming a vertical C-C-C match at Row 1,2,3.
            // 3. The while() loop catches the new 'C' match and clears it too.
            field.IsEmpty(2, 0).Should().BeTrue("A should be cleared");
            field.IsEmpty(3, 0).Should().BeTrue("C combo should be cleared");
            field.IsEmpty(1, 0).Should().BeTrue("C combo should be cleared");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void ProcessMatches_NoMatch_ShouldNotChangeField()
        {
            var options = GetDefaultOptions();
            var service = new MatchService(options);
            var field = new GameField(3, 3, '.');
            field.SetCell(0, 0, 'X');
            field.SetCell(0, 1, 'Y');
            field.SetCell(0, 2, 'Z');

            // Act
            service.ProcessMatches(field);

            // Assert: Field remains untouched
            field.GetCell(0, 0).Should().Be('X');
            field.GetCell(0, 1).Should().Be('Y');
            field.GetCell(0, 2).Should().Be('Z');
        }

        #endregion
    }
}
