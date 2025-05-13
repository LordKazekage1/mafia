using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MafiaGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            IInputProvider inputProvider = new ConsoleInputProvider();
            IOutputProvider outputProvider = new ConsoleOutputProvider();
            ILoggerProvider loggerProvider = new FileLoggerProvider("game_log.txt");

            GameController gameController = new GameController(inputProvider, outputProvider, loggerProvider);
            bool exit = false;

            while (!exit)
            {
                outputProvider.Clear();
                outputProvider.WriteLine("=== МАФІЯ ===");
                outputProvider.WriteLine("1. Почати гру");
                outputProvider.WriteLine("2. Правила");
                outputProvider.WriteLine("3. Вийти");
                outputProvider.Write("Виберіть опцію: ");

                string choice = inputProvider.ReadLine();

                switch (choice)
                {
                    case "1":
                        StartGame(gameController, inputProvider, outputProvider);
                        break;
                    case "2":
                        ShowRules(outputProvider, inputProvider);
                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        outputProvider.WriteLine("Невірний вибір. Натисніть будь-яку клавішу для продовження...");
                        inputProvider.ReadKey();
                        break;
                }
            }
        }

        static void StartGame(GameController gameController, IInputProvider inputProvider, IOutputProvider outputProvider)
        {
            outputProvider.Clear();
            outputProvider.WriteLine("=== НОВА ГРА ===");

            int playersCount = 0;
            bool validInput = false;

            while (!validInput)
            {
                outputProvider.Write("Введіть кількість гравців (від 7 до 10): ");
                string input = inputProvider.ReadLine();

                if (int.TryParse(input, out playersCount) && playersCount >= 7 && playersCount <= 10)
                {
                    validInput = true;
                }
                else
                {
                    outputProvider.WriteLine("Будь ласка, введіть число від 7 до 10.");
                }
            }

            gameController.StartNewGame(playersCount);
            gameController.PlayGame();

            outputProvider.WriteLine("\nГра завершена! Натисніть будь-яку клавішу для повернення в головне меню...");
            inputProvider.ReadKey();
        }

        static void ShowRules(IOutputProvider outputProvider, IInputProvider inputProvider)
        {
            outputProvider.Clear();
            outputProvider.WriteLine("=== ПРАВИЛА ГРИ ===");
            outputProvider.WriteLine("Мафія — це командна психологічна гра з розподілом ролей.");
            outputProvider.WriteLine("Основні правила:");
            outputProvider.WriteLine("1. Гравці діляться на дві команди: 'Мирні жителі' та 'Мафія'.");
            outputProvider.WriteLine("2. Гра складається з двох фаз: 'День' і 'Ніч'.");
            outputProvider.WriteLine("3. Під час 'Ночі':");
            outputProvider.WriteLine("   - Мафія обирає жертву");
            outputProvider.WriteLine("   - Детектив може перевірити одного гравця");
            outputProvider.WriteLine("   - Лікар може вилікувати одного гравця");
            outputProvider.WriteLine("   - Повія може відвідати одного гравця, блокуючи його дію");
            outputProvider.WriteLine("4. Під час 'Дня' всі гравці обговорюють і голосують за підозрюваного.");
            outputProvider.WriteLine("5. Мета мирних жителів - виявити і усунути всіх представників мафії.");
            outputProvider.WriteLine("6. Мета мафії - усунути всіх мирних жителів або рівностороннє голосування.");
            outputProvider.WriteLine("\nНатисніть будь-яку клавішу для повернення в головне меню...");
            inputProvider.ReadKey();
        }
    }

    #region Interfaces

    public interface IInputProvider
    {
        string ReadLine();
        void ReadKey();
        int GetIntInput(int min, int max);
    }

    public interface IOutputProvider
    {
        void WriteLine(string message);
        void Write(string message);
        void Clear();
    }

    public interface ILoggerProvider
    {
        void Log(string message);
    }

    public interface IPlayer
    {
        string Name { get; }
        bool IsAlive { get; set; }
        Role Role { get; }
        bool IsBlocked { get; set; }
        void PerformNightAction(Game game, IInputProvider inputProvider, IOutputProvider outputProvider);
        string GetInfo();
    }

    public interface IRandomProvider
    {
        int Next(int maxValue);
        int Next(int minValue, int maxValue);
    }

    #endregion

    #region Provider Implementations

    public class ConsoleInputProvider : IInputProvider
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public void ReadKey()
        {
            Console.ReadKey();
        }

        public int GetIntInput(int min, int max)
        {
            int result;
            bool valid = false;
            do
            {
                string input = Console.ReadLine();
                valid = int.TryParse(input, out result) && result >= min && result <= max;
                if (!valid)
                {
                    Console.Write($"Введіть число від {min} до {max}: ");
                }
            } while (!valid);

            return result;
        }
    }

    public class ConsoleOutputProvider : IOutputProvider
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public void Write(string message)
        {
            Console.Write(message);
        }

        public void Clear()
        {
            Console.Clear();
        }
    }

    public class FileLoggerProvider : ILoggerProvider
    {
        private string _logFilePath;

        public FileLoggerProvider(string logFilePath)
        {
            _logFilePath = logFilePath;

            try
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, false))
                {
                    writer.WriteLine($"=== Гра Мафія - Логи [{DateTime.Now}] ===");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при створенні лог-файлу: {ex.Message}");
            }
        }

        public void Log(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, true))
                {
                    writer.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при записі в лог: {ex.Message}");
            }
        }
    }

    public class DefaultRandomProvider : IRandomProvider
    {
        private Random _random = new Random();

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }

    #endregion

    #region Enums

    public enum Role
    {
        Civilian,
        Mafia,
        Detective,
        Doctor,
        Courtesan
    }

    public enum GamePhase
    {
        Day,
        Night
    }

    #endregion

    #region Player Classes

    public abstract class PlayerBase : IPlayer
    {
        public string Name { get; private set; }
        public bool IsAlive { get; set; } = true;
        public bool IsBlocked { get; set; } = false;
        public abstract Role Role { get; }

        protected PlayerBase(string name)
        {
            Name = name;
        }

        public abstract void PerformNightAction(Game game, IInputProvider inputProvider, IOutputProvider outputProvider);

        public virtual string GetInfo()
        {
            return $"{Name} ({Role})";
        }
    }

    public class Civilian : PlayerBase
    {
        public override Role Role => Role.Civilian;

        public Civilian(string name) : base(name) { }

        public override void PerformNightAction(Game game, IInputProvider inputProvider, IOutputProvider outputProvider)
        {

        }
    }

    public class Mafia : PlayerBase
    {
        public override Role Role => Role.Mafia;

        public Mafia(string name) : base(name) { }

        public override void PerformNightAction(Game game, IInputProvider inputProvider, IOutputProvider outputProvider)
        {
            if (IsBlocked)
            {
                outputProvider.WriteLine("Вас відвідала повія. Ви не можете виконати дію цієї ночі.");
                game.Logger.Log($"Мафія {Name} заблокована повією і не може виконати дію");
                return;
            }

            var availablePlayers = game.GetAlivePlayers().Where(p => p.Role != Role.Mafia).ToList();

            if (availablePlayers.Count == 0) return;

            outputProvider.WriteLine("\nВиберіть жертву:");
            for (int i = 0; i < availablePlayers.Count; i++)
            {
                outputProvider.WriteLine($"{i + 1}. {availablePlayers[i].Name}");
            }

            int choice = inputProvider.GetIntInput(1, availablePlayers.Count);
            IPlayer victim = availablePlayers[choice - 1];
            game.CurrentVictim = victim;
            game.Logger.Log($"Мафія вибрала жертву: {victim.Name}");
        }
    }

    public class Detective : PlayerBase
    {
        public override Role Role => Role.Detective;

        public Detective(string name) : base(name) { }

        public override void PerformNightAction(Game game, IInputProvider inputProvider, IOutputProvider outputProvider)
        {
            if (!IsAlive) return;

            if (IsBlocked)
            {
                outputProvider.WriteLine("Вас відвідала повія. Ви не можете виконати дію цієї ночі.");
                game.Logger.Log($"Детектив {Name} заблокований повією і не може виконати дію");
                return;
            }

            var availablePlayers = game.GetAlivePlayers().Where(p => p != this).ToList();

            if (availablePlayers.Count == 0) return;
            outputProvider.WriteLine("\nВиберіть гравця для перевірки:");
            for (int i = 0; i < availablePlayers.Count; i++)
            {
                outputProvider.WriteLine($"{i + 1}. {availablePlayers[i].Name}");
            }

            int choice = inputProvider.GetIntInput(1, availablePlayers.Count);
            IPlayer suspect = availablePlayers[choice - 1];
            bool isMafia = suspect.Role == Role.Mafia;

            outputProvider.WriteLine($"Результат перевірки: {suspect.Name} - {(isMafia ? "МАФІЯ" : "НЕ МАФІЯ")}");
            game.Logger.Log($"Детектив перевірив гравця {suspect.Name}: {(isMafia ? "Мафія" : "Не мафія")}");
        }
    }

    public class Doctor : PlayerBase
    {
        public override Role Role => Role.Doctor;
        private IPlayer _lastHealed = null;

        public Doctor(string name) : base(name) { }

        public override void PerformNightAction(Game game, IInputProvider inputProvider, IOutputProvider outputProvider)
        {
            if (!IsAlive) return;

            if (IsBlocked)
            {
                outputProvider.WriteLine("Вас відвідала повія. Ви не можете виконати дію цієї ночі.");
                game.Logger.Log($"Лікар {Name} заблокований повією і не може виконати дію");
                return;
            }

            var availablePlayers = game.GetAlivePlayers().ToList();

            if (availablePlayers.Count == 0) return;
            outputProvider.WriteLine("\nВиберіть гравця для лікування:");
            for (int i = 0; i < availablePlayers.Count; i++)
            {
                outputProvider.WriteLine($"{i + 1}. {availablePlayers[i].Name}{(availablePlayers[i] == _lastHealed ? " (лікували минулої ночі)" : "")}");
            }

            int choice = inputProvider.GetIntInput(1, availablePlayers.Count);
            IPlayer patient = availablePlayers[choice - 1];

            if (patient == _lastHealed)
            {
                outputProvider.WriteLine("Ви не можете лікувати одного і того ж гравця дві ночі поспіль. Виберіть іншого.");
                PerformNightAction(game, inputProvider, outputProvider);
                return;
            }

            game.HealedPlayer = patient;
            _lastHealed = patient;
            outputProvider.WriteLine($"Ви вилікували гравця {patient.Name}");
            game.Logger.Log($"Лікар вилікував гравця {patient.Name}");
        }


        public IPlayer GetLastHealed()
        {
            return _lastHealed;
        }
    }

    public class Courtesan : PlayerBase
    {
        public override Role Role => Role.Courtesan;
        private IPlayer _lastVisited = null;

        public Courtesan(string name) : base(name) { }

        public override void PerformNightAction(Game game, IInputProvider inputProvider, IOutputProvider outputProvider)
        {
            if (!IsAlive) return;

            if (IsBlocked)
            {
                outputProvider.WriteLine("Вас відвідала інша повія. Ви не можете виконати дію цієї ночі.");
                game.Logger.Log($"Повія {Name} заблокована і не може виконати дію");
                return;
            }

            var availablePlayers = game.GetAlivePlayers().Where(p => p != this).ToList();

            if (availablePlayers.Count == 0) return;
            outputProvider.WriteLine("\nВиберіть гравця для відвідування:");
            for (int i = 0; i < availablePlayers.Count; i++)
            {
                outputProvider.WriteLine($"{i + 1}. {availablePlayers[i].Name}{(availablePlayers[i] == _lastVisited ? " (відвідували минулої ночі)" : "")}");
            }

            int choice = inputProvider.GetIntInput(1, availablePlayers.Count);
            IPlayer target = availablePlayers[choice - 1];

            if (target == _lastVisited)
            {
                outputProvider.WriteLine("Ви не можете відвідувати одного і того ж гравця дві ночі поспіль. Виберіть іншого.");
                PerformNightAction(game, inputProvider, outputProvider);
                return;
            }

            target.IsBlocked = true;
            _lastVisited = target;
            outputProvider.WriteLine($"Ви відвідали гравця {target.Name} і заблокували його дію на цю ніч");
            game.Logger.Log($"Повія відвідала гравця {target.Name} і заблокувала його дію");
        }


        public IPlayer GetLastVisited()
        {
            return _lastVisited;
        }
    }

    #endregion

    #region Game Logic Classes

    public class Game
    {
        private List<IPlayer> _players = new List<IPlayer>();
        public int CurrentDay { get; private set; } = 1;
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Day;
        public IPlayer CurrentVictim { get; set; }
        public IPlayer HealedPlayer { get; set; }
        public ILoggerProvider Logger { get; private set; }
        private IRandomProvider _randomProvider;

        public event EventHandler<GameStateEventArgs> GameStateChanged;

        public Game(int playersCount, ILoggerProvider logger, IRandomProvider randomProvider = null)
        {
            Logger = logger;
            _randomProvider = randomProvider ?? new DefaultRandomProvider();
            InitializePlayers(playersCount);
        }


        public Game(List<IPlayer> players, ILoggerProvider logger)
        {
            _players = players;
            Logger = logger;
            CurrentDay = 1;
            CurrentPhase = GamePhase.Day;
        }

        private void InitializePlayers(int playersCount)
        {
            string[] names = {
                "Іван", "Марія", "Олександр", "Оксана", "Петро",
                "Наталія", "Андрій", "Олена", "Михайло", "Тетяна"
            };

            int mafiaCount = Math.Max(playersCount / 4, 1);

            List<Role> roles = new List<Role>();
            roles.Add(Role.Detective);
            roles.Add(Role.Doctor);
            roles.Add(Role.Courtesan);

            for (int i = 0; i < mafiaCount; i++)
                roles.Add(Role.Mafia);

            for (int i = 0; i < playersCount - mafiaCount - 3; i++)
                roles.Add(Role.Civilian);

            roles = roles.OrderBy(r => _randomProvider.Next(roles.Count)).ToList();

            for (int i = 0; i < playersCount; i++)
            {
                string name = names[i % names.Length] + (i >= names.Length ? " " + (i / names.Length + 1) : "");
                IPlayer player;

                switch (roles[i])
                {
                    case Role.Mafia:
                        player = new Mafia(name);
                        break;
                    case Role.Detective:
                        player = new Detective(name);
                        break;
                    case Role.Doctor:
                        player = new Doctor(name);
                        break;
                    case Role.Courtesan:
                        player = new Courtesan(name);
                        break;
                    default:
                        player = new Civilian(name);
                        break;
                }

                _players.Add(player);
            }

            Logger.Log("Гра створена. Розподіл ролей:");
            foreach (var player in _players)
            {
                Logger.Log($"{player.Name}: {player.Role}");
            }
        }

        public List<IPlayer> GetAllPlayers()
        {
            return _players;
        }

        public List<IPlayer> GetAlivePlayers()
        {
            return _players.Where(p => p.IsAlive).ToList();
        }

        public List<IPlayer> GetMafiaPlayers()
        {
            return _players.Where(p => p.Role == Role.Mafia && p.IsAlive).ToList();
        }

        public List<IPlayer> GetCivilianPlayers()
        {
            return _players.Where(p => p.Role != Role.Mafia && p.IsAlive).ToList();
        }

        public void SetPhase(GamePhase phase)
        {
            CurrentPhase = phase;
            Logger.Log($"{(phase == GamePhase.Day ? "День" : "Ніч")} {CurrentDay}");
            OnGameStateChanged(new GameStateEventArgs { Phase = CurrentPhase, Day = CurrentDay });
        }

        public void NextPhase()
        {
            if (CurrentPhase == GamePhase.Day)
            {
                CurrentPhase = GamePhase.Night;
                Logger.Log($"Ніч {CurrentDay}");
            }
            else
            {
                CurrentPhase = GamePhase.Day;
                CurrentDay++;
                Logger.Log($"День {CurrentDay}");

                foreach (var player in _players)
                {
                    player.IsBlocked = false;
                }
            }

            OnGameStateChanged(new GameStateEventArgs { Phase = CurrentPhase, Day = CurrentDay });
        }

        public bool IsGameOver(out Role winner)
        {
            int mafiaCount = GetMafiaPlayers().Count;
            int civilianCount = GetCivilianPlayers().Count;

            if (mafiaCount == 0)
            {
                winner = Role.Civilian;
                return true;
            }

            if (mafiaCount >= civilianCount)
            {
                winner = Role.Mafia;
                return true;
            }

            winner = Role.Civilian;
            return false;
        }

        protected virtual void OnGameStateChanged(GameStateEventArgs e)
        {
            GameStateChanged?.Invoke(this, e);
        }
    }

    public class GameController
    {
        private Game _game;
        private IInputProvider _inputProvider;
        private IOutputProvider _outputProvider;
        private ILoggerProvider _loggerProvider;
        private IRandomProvider _randomProvider;

        public GameController(IInputProvider inputProvider, IOutputProvider outputProvider, ILoggerProvider loggerProvider, IRandomProvider randomProvider = null)
        {
            _inputProvider = inputProvider;
            _outputProvider = outputProvider;
            _loggerProvider = loggerProvider;
            _randomProvider = randomProvider ?? new DefaultRandomProvider();
        }

        public void StartNewGame(int playersCount)
        {
            _game = new Game(playersCount, _loggerProvider, _randomProvider);
            _game.GameStateChanged += Game_GameStateChanged;
        }


        public void SetGame(Game game)
        {
            _game = game;
            _game.GameStateChanged += Game_GameStateChanged;
        }

        private void Game_GameStateChanged(object sender, GameStateEventArgs e)
        {
            _outputProvider.WriteLine($"\n{(e.Phase == GamePhase.Day ? "День" : "Ніч")} {e.Day}");
        }

        public void PlayGame()
        {
            _outputProvider.Clear();
            _outputProvider.WriteLine("Гра починається!");
            DisplayPlayersInfo();
            _outputProvider.WriteLine("\nДень 1 - час знайомства. Сьогодні голосування не проводиться.");
            _game.Logger.Log("День 1: Знайомство, голосування не проводилось");

            _outputProvider.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
            _inputProvider.ReadKey();

            _game.SetPhase(GamePhase.Night);
            Role winner;
            while (!_game.IsGameOver(out winner))
            {
                switch (_game.CurrentPhase)
                {
                    case GamePhase.Day:
                        HandleDayPhase();
                        break;
                    case GamePhase.Night:
                        HandleNightPhase();
                        break;
                }

                _game.NextPhase();

                _outputProvider.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
                _inputProvider.ReadKey();
            }
            _outputProvider.WriteLine($"\nГра закінчена! Перемога: {(winner == Role.Mafia ? "Мафія" : "Мирні жителі")}");
            _game.Logger.Log($"Гра закінчена! Перемога: {winner}");
        }

        private void DisplayPlayersInfo()
        {
            _outputProvider.WriteLine("\nІнформація про гравців (для тестування - в реальній грі ця інформація буде прихована):");
            var players = _game.GetAllPlayers();

            foreach (var player in players)
            {
                _outputProvider.WriteLine($"{player.Name} - {player.Role}");
            }

            _outputProvider.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
            _inputProvider.ReadKey();
            _outputProvider.Clear();
        }

        private void HandleDayPhase()
        {
            var alivePlayers = _game.GetAlivePlayers();

            if (_game.CurrentVictim != null)
            {
                var victim = _game.CurrentVictim;

                if (_game.HealedPlayer == victim)
                {
                    _outputProvider.WriteLine("\nЛікар успішно врятував жертву цієї ночі!");
                    _game.Logger.Log($"Лікар врятував гравця {victim.Name} від смерті");
                }
                else
                {
                    victim.IsAlive = false;
                    _outputProvider.WriteLine($"\nЗа ніч був вбитий гравець: {victim.Name}");
                    _game.Logger.Log($"Гравець {victim.Name} був вбитий вночі");
                }

                _game.CurrentVictim = null;
                _game.HealedPlayer = null;
            }
            else
            {
                _outputProvider.WriteLine("\nЦієї ночі ніхто не постраждав.");
            }

            Role winner;
            if (_game.IsGameOver(out winner)) return;
            _outputProvider.WriteLine("\nЖиві гравці:");
            alivePlayers = _game.GetAlivePlayers();
            for (int i = 0; i < alivePlayers.Count; i++)
            {
                _outputProvider.WriteLine($"{i + 1}. {alivePlayers[i].Name}");
            }

            _outputProvider.WriteLine("\nЧас голосування!");

            var voters = _game.GetAlivePlayers();
            Dictionary<IPlayer, int> votes = new Dictionary<IPlayer, int>();

            foreach (var player in voters)
            {
                votes[player] = 0;
            }

            foreach (var voter in voters)
            {
                _outputProvider.WriteLine($"\nГравець {voter.Name} голосує:");
                for (int i = 0; i < voters.Count; i++)
                {
                    if (voters[i] != voter)
                    {
                        _outputProvider.WriteLine($"{i + 1}. {voters[i].Name}");
                    }
                }

                int choice;
                do
                {
                    _outputProvider.Write("Введіть номер підозрюваного: ");
                    choice = _inputProvider.GetIntInput(1, voters.Count);
                } while (voters[choice - 1] == voter);

                IPlayer suspect = voters[choice - 1];
                votes[suspect]++;

                _outputProvider.WriteLine($"Гравець {voter.Name} проголосував проти {suspect.Name}");
                _game.Logger.Log($"Гравець {voter.Name} проголосував проти {suspect.Name}");
            }

            _outputProvider.WriteLine("\nРезультати голосування:");
            foreach (var vote in votes)
            {
                _outputProvider.WriteLine($"{vote.Key.Name}: {vote.Value} голосів");
            }

            var eliminated = votes.OrderByDescending(v => v.Value).First().Key;
            eliminated.IsAlive = false;

            _outputProvider.WriteLine($"\nЗа результатами голосування, гравець {eliminated.Name} ({eliminated.Role}) покидає гру");
            _game.Logger.Log($"Гравець {eliminated.Name} ({eliminated.Role}) був виключений за результатами голосування");
        }

        private void HandleNightPhase()
        {
            _outputProvider.Clear();
            _outputProvider.WriteLine("\nНіч. Місто засинає...");
            var courtesanPlayers = _game.GetAlivePlayers().Where(p => p.Role == Role.Courtesan).ToList();
            if (courtesanPlayers.Count > 0)
            {
                _outputProvider.WriteLine("\nПрокидається повія...");
                _outputProvider.WriteLine("(В реальній грі тільки гравець з роллю Повії повинен дивитись на екран)");
                _outputProvider.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                _inputProvider.ReadKey();

                courtesanPlayers[0].PerformNightAction(_game, _inputProvider, _outputProvider);

                _outputProvider.WriteLine("Повія зробила свій вибір і засинає.");
                _outputProvider.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                _inputProvider.ReadKey();
                _outputProvider.Clear();
            }
            var mafiaPlayers = _game.GetMafiaPlayers();
            if (mafiaPlayers.Count > 0)
            {
                _outputProvider.WriteLine("\nПрокидається мафія...");
                _outputProvider.WriteLine("(В реальній грі тільки гравці з роллю Мафії повинні дивитись на екран)");
                _outputProvider.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                _inputProvider.ReadKey();

                foreach (var mafia in mafiaPlayers)
                {
                    mafia.PerformNightAction(_game, _inputProvider, _outputProvider);
                }

                _outputProvider.WriteLine("Мафія зробила свій вибір і засинає.");
                _outputProvider.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                _inputProvider.ReadKey();
                _outputProvider.Clear();
            }

            var doctorPlayers = _game.GetAlivePlayers().Where(p => p.Role == Role.Doctor).ToList();
            if (doctorPlayers.Count > 0)
            {
                _outputProvider.WriteLine("\nПрокидається лікар...");
                _outputProvider.WriteLine("(В реальній грі тільки гравець з роллю Лікаря повинен дивитись на екран)");
                _outputProvider.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                _inputProvider.ReadKey();

                doctorPlayers[0].PerformNightAction(_game, _inputProvider, _outputProvider);

                _outputProvider.WriteLine("Лікар зробив свій вибір і засинає.");
                _outputProvider.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                _inputProvider.ReadKey();
                _outputProvider.Clear();
            }

            var detectivePlayers = _game.GetAlivePlayers().Where(p => p.Role == Role.Detective).ToList();
            if (detectivePlayers.Count > 0)
            {
                _outputProvider.WriteLine("\nПрокидається детектив...");
                _outputProvider.WriteLine("(В реальній грі тільки гравець з роллю Детектива повинен дивитись на екран)");
                _outputProvider.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                _inputProvider.ReadKey();

                detectivePlayers[0].PerformNightAction(_game, _inputProvider, _outputProvider);

                _outputProvider.WriteLine("Детектив зробив свій вибір і засинає.");
                _outputProvider.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                _inputProvider.ReadKey();
                _outputProvider.Clear();
            }



            _outputProvider.WriteLine("Ніч закінчується. Місто прокидається...");
        }
    }

    public class GameStateEventArgs : EventArgs
    {
        public GamePhase Phase { get; set; }
        public int Day { get; set; }
    }

    #endregion
}