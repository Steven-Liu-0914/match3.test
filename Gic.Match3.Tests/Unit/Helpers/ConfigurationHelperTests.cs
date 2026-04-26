using Gic.Match3.Domain.Helpers;
using Gic.Match3.Domain.Models;
using System.Text.Json;

namespace Gic.Match3.Tests.Unit.Helpers
{
    /// <summary>
    /// Tests the configuration loading logic. 
    /// Uses a temporary JSON file to simulate real appsettings.json behavior.
    /// </summary>
    public class ConfigurationHelperTests : IDisposable
    {
        private const string SettingsFileName = "appsettings.json";

        [Fact]
        public void LoadConfiguration_ValidJson_ShouldBindToOptionsCorrectly()
        {
            // Arrange
            var expectedOptions = new
            {
                GameRules = new GameRulesOptions
                {
                    EmptySymbol = '.',
                    AllowedSymbols = ['@', '#', '~'],
                    MaxBricks = 5,
                    BrickSize = 3,
                    FirstReadCommands = 10
                }
            };

            var jsonContent = JsonSerializer.Serialize(expectedOptions);
            File.WriteAllText(SettingsFileName, jsonContent);

            // Act
            var result = ConfigurationHelper.LoadConfiguration();

            // Assert
            result.Should().NotBeNull();
            result.EmptySymbol.Should().Be('.');
            result.AllowedSymbols.Should().BeEquivalentTo(['@', '#', '~']);
            result.MaxBricks.Should().Be(5);
        }

        [Fact]
        public void LoadConfiguration_MissingSection_ShouldReturnNewOptionsInstance()
        {
            // Arrange
            var emptyConfig = new { OtherSection = "Value" };
            File.WriteAllText(SettingsFileName, JsonSerializer.Serialize(emptyConfig));

            // Act
            var result = ConfigurationHelper.LoadConfiguration();

            // Assert
            result.Should().NotBeNull();
            result.AllowedSymbols.Should().BeEmpty();
        }

        #region Disposable Pattern Implementation

        /// <summary>
        /// Standard Dispose implementation.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Corrects CA1816: Inform the GC that the finalizer does not need to run.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Actual disposal logic.
        /// </summary>
        /// <param name="disposing">True if called from Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && File.Exists(SettingsFileName))
            {
                // Cleanup the physical file created during the test to avoid side effects.
                File.Delete(SettingsFileName);
            }
        }

        #endregion
    }
}
