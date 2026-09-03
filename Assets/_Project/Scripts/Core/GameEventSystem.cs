using System;
using UnityEngine;

public static class GameEventSystem
{
    // event examples

    public static Action OnEnemyKilled;


    public static Action OnWaveStarted;
    public static Action OnWaveEnded;

    // -- Player Events --

    public static Action OnPlayerDeath;

    public static Action<WeaponController> OnPlayerWeaponDraw;
    public static Action<WeaponController> OnPlayerWeaponTransform;
    public static Action OnPlayerWeaponHolster;
    public static Action<Vector2> OnPlayerWeaponScroll;

    public static Action OnPlayerJump;

    public static Action OnPlayerAttackStarted;
    public static Action OnPlayerAttackEnded;

    public static Action OnPlayerSprintStarted;
    public static Action OnPlayerSprintEnded;

    public static Action OnPlayerReloadStarted;
    public static Action OnPlayerReloadEnded;
}
