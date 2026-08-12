using System;
using UnityEngine;

public static class GameEventSystem {
    // event examples

    public static Action<Zombie> OnEnemyKilled;

    public static Action OnPlayerDeath;

    public static Action OnWaveStarted;
    public static Action OnWaveEnded;
}
