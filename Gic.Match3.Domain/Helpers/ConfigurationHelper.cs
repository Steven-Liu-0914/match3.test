using Gic.Match3.Domain.Consts;
using Gic.Match3.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace Gic.Match3.Domain.Helpers
{
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Loads the game configuration from appsettings.json and binds it to GameRulesOptions.
        /// </summary>
        public static GameRulesOptions LoadConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var gameRulesOptions = configuration.GetSection("GameRules").Get<GameRulesOptions>() ?? new GameRulesOptions();

            //Load Latest Game Rules to Update Messages
            Messages.LoadGameRulesOptions(gameRulesOptions);

            return gameRulesOptions;
        }
    }
}
