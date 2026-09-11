using System;
using UnityEngine;
using System.Collections.Generic;
using WeaponNamespace;

public static class GameEventSystem
{
    // -- Pause --
    public static Action<bool> OnGamePause;

    // -- Story --
    public static Action<string> OnDoorOpened; // string = doorId, z.B. "Door_A1"
    public static Action OnPowerTurnedOn;

    // -- Mystery Box --
    public static Action<WeaponType, List<ModuleType>> OnWeaponWon;

    // -- Perks --
    public static Action<PerkType> OnPerkAquired;

    // -- Points --
    public static Action<int> OnPointsAdded;

    // -- Zombies and Wave Manager --
    public static Action<Zombie> OnEnemyKilled;
    public static Action OnWaveStarted;
    public static Action OnWaveEnded;

    // -- Player Events --
    public static Action<bool> OnPlayerPreDeath;
    public static Action<float> OnPlayerDeath;

    public static Action<float, float> OnPlayerHealthChanged; // (current, max)

    public static Action<WeaponController> OnPlayerWeaponDraw;
    public static Action<WeaponController> OnPlayerWeaponTransform;
    public static Action OnPlayerWeaponHolster;
    public static Action<Vector2> OnPlayerWeaponScroll;
    public static Action<float, float> OnPlayerWeaponRecoil;

    public static Action OnPlayerJump;

    public static Action OnPlayerAttackStarted;
    public static Action OnPlayerAttackEnded;

    public static Action OnPlayerSprintStarted;
    public static Action OnPlayerSprintEnded;

    public static Action OnPlayerReloadStarted;
    public static Action OnPlayerReloadEnded;
}
