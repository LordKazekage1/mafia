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

        [TestMethod]
        public void Game_CreateGame_ContainsMafia()
        {
            Game game = new Game(7, _loggerProvider, _randomProvider);

            Assert.IsTrue(game.GetAllPlayers().Any(p => p.Role == Role.Mafia),
                "У грі має бути принаймні один гравець з роллю Мафія");
        }

        [TestMethod]
        public void Game_CreateGame_ContainsAllRoles()
        {
            Game game = new Game(8, _loggerProvider, _randomProvider);
            var players = game.GetAllPlayers();

            Assert.IsTrue(players.Any(p => p.Role == Role.Detective),
                "У грі має бути гравець з роллю Детектив");
            Assert.IsTrue(players.Any(p => p.Role == Role.Doctor),
                "У грі має бути гравець з роллю Лікар");
            Assert.IsTrue(players.Any(p => p.Role == Role.Courtesan),
                "У грі має бути гравець з роллю Повія");
            Assert.IsTrue(players.Any(p => p.Role == Role.Civilian),
                "У грі має бути принаймні один гравець з роллю Мирний житель");
        }

        [TestMethod]
        public void Game_NextPhase_PhasesAlternateCorrectly()
        {
            Game game = new Game(7, _loggerProvider, _randomProvider);
            GamePhase initialPhase = game.CurrentPhase;

            game.NextPhase();
            GamePhase secondPhase = game.CurrentPhase;
            game.NextPhase();
            GamePhase thirdPhase = game.CurrentPhase;

            Assert.AreEqual(GamePhase.Day, initialPhase, "Початкова фаза гри має бути День");
            Assert.AreEqual(GamePhase.Night, secondPhase, "Після дня має бути ніч");
            Assert.AreEqual(GamePhase.Day, thirdPhase, "Після ночі має бути день");
            Assert.AreEqual(2, game.CurrentDay, "Після повного циклу (день-ніч) номер дня має збільшитись");
        }

        [TestMethod]
        public void Game_IsBlocked_ResetAfterNight()
        {
            Game game = new Game(7, _loggerProvider, _randomProvider);
            var player = game.GetAllPlayers().First();
            player.IsBlocked = true;

            game.SetPhase(GamePhase.Night);
            game.NextPhase();

            Assert.IsFalse(player.IsBlocked, "Блокування гравця має бути скинуте після ночі");
        }

        [TestMethod]
        public void Game_MafiaWins_WhenEqualCount()
        {
            Game game = new Game(7, _loggerProvider, _randomProvider);

            var civiliansToKill = game.GetCivilianPlayers().Skip(1).ToList();
            foreach (var civilian in civiliansToKill)
            {
                civilian.IsAlive = false;
            }

            Role winner;
            bool isGameOver = game.IsGameOver(out winner);

            Assert.IsTrue(isGameOver, "Гра має закінчитися, коли мафії та мирних жителів однакова кількість");
            Assert.AreEqual(Role.Mafia, winner, "Переможцем має бути Мафія");
        }

        [TestMethod]
        public void Game_CiviliansWin_WhenAllMafiaDead()
        {
            Game game = new Game(7, _loggerProvider, _randomProvider);

            var mafiaPlayers = game.GetAllPlayers().Where(p => p.Role == Role.Mafia).ToList();
            foreach (var mafia in mafiaPlayers)
            {
                mafia.IsAlive = false;
            }

            Role winner;
            bool isGameOver = game.IsGameOver(out winner);

            Assert.IsTrue(isGameOver, "Гра має закінчитися, коли всі мафіозі мертві");
            Assert.AreEqual(Role.Civilian, winner, "Переможцем мають бути Мирні жителі");
        }
    }

    [TestClass]
    public class PlayerTests
    {
        private MockInputProvider _inputProvider;
        private MockOutputProvider _outputProvider;
        private MockLoggerProvider _loggerProvider;
        private Game _game;

        [TestInitialize]
        public void Initialize()
        {
            _inputProvider = new MockInputProvider();
            _outputProvider = new MockOutputProvider();
            _loggerProvider = new MockLoggerProvider();

            List<IPlayer> players = new List<IPlayer>
            {
                new Civilian("Гравець1"),
                new Civilian("Гравець2"),
                new Mafia("Мафія"),
                new Doctor("Лікар"),
                new Detective("Детектив"),
                new Courtesan("Повія")
            };

            _game = new Game(players, _loggerProvider);
        }



        [TestMethod]
        public void Detective_CorrectlyIdentifiesMafia()
        {
            var detective = _game.GetAllPlayers().First(p => p.Role == Role.Detective) as Detective;
            var mafia = _game.GetAllPlayers().First(p => p.Role == Role.Mafia);

            int mafiaIndex = _game.GetAlivePlayers().IndexOf(mafia) + 1;
            _inputProvider.AddInput(mafiaIndex.ToString());

            detective.PerformNightAction(_game, _inputProvider, _outputProvider);

            string output = _outputProvider.GetOutputs().Last(o => o.Contains("Результат перевірки"));
            Assert.IsTrue(output.Contains("МАФІЯ"), "Детектив має правильно ідентифікувати мафію");
        }

        [TestMethod]
        public void Courtesan_BlocksPlayerAction()
        {
            var courtesan = _game.GetAllPlayers().First(p => p.Role == Role.Courtesan) as Courtesan;
            var target = _game.GetAllPlayers().First(p => p.Role == Role.Mafia);

            int targetIndex = _game.GetAlivePlayers().IndexOf(target) + 1;
            _inputProvider.AddInput(targetIndex.ToString());

            courtesan.PerformNightAction(_game, _inputProvider, _outputProvider);

            Assert.IsTrue(target.IsBlocked, "Гравець має бути заблокований після відвідування повією");
        }



        [TestMethod]
        public void Mafia_SelectsVictim()
        {
            var mafia = _game.GetAllPlayers().First(p => p.Role == Role.Mafia) as Mafia;
            var victim = _game.GetAllPlayers().First(p => p.Role == Role.Civilian);

            int victimIndex = _game.GetAlivePlayers().Where(p => p.Role != Role.Mafia).ToList().IndexOf(victim) + 1;
            _inputProvider.AddInput(victimIndex.ToString());


            mafia.PerformNightAction(_game, _inputProvider, _outputProvider);


            Assert.AreEqual(victim, _game.CurrentVictim, "Мафія має вибрати правильну жертву");
        }

        [TestMethod]
        public void Mafia_CannotAct_WhenBlocked()
        {

            var mafia = _game.GetAllPlayers().First(p => p.Role == Role.Mafia) as Mafia;
            mafia.IsBlocked = true;

            mafia.PerformNightAction(_game, _inputProvider, _outputProvider);

            Assert.IsNull(_game.CurrentVictim, "Заблокована мафія не може вибрати жертву");
            Assert.IsTrue(_outputProvider.GetOutputs().Any(o => o.Contains("Вас відвідала повія")),
                "Має бути повідомлення про блокування");
        }
    }

    [TestClass]
    public class GameControllerTests
    {
        private MockInputProvider _inputProvider;
        private MockOutputProvider _outputProvider;
        private MockLoggerProvider _loggerProvider;
        private MockRandomProvider _randomProvider;
        private GameController _gameController;

        [TestInitialize]
        public void Initialize()
        {
            _inputProvider = new MockInputProvider();
            _outputProvider = new MockOutputProvider();
            _loggerProvider = new MockLoggerProvider();
            _randomProvider = new MockRandomProvider();

            _gameController = new GameController(_inputProvider, _outputProvider, _loggerProvider, _randomProvider);
        }


    }
}