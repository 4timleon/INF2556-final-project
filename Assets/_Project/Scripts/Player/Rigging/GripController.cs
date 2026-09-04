using System;
using System.Collections.Generic;
using UnityEngine;
using WeaponNamespace;

public class GripController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private int layer;

    private Dictionary<GripType, int> animationHashes;

    private WeaponController weaponController;

    void Start()
    {
        animationHashes = new();
    }

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponTransform += Event_WeaponTransform;
        GameEventSystem.OnPlayerReloadStarted += Event_ReloadStarted;
        GameEventSystem.OnPlayerReloadEnded += Event_ReloadEnded;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponTransform -= Event_WeaponTransform;
        GameEventSystem.OnPlayerReloadStarted -= Event_ReloadStarted;
        GameEventSystem.OnPlayerReloadEnded -= Event_ReloadEnded;
    }

    void Event_ReloadStarted()
    {
        if (weaponController.CurrentAmmo >= weaponController.GetStat(StatType.MagazineSize)) return;

        if (((BodyModule)weaponController.GetModule(ModuleType.Body)).HasMagazine)
        {
            ChangeGrip(weaponController.GetModule(ModuleType.Magazine).GripType, 0.12f);
        }
        else
        {
            ChangeGrip(GripType.Bullet, 0.12f);
        }
    }

    void Event_ReloadEnded() => ChangeGrip(weaponController.LeftHandGripType, .1f);
    void Event_WeaponTransform(WeaponController weaponController) => ChangeGrip(weaponController.LeftHandGripType, .1f);
    void Event_WeaponDraw(WeaponController weaponController) => this.weaponController = weaponController;

    void ChangeGrip(GripType gripType, float time)
    {
        if (!animationHashes.ContainsKey(gripType))
        {
            animationHashes.Add(gripType, Animator.StringToHash(gripType.ToString()));
        }

        animator.CrossFadeInFixedTime(animationHashes[gripType], time, layer);
    }
}
