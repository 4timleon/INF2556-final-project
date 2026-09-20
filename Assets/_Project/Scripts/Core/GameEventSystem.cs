using System;
using UnityEngine;
using System.Collections.Generic;
using WeaponNamespace;

public static class GameEventSystem
{
    // -- Pause --
    public static Action<bool> OnGamePause;

    // -- Story --
    public static Action<DoorController> OnDoorOpened; // string = doorId, z.B. "Door_A1"
    public static Action<bool> OnPowerChanged;

    // -- Mystery Box --
    public static Action<Transform> OnMysteryBoxDisappear;

    // -- Perks --
    public static Action<PerkType> OnPerkAquired;

    // -- Points --
    public static Action OnMoneyUpdated;

    // -- Interact --
    public static Action<IInteractable> OnInteractionEnter;
    public static Action OnInteractionExit;

    public static Action<float> OnShowPriceText;
    public static Action OnHidePriceText;
    public static Action<bool> OnShowRepairText;

    // -- Zombies and Wave Manager --
    public static Action<Zombie> OnEnemyKilled;
    public static Action OnWaveStarted;
    public static Action OnWaveEnded;

    // -- Player Events --
    public static Action OnPlayerSmokeJointStarted;
    public static Action OnPlayerSmokeJointEnded;

    public static Action<bool> OnPlayerPreDeath;
    public static Action<float> OnPlayerDeath;

    public static Action OnPlayerInteractStarted;
    public static Action OnPlayerInteractEnded;

    public static Action<float, float> OnPlayerHealthChanged; // (current, max)

    public static Action<WeaponController> OnPlayerWeaponDraw;
    public static Action<WeaponController> OnPlayerWeaponTransform;
    public static Action OnPlayerWeaponHolster;
    public static Action<Vector2> OnPlayerWeaponScroll;
    public static Action<float, float> OnPlayerWeaponShoot; // args = recoil

    public static Action OnPlayerJump;
    public static Action OnPlayerInspect;

    public static Action OnPlayerAttackStarted;
    public static Action OnPlayerAttackEnded;

    public static Action OnPlayerAimStarted;
    public static Action OnPlayerAimEnded;

    public static Action<bool> OnPlayerInSight;

    public static Action OnPlayerSprintStarted;
    public static Action OnPlayerSprintEnded;

    public static Action OnPlayerReloadStarted;
    public static Action OnPlayerReloadTick;
    public static Action OnPlayerReloadEnded;
}
