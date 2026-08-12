using UnityEngine;

public static class GameData
{
    // only singleplayer for now

    public static int Money { get; set; }
    public static int Wave { get; set; }

    public static bool IsPowerOn { get; set; }

    public static int PlayerHealth { get; set; }
    public static int PlayerMaxHealth { get; set; } = 100;
}