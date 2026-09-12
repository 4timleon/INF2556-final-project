using System;
using EditorAttributes;
using UnityEngine;

public class PlayerADS : MonoBehaviour
{
    [Title("Camera & Rig:", 14)]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private IKDisplacementHandler ikDisplacementHandler;
    [Space]
    [Title("Animation:", 14)]
    [SerializeField] private Animator animator;
    [SerializeField] private string aimAnimationName;
    [SerializeField] private string idleAnimationName;
    [SerializeField] private int animationLayer;
    [Space]
    [Title("Settings:", 14)]
    [SerializeField] private float idleFOV = 75f;
    [SerializeField] private float adsFOV = 50f;
    [SerializeField] private float adsSpeed = 0.25f;

    private LTDescr fovTween;

    private bool isAiming;
    private bool isSprinting;

    private int aimAnimationHash;
    private int idleAnimationHash;

    private WeaponController weaponController;

    void Start()
    {
        aimAnimationHash = Animator.StringToHash(aimAnimationName);
        idleAnimationHash = Animator.StringToHash(idleAnimationName);
    }

    void OnEnable()
    {
        GameEventSystem.OnPlayerAimStarted += Event_AimStarted;
        GameEventSystem.OnPlayerAimEnded += Event_AimEnded;

        GameEventSystem.OnPlayerReloadStarted += Event_ReloadStarted;

        GameEventSystem.OnPlayerSprintStarted += Event_SprintStarted;
        GameEventSystem.OnPlayerSprintEnded += Event_SprintEnded;

        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster += Event_WeaponHolster;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerAimStarted -= Event_AimStarted;
        GameEventSystem.OnPlayerAimEnded -= Event_AimEnded;

        GameEventSystem.OnPlayerReloadStarted -= Event_ReloadStarted;

        GameEventSystem.OnPlayerSprintStarted -= Event_SprintStarted;
        GameEventSystem.OnPlayerSprintEnded -= Event_SprintEnded;

        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster -= Event_WeaponHolster;
    }

    void Event_WeaponDraw(WeaponController weaponController) => this.weaponController = weaponController;
    void Event_SprintEnded() => isSprinting = false;
    void Event_AimStarted() => StartADS();
    void Event_AimEnded() => EndADS();

    private void Event_ReloadStarted()
    {
        if (isAiming) EndADS();
    }

    void Event_WeaponHolster()
    {
        weaponController = null;

        if (isAiming) EndADS();
    }

    void Event_SprintStarted()
    {
        isSprinting = true;

        if (isAiming) EndADS();
    }

    void StartADS()
    {
        if (weaponController == null || weaponController.IsReloading || isSprinting) return;

        isAiming = true;

        TweenFOV(adsFOV, () =>
        {
            GameEventSystem.OnPlayerInSight?.Invoke(true);
        });
        animator.CrossFadeInFixedTime(aimAnimationHash, adsSpeed, animationLayer);
        ikDisplacementHandler.StartADS(adsSpeed);
    }

    void EndADS()
    {
        if (isAiming)
        {
            ikDisplacementHandler.EndADS(adsSpeed);
            TweenFOV(idleFOV);
            animator.CrossFadeInFixedTime(idleAnimationHash, adsSpeed, animationLayer);
            GameEventSystem.OnPlayerInSight?.Invoke(false);
        }

        isAiming = false;
    }

    void TweenFOV(float to, Action onComplete = null)
    {
        LeanTween.cancel(playerCamera.gameObject);
        fovTween = LeanTween.value(playerCamera.gameObject, playerCamera.fieldOfView, to, adsSpeed).setOnUpdate((value) =>
        {
            playerCamera.fieldOfView = value;
        }).setOnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
}
