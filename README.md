# GIC Take-Home Assessment : Match-3 Console Game
> **Candidate:** Liu Jingtai | **Language:** C#

A robust and highly extensible Match-3 console application built with C# and .NET. Designed with Clean Architecture principles, this project not only simulates the dynamic grid-based puzzle mechanics—moving, dropping, and clearing bricks—but also demonstrates production-level engineering practices, including robust 2D pattern recognition, strict state validation, and a comprehensive TDD approach.

## Table of Contents
1. [Environment & Prerequisites](#environment--prerequisites)
2. [Running Instructions](#running-instructions)
3. [Configuration & Extensibility](#configuration--extensibility-gamerulesoptions)
4. [Project Structure & Architecture](#project-structure--architecture)
5. [Testing & Quality Assurance](#testing--quality-assurance)

---

## Environment & Prerequisites

This application is built using **.NET (Core)**. No external problem-solving libraries were used; all core algorithmic logic is natively implemented.

- **Required SDK:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- **Testing Framework:** xUnit & FluentAssertions
- **Solution Loading:**
    - **`Gic.Match3.sln`**: Standard UTF-8 solution format compatible with Visual Studio 2022 or earlier.
    - **`Gic.Match3.slnx`**: Modern XML-based solution format for Visual Studio 2026 or newer.
---

## Running Instructions

**Note:** In compliance with security requirements, no compiled executables (`.exe`, `.dll`, `.msi`) are included. Please build and run the application via the .NET CLI or Visual Studio.

### 1. Run the Game
Navigate to the root directory containing the solution file, or go directly into the console application folder:

```bash
# Navigate to the main application project
cd src/Gic.Match3.ConsoleUI
```

**Adjust the Configuration (Optional):** Before running the game, you can easily tweak the game rules or test the extensibility. Simply open `appsettings.json` under `Gic.Match3.ConsoleUI` and update the game rules. 

*(Please refer to the Configuration & Extensibility section below for detailed options).*

**Build and run the project:**
```bash
dotnet run
```
*Follow the on-screen prompts to input board dimensions, brick sequences, and directional commands (L, R, D).*

### 2. Run the Test Suite
To execute the comprehensive Unit and Integration tests:

```bash
# Navigate to the test project
cd tests/Gic.Match3.Tests

# Run all tests
dotnet test
```
*Note: The Integration Tests involve Console I/O redirection. The test assembly is configured to disable parallelization (`[assembly: CollectionBehavior(DisableTestParallelization = true)]`) to prevent race conditions over global console resources.*

---

## Configuration & Extensibility (`GameRulesOptions`)

To demonstrate scalability and maintainability, the application's rules are completely decoupled from the hardcoded logic. The system uses a centralized `GameRulesOptions` class (which can be seamlessly mapped to an `appsettings.json` file in a production environment via Microsoft.Extensions.Options).

This design allows to tweak the game balance without recompiling the codebase.

**Example `appsettings.json` configuration:**
```json
{
  "GameRules": {
    "EmptySymbol": ".",
    "AllowedSymbols": ["~", "^", "*", "@"],
    "MaxBricks": 5,
    "BrickSize": 3,
    "FirstReadCommands": 2,
    "EnableAutoDropAfterMatch": false
  }
}
```

**Available Configurations:**
* **`BrickSize` (int):** Defines the length of falling bricks (Default: `3`).
* **`MaxBricks` (int):** The maximum number of bricks provided per game session (Default: `5`).
* **`AllowedSymbols` (char[]):** The pool of valid characters generated inside the bricks (e.g., `~`, `^`, `*`, `@`).
* **`EmptySymbol` (char):** The character representing an empty space on the grid (Default: `.`).
* **`FirstReadCommands` (int):** Limits the number of valid directional commands processed per frame (Default: 2).
* **`EnableAutoDropAfterMatch` (bool) - *FEATURE TOGGLE*:**
    * `false` (Default): Gravity is disabled after matches. Suspended blocks hover exactly where they are, strictly complying with the assessment's example outputs.
    * `true` : Activates recursive gravity and multi-step chain reactions (Combos).

---

## Project Structure & Architecture

The solution is divided into distinct projects inspired by Clean Architecture principles, ensuring strict separation of concerns, high testability, and UI-agnostic business logic:

### 1. `Gic.Match3.Domain` (Core Business Logic)
- **Purpose:** The heart of the application. It contains all pure domain logic, independent of any UI or presentation layer. If the game needs to be ported to a Web API or Unity game engine tomorrow, this project can be plugged in without a single line of code change.
- **Key Components:**
  - `Models/`: Data structures defining the core entities like `GameField`, `Brick`, and configuration options.
  - `Services/`: The core algorithmic engines (`MatchService` for deferred execution elimination, `EngineService` for game loop and physics).
  - `Validators/`: Centralized input validation logic to ensure robust error handling.
  - `Consts/` *(`Constants`)*: Centralized string management for prompts, status updates, and errors. This decouples hardcoded text from the logic.
  - `Enums/`: Strongly-typed enumerations (e.g., movement directions) to ensure compile-time safety.
  - `Helpers/`: Utility classes and extension methods designed for specific domain operations without bloating the core models.

### 2. `Gic.Match3.ConsoleUI` (Presentation & Entry Point)
- **Purpose:** The Console UI layer and the Dependency Injection (DI) orchestrator.
- **Key Components:**
  - `Program.cs`: The entry point. Handles the DI container (`ServiceCollection`), maps the global configurations, and features a **Global Exception Handler** for defensive programming (preventing abrupt crashes).
  - Acts merely as a host to inject dependencies into the `IGameRunner` interface.

---

## Testing & Quality Assurance

**Purpose:** Ensures absolute reliability of the system through rigorous automated testing, strongly adhering to Test-Driven Development (TDD) philosophies. 

### Unit Tests
Isolated testing of models, helper methods, validators, and core services.

### Interactive Integration Tests
Features an advanced `IntegrationTestBase` that hijacks `Console.In` and `Console.Out`. It uses a custom `GridBuilder` to assert frame-by-frame visual accuracy, mimicking a real human player interactively feeding inputs and verifying outputs.

**Integration Test Scenarios:**
To demonstrate the robustness of the engine, the integration test suite covers a spectrum of highly complex edge cases:
1. **Standard Game Flow (`StandardGameFlowTests`):** Verifies the core assignment logic, ensuring strict compliance with the baseline rules (e.g., hovering blocks with no gravity applied after a match).
2. **Boundary & Validation (`BoundaryAndValidationTests`):** Aggressively tests edge cases, including invalid user inputs, wall collisions, and ignored out-of-bounds commands.
3. **Complex Intersections (`ComplexEliminationTests`):** Sets up an "Inverted-T" valley to prove that the deferred execution algorithm can perfectly clear intersecting vertical and horizontal matches simultaneously, including scenarios where the final brick self-destructs.
4. **Chain Combos (`AdvancedComboIntegrationTests`):** Toggles `EnableAutoDropAfterMatch` to `true`, demonstrating the engine's extensibility by validating recursive gravity and multi-step chain reactions.
5. **Game Over Conditions (`GameEndsBlockedTests`):** Validates precise game termination logic by proving the game ends immediately if a newly spawned vertical brick is physically blocked by existing structures.
