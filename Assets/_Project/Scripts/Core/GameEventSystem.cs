using System;
using UnityEngine;
using System.Collections.Generic;

public static class GameEventSystem
{
    // event examples
    public static Action OnEnemyKilled;
    public static Action OnPlayerDeath;
    public static Action OnWaveStarted;
    public static Action OnWaveEnded;

    // Story
    public static Action<string> OnDoorOpened; // string = doorId, z.B. "Door_A1"
    public static Action OnPowerTurnedOn;

    // Mystery Box
    public static Action<WeaponType, List<ModuleType>> OnWeaponWon;

    public static Action<int, int> OnPlayerHealthChanged; // (current, max)
}