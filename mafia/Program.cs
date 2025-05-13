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