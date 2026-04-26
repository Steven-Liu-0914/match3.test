using FluentValidation;
using Gic.Match3.Domain.Consts;
using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Helpers;
using Gic.Match3.Domain.Models;

namespace Gic.Match3.Domain.Services;

public interface IGameRunner : IBaseService
{
    void StartExecute();
}

public class GameRunner(
    GameRulesOptions options,
    IGameEngineService engine,
    IConsoleRendererService renderer,
    IValidator<GameInitDto> initValidator,
    IValidator<FrameCommandDto> frameCommandValidator,
    IValidator<string> nextActionValidator,
    IMatchService matchService) : IGameRunner
{
    private readonly GameRulesOptions _options = options;
    private readonly IGameEngineService _engine = engine;
    private readonly IConsoleRendererService _renderer = renderer;
    private readonly IValidator<GameInitDto> _initValidator = initValidator;
    private readonly IValidator<FrameCommandDto> _frameCommandValidator = frameCommandValidator;
    private readonly IValidator<string> _nextActionValidator = nextActionValidator;
    private readonly IMatchService _matchService = matchService;

    /// <summary>
    /// Controls the main menu loop. It starts the game and asks the user if they want to play again or quit when it ends.
    /// </summary>
    public void StartExecute()
    {
        while (true)
        {
            Console.WriteLine(PromptMessages.Welcome);

            // Start a new game session
            RunGameSession();

            // After the game session ends, ask what to do next
            var choice = GetValidatedInput(
                PromptMessages.NextAction,
                input => input.Trim(),
                _nextActionValidator);

            if (choice == Commands.StartOver)
            {
                Console.Clear();
                continue;
            }

            if (choice == Commands.Quit)
            {
                Console.WriteLine(StatusMessages.ThankYou);
                break;
            }

            break;
        }
    }

    // This function handles a single playthrough of the game.
    // It sets up the board, gets the bricks ready, and drops them one by one until the game is over.
    private void RunGameSession()
    {
        var initDto = GetValidatedInput(
            PromptMessages.Initialization,
            InputParser.ParseInitSetup,
            _initValidator);

        var field = new GameField(initDto.Width, initDto.Height, _options.EmptySymbol);
        var brickQueue = new Queue<Brick>(initDto.Bricks);
        int frameCount = 1;

        while (brickQueue.Count > 0)
        {
            var activeBrick = brickQueue.Dequeue();
            _engine.SetInitialPosition(activeBrick, field.Width);

            // Stop the game immediately if the new brick has no space to appear
            if (_engine.IsInitialPositionBlocked(activeBrick, field))
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(StatusMessages.GameEndsBlocked);
                break;
            }

            // Handle the falling logic for this specific brick
            ProcessActiveBrick(activeBrick, field, ref frameCount);

            // If we just processed the last brick, show the final board
            if (brickQueue.Count == 0)
            {
                Console.WriteLine($"Frame {frameCount}");
                _renderer.DrawFinalState(field);
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(StatusMessages.GameEndsNoBricks);
            }
        }
    }

    // This function controls a single falling brick.
    // It takes user commands, moves the brick, and stops when the brick hits the bottom or another block.
    private void ProcessActiveBrick(Brick activeBrick, GameField field, ref int frameCount)
    {
        bool brickSettled = false;

        while (!brickSettled)
        {
            _renderer.DrawFrame(frameCount, field, activeBrick);

            var commandDto = GetValidatedInput(
                PromptMessages.FrameCommand,
                input => new FrameCommandDto(input),
                _frameCommandValidator);

            //Take only the first 2 commands to prevent excessive input and convert them to MoveCommand enums
            var actualCommands = commandDto.RawInput
                .Take(_options.FirstReadCommands)
                .Select(command => (MoveCommand)char.ToUpper(command))
                .ToList();

            for (int i = 0; i < actualCommands.Count; i++)
            {
                bool isLast = (i == actualCommands.Count - 1);
                bool moveSuccess = _engine.TryMove(activeBrick, field, actualCommands[i], isLast);

                // Stop taking commands if the brick hits something and becomes settled, cannot move anymore
                // Inline the check for settlement here to prevent unnecessary moves after the brick is already settled
                if (isLast && (!moveSuccess || _engine.IsSettled(activeBrick, field)))
                {
                    brickSettled = true;
                    break;
                }
            }

            //Outer check if the brick is settled after processing all VALID commands, to fix it on the board and move to the next brick
            if (brickSettled)
            {
                field.FixBrick(activeBrick);
                _matchService.ProcessMatches(field);
            }

            frameCount++;
        }
    }

    // A helper function asks the user for input and checks if it is valid with custom validator.
    // If the input is wrong, it shows an error and asks again until the user gets it right.
    private static T GetValidatedInput<T>(string prompt, Func<string, T> parser, IValidator<T> validator)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string input = Console.ReadLine() ?? string.Empty;

            // Special handling for frame commands: if the user just presses Enter, treat it as a "Continue" command
            if (prompt == PromptMessages.FrameCommand && string.IsNullOrEmpty(input.Trim()))
            {
                input = ((char)MoveCommand.Continue).ToString();
            }

            T dto = parser(input);
            var result = validator.Validate(dto);

            if (result.IsValid) return dto;

            // Display all validation errors at once
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"[Validation Error]: {error.ErrorMessage}");
            }

            Console.WriteLine("Please try again.\n");
        }
    }
}