using System;
using UnityEngine;
using UnityEngine.InputSystem;
using WeaponNamespace;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private RecoilController recoilController;

    private WeaponController weaponController;

    private float roundsPerSecond;
    private float recoil;

    private bool isPerformingAttack;
    private float nextAttackTimestamp;

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster += Event_WeaponHolster;
        GameEventSystem.OnPlayerAttackStarted += Event_AttackStarted;
        GameEventSystem.OnPlayerAttackEnded += Event_AttackEnded;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster -= Event_WeaponHolster;
        GameEventSystem.OnPlayerAttackStarted -= Event_AttackStarted;
        GameEventSystem.OnPlayerAttackEnded -= Event_AttackEnded;
    }

    void Event_WeaponDraw(WeaponController weaponController)
    {
        this.weaponController = weaponController;
        roundsPerSecond = weaponController.WeaponData.Stats[StatType.Firerate];
        recoil = 1f - (weaponController.WeaponData.Stats[StatType.Handling] / 100f);
    }

    void Event_WeaponHolster() => weaponController = null;

    void Event_AttackStarted() => isPerformingAttack = true;
    void Event_AttackEnded() => isPerformingAttack = false;

    void Update()
    {
        if (!isPerformingAttack || weaponController == null)
        {
            return;
        }

        if (weaponController.IsReloading)
        {
            isPerformingAttack = false;
            return;
        }

        if (Time.time >= nextAttackTimestamp)
        {
            float interval = 1f / roundsPerSecond;

            if (Time.time > nextAttackTimestamp + interval)
            {
                nextAttackTimestamp = Time.time;
            }

            nextAttackTimestamp += interval;

            Shoot();
        }
    }

    void Shoot()
    {
        if (weaponController.CurrentAmmo <= 0)
        {
            GameEventSystem.OnPlayerReloadStarted?.Invoke();
            return;
        }

        recoilController.ApplyRecoil(recoil, recoil);
        weaponController.CurrentAmmo--;
    }
}
