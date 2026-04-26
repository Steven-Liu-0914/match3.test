using Gic.Match3.Domain.Enums;
using Gic.Match3.Domain.Models;

namespace Gic.Match3.Domain.Services
{
    public interface IGameEngineService : IBaseService
    {
        void SetInitialPosition(Brick brick, int fieldWidth);

        bool TryMove(Brick brick, GameField field, MoveCommand command, bool isLastCommand);

        bool IsSettled(Brick brick, GameField field);

        bool IsInitialPositionBlocked(Brick brick, GameField field);
    }

    public class GameEngineService(GameRulesOptions options) : IGameEngineService
    {
        private readonly GameRulesOptions _options = options;

        /// <summary>
        /// Calculates the starting position at the top-center of the board based on the brick's size.
        /// </summary>
        public void SetInitialPosition(Brick brick, int fieldWidth)
        {
            // A horizontal brick takes up width equal to its size. A vertical brick only takes 1 column.
            int span = (brick.Orientation == Orientation.Horizontal) ? _options.BrickSize : 1;

            // Calculate the starting column to perfectly center the brick
            int startCol = (fieldWidth - span) / 2;

            // Place the brick at the very top row (Row 0)
            brick.UpdatePosition(0, startCol);
        }

        /// <summary>
        /// Tries to move the brick based on the command. 
        /// Invalid horizontal commands (hitting a wall) are safely ignored.
        /// </summary>
        public bool TryMove(Brick brick, GameField field, MoveCommand command, bool isLastCommand)
        {
            var currentPoints = brick.GetOccupiedPoints().ToList();

            // 1. Ask the helper to figure out all the safe movement numbers
            var (FinalCommand, HorizontalMoveStep, VerticalMoveStep, ApplyAutoRowMove) = CalculateMovementReults(brick, field, command, isLastCommand, currentPoints);

            // 2. Give those numbers to the second helper to actually move the points
            ApplyCalculatedMovesToPoints(
                currentPoints,
                FinalCommand,
                HorizontalMoveStep,
                VerticalMoveStep,
                ApplyAutoRowMove);

            // 5. Update the brick's anchor position
            brick.UpdatePosition(currentPoints.Min(x => x.Row), currentPoints.Min(x => x.Col));

            return true;
        }

        // Get the maximum number of rows a brick can drop before it hits an obstacle.
        private static int CalculateMaxDrop(Brick brick, GameField field)
        {
            // Each point might have a different distance to the nearest obstacle below it.
            // The brick as a whole can only move as far as the point with the SHORTEST path.
            return brick.GetOccupiedPoints()
                        .Select(p => field.GetMaxDropDistance(p.Row, p.Col))
                        .Min();
        }

        /// <summary>
        /// Checks if the brick has landed and can no longer move down.
        /// This is used to determine if the brick should be locked into the board permanently.
        /// </summary>
        public bool IsSettled(Brick brick, GameField field)
        {
            // Get the coordinates exactly one row below the brick's current position
            var pointsBelow = brick.GetOccupiedPoints()
                .Select(p => new Point(p.Row + 1, p.Col));

            // The brick is settled if ANY of its blocks touch the floor (field.Height) 
            // or land on a cell that is not empty.
            return pointsBelow.Any(p => p.Row >= field.Height || field.GetCell(p.Row, p.Col) != _options.EmptySymbol);
        }

        // Checks if the brick's starting position is already occupied by other bricks.
        // This is used to detect if the board is full right when a new brick spawns.
        public bool IsInitialPositionBlocked(Brick brick, GameField field)
        {
            // Check the exact coordinates where the brick is trying to appear.
            // Return true if any part of the brick is outside the board or placed on a non-empty cell.
            return brick.GetOccupiedPoints().Any(p => !field.IsWithinBounds(p.Row, p.Col) || !field.IsEmpty(p.Row, p.Col));
        }

        /// <summary>
        /// Calculates the final exact mathematical movement and applies it to all blocks in the brick.
        /// </summary>
        private static void ApplyCalculatedMovesToPoints(
            List<Point> points,
            MoveCommand command,
            int horizontalMoveStep,
            int verticalMoveStep,
            bool applyAutoRowMove)
        {

            //Auto Move down by 1 row if it's the last command and the brick can still fall further
            //Except applying the 'D' command's drop to bottom directly.
            if (applyAutoRowMove && command != MoveCommand.Down)
            {
                verticalMoveStep += 1;
            }

            // Apply the final numbers to every point
            foreach (var p in points)
            {
                p.MoveBy(verticalMoveStep, horizontalMoveStep);
            }
        }

        /// <summary>
        /// Analyzes the command and board state to calculate exactly how far the brick should move horizontally and vertically.
        /// </summary>
        private static (MoveCommand FinalCommand, int HorizontalMoveStep, int VerticalMoveStep, bool ApplyAutoRowMove) CalculateMovementReults(
            Brick brick, GameField field, MoveCommand command, bool isLastCommand, List<Point> currentPoints)
        {
            // 1. Calculate and Validate Horizontal Move
            int horizontalMoveStep = 0;
            if (command == MoveCommand.Left) horizontalMoveStep = -1; //Left command means move left by 1 column, which is -1
            else if (command == MoveCommand.Right) horizontalMoveStep = 1; //Right command means move right by 1 column, which is +1

            if (horizontalMoveStep != 0)
            {
                //check if all points can move in the desired horizontal direction without going out of bounds or hitting non-empty cells
                bool canMoveHorizontal = currentPoints.All(p =>
                    field.IsWithinBounds(p.Row, p.Col + horizontalMoveStep) &&
                    field.IsEmpty(p.Row, p.Col + horizontalMoveStep));

                if (!canMoveHorizontal)
                {
                    // Ignore wall crashes and default to Continue so auto vertical drop still works
                    horizontalMoveStep = 0;
                    command = MoveCommand.Continue;
                }
            }

            // 2. Calculate and Validate Vertical Move ('D' command)
            int verticalMoveStep = 0;
            if (command == MoveCommand.Down)
            {
                verticalMoveStep = CalculateMaxDrop(brick, field);
            }

            // 3. Calculate and Validate Gravity
            bool applyAutoRowMove = false;
            if (isLastCommand)
            {
                //check if all points can move down by 1 row without going out of bounds or hitting non-empty cells
                applyAutoRowMove = currentPoints.All(p =>
                    field.IsWithinBounds(p.Row + verticalMoveStep + 1, p.Col + horizontalMoveStep) &&
                    field.IsEmpty(p.Row + verticalMoveStep + 1, p.Col + horizontalMoveStep));
            }

            // Pack all the calculated results into a single Tuple and return it
            return (command, horizontalMoveStep, verticalMoveStep, applyAutoRowMove);
        }
    }
}