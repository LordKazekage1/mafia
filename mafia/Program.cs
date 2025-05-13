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