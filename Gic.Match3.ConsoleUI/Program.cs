using Microsoft.Extensions.DependencyInjection;
using Gic.Match3.Domain.Helpers;
using Gic.Match3.Domain.Services;

/// <summary>
/// Main entry point for the Match-3 Game.
/// </summary>

// Wrap the entire application execution in a global try-catch block.
// This prevents the console window from abruptly closing (crashing to desktop) if an unhandled exception occurs
try
{
    // Step 1: Load Configuration from appsettings.json via Static Helper
    var options = ConfigurationHelper.LoadConfiguration();

    // Step 2: Set up the Dependency Injection (DI) container
    // Uses automated assembly scanning to register all services and validators
    var serviceProvider = new ServiceCollection()
        .AddServices(options)
        .BuildServiceProvider();

    // Step 3: Resolve the Game Orchestrator (GameRunner)
    // This service coordinates the parser, validator, engine, and renderer
    var gameRunner = serviceProvider.GetRequiredService<IGameRunner>();

    // Step 4: Execute the game initialization and logic
    gameRunner.StartExecute();
}
catch (Exception ex)
{
    // Global Exception Handler
    // Highlight the error in red to alert, ensuring the error message is caught and displayed before termination.
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine();
    Console.WriteLine("==================================================");
    Console.WriteLine("[FATAL ERROR] The game encountered an unexpected exception.");
    Console.WriteLine("==================================================");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");

    Console.ResetColor();
}
finally
{
    // Graceful Exit
    // Whether the game exits normally (e.g., user presses 'Q') or crashes,
    // this ensures the console window remains open until the user acknowledges it.
    Console.WriteLine();
    Console.WriteLine("Press ENTER to close the program...");
    Console.ReadLine();
}