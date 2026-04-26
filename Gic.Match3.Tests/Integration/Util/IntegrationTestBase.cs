using Gic.Match3.Domain.Helpers;
using Gic.Match3.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Text;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Gic.Match3.Tests.Integration.Util
{
    /// <summary>
    /// A base class for Integration Testing.
    /// Handles Dependency Injection (DI) lifecycle and Console I/O redirection.
    /// </summary>
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly ServiceProvider ServiceProvider;
        private readonly BlockingCollection<string> _inputQueue = [];
        private readonly StringBuilder _outputLog = new();
        private readonly StringWriter _testOutputWriter;
        private readonly TextWriter _originalOut;
        private readonly TextReader _originalIn;
        private int _lastReadPosition = 0;
        private bool _disposedValue;

        // Passing options through the constructor allows polymorphic test configurations.
        protected IntegrationTestBase(GameRulesOptions options)
        {
            var services = new ServiceCollection();
            services.AddServices(options);
            ServiceProvider = services.BuildServiceProvider();

            // Redirect Console.In to our Queue-based reader
            _originalIn = Console.In;
            Console.SetIn(new QueueTextReader(_inputQueue));

            // Redirect Console.Out to capture output
            _originalOut = Console.Out;
            _testOutputWriter = new StringWriter(_outputLog);
            Console.SetOut(_testOutputWriter);

            //Load Configuratin For Message Constants
            LoadGameRulesOptions(options);
        }

        // Properties to remember the board size for the current test
        public int CurrentWidth { get; private set; }
        public int CurrentHeight { get; private set; }

        /// <summary>
        /// Updates the tracked dimensions of the game board.
        /// </summary>
        public void SetDimensions(int width, int height)
        {
            CurrentWidth = width;
            CurrentHeight = height;
        }

        /// <summary>
        /// Simulate user typing a line.
        /// </summary>
        public void SendInput(string input)
        {
            _inputQueue.Add(input);
            // Give the game thread a tiny bit of time to process
            Thread.Sleep(50);
        }

        /// <summary>
        /// Gets only the NEW text printed to the console since the last call to GetNewPrint.
        /// </summary>
        public string GetNewPrint()
        {
            // Wait a bit for the console to finish flushing
            Thread.Sleep(100);
            var fullOutput = _outputLog.ToString();
            var newOutput = fullOutput[_lastReadPosition..];
            _lastReadPosition = fullOutput.Length;
            return newOutput;
        }

        /// <summary>
        /// Custom Reader that waits for items in the BlockingCollection.
        /// </summary>
        private class QueueTextReader(BlockingCollection<string> queue) : TextReader
        {
            public override string ReadLine() => queue.Take();
        }

        #region Standard Dispose Pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Console.SetOut(_originalOut);
                    Console.SetIn(_originalIn);
                    _testOutputWriter.Dispose();
                    _inputQueue.Dispose();
                    ServiceProvider.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
