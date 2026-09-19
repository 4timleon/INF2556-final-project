using UnityEngine;

public static class GameData
{
    // only singleplayer for now

    private static int _money;

    public static int Money
    {
        get => _money;
        set
        {
            _money = value;
            GameEventSystem.OnMoneyUpdated?.Invoke();
        }
    }

    public static int Wave { get; set; }

    public static bool IsPowerOn { get; set; }

    public static int PlayerHealth { get; set; }
    public static int PlayerMaxHealth { get; set; } = 100;
}