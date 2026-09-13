using System;
using EditorAttributes;
using UnityEngine;

public class PlayerInspect : MonoBehaviour
{
    [Title("Animation:", 14)]
    [SerializeField] private Animator animator;
    [SerializeField] private string inspectAnimationName;
    [SerializeField] private string idleAnimationName;
    [SerializeField] private int animationLayer;

    private WeaponController weaponController;

    private int inspectAnimationHash;
    private int idleAnimationHash;

    private bool isSprinting;
    private bool isSwitching;

    void Start()
    {
        inspectAnimationHash = Animator.StringToHash(inspectAnimationName);
        idleAnimationHash = Animator.StringToHash(idleAnimationName);
    }

    void OnEnable()
    {
        GameEventSystem.OnPlayerInspect += Event_PlayerInspect;

        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster += Event_WeaponHolster;

        GameEventSystem.OnPlayerSprintStarted += Event_SprintStarted;
        GameEventSystem.OnPlayerSprintEnded += Event_SprintEnded;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerInspect -= Event_PlayerInspect;

        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster -= Event_WeaponHolster;

        GameEventSystem.OnPlayerSprintStarted -= Event_SprintStarted;
        GameEventSystem.OnPlayerSprintEnded -= Event_SprintEnded;
    }


    void Event_SprintStarted() => isSprinting = true;
    void Event_SprintEnded() => isSprinting = false;
    void Event_PlayerInspect() => StartInspect();
    void Event_WeaponHolster() => isSwitching = true;

    void Event_WeaponDraw(WeaponController weaponController)
    {
        this.weaponController = weaponController;
        isSwitching = false;
    }

    void Update()
    {
        if (weaponController == null || !weaponController.IsInspecting) return;

        if (isSwitching) { EndInspect(); return; }
        if (weaponController.IsReloading) { EndInspect(false); return; }
        if (weaponController.IsShooting) { EndInspect(); return; }
        if (weaponController.IsAiming) { EndInspect(false); return; }
        if (isSprinting) { EndInspect(); return; }
    }

    void StartInspect()
    {
        if (weaponController.IsInspecting || weaponController == null || isSwitching || weaponController.IsReloading || weaponController.IsShooting || weaponController.IsAiming || isSprinting) return;

        animator.CrossFadeInFixedTime(inspectAnimationHash, .5f, animationLayer);
        weaponController.IsInspecting = true;
    }

    void EndInspect(bool returnToNormal = true)
    {
        if (returnToNormal) animator.CrossFadeInFixedTime(idleAnimationHash, .25f, animationLayer);
        weaponController.IsInspecting = false;
    }
}
