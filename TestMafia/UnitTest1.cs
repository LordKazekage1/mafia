

using Microsoft.VisualStudio.TestTools.UnitTesting;
using MafiaGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Numerics;

namespace MafiaGame.Tests
{
    #region Mock Classes for Testing

    public class MockInputProvider : IInputProvider
    {
        private Queue<string> _inputs = new Queue<string>();

        public void AddInput(string input)
        {
            _inputs.Enqueue(input);
        }

        public void AddInputs(params string[] inputs)
        {
            foreach (var input in inputs)
            {
                AddInput(input);
            }
        }

        public string ReadLine()
        {
            if (_inputs.Count > 0)
            {
                return _inputs.Dequeue();
            }
            return "1";
        }

        public void ReadKey()
        {

        }

        public int GetIntInput(int min, int max)
        {
            if (_inputs.Count > 0)
            {
                string input = _inputs.Dequeue();
                if (int.TryParse(input, out int result) && result >= min && result <= max)
                {
                    return result;
                }
            }
            return min;
        }
    }

    public class MockOutputProvider : IOutputProvider
    {
        private List<string> _outputs = new List<string>();

        public List<string> GetOutputs()
        {
            return _outputs;
        }

        public string GetLastOutput()
        {
            return _outputs.LastOrDefault();
        }

        public void WriteLine(string message)
        {
            _outputs.Add(message);
        }

        public void Write(string message)
        {
            _outputs.Add(message);
        }

        public void Clear()
        {
            _outputs.Add("[CLEAR]");
        }
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private List<string> _logs = new List<string>();

        public List<string> GetLogs()
        {
            return _logs;
        }

        public void Log(string message)
        {
            _logs.Add(message);
        }
    }

    public class MockRandomProvider : IRandomProvider
    {
        private Queue<int> _values = new Queue<int>();

        public void SetNextValues(params int[] values)
        {
            foreach (var value in values)
            {
                _values.Enqueue(value);
            }
        }

        public int Next(int maxValue)
        {
            if (_values.Count > 0)
            {
                return _values.Dequeue() % maxValue;
            }
            return 0;
        }

        public int Next(int minValue, int maxValue)
        {
            if (_values.Count > 0)
            {
                int value = _values.Dequeue();
                return minValue + (value % (maxValue - minValue));
            }
            return minValue;
        }
    }

    #endregion

    [TestClass]
    public class GameTests
    {
        private MockInputProvider _inputProvider;
        private MockOutputProvider _outputProvider;
        private MockLoggerProvider _loggerProvider;
        private MockRandomProvider _randomProvider;

        [TestInitialize]
        public void Initialize()
        {
            _inputProvider = new MockInputProvider();
            _outputProvider = new MockOutputProvider();
            _loggerProvider = new MockLoggerProvider();
            _randomProvider = new MockRandomProvider();
        }

        [TestMethod]
        public void Game_CreateGame_CorrectNumberOfPlayers()
        {
            int expectedPlayersCount = 7;

            Game game = new Game(expectedPlayersCount, _loggerProvider, _randomProvider);

            Assert.AreEqual(expectedPlayersCount, game.GetAllPlayers().Count,
                "Кількість гравців має дорівнювати заданому значенню");
        }

  


}