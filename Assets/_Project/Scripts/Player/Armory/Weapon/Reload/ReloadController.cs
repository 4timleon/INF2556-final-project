using System;
using EditorAttributes;
using UnityEngine;
using WeaponNamespace;

public class ReloadController : MonoBehaviour
{
    [field: Title("References:", 14)]
    [field: SerializeField] public IKTargetHandler TargetHandler { get; private set; }
    [field: SerializeField] public RecoilController RecoilController { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public Transform BulletPouchTarget { get; private set; }
    [field: SerializeField] public Transform BulletPouchSocket { get; private set; }
    [Space]
    [Title("Animation:", 14)]
    [SerializeField] private string reloadAnimationName;
    [SerializeField] private string movementAnimationName;
    [field: SerializeField] public int ReloadLayer { get; private set; } = 0;
    [Space]
    [Title("Sound:", 14)]
    [SerializeField] private AudioSource reloadAudioSource;

    public int ReloadAnimationHash { get; private set; }
    public int MovementAnimationHash { get; private set; }

    private WeaponController weaponController;
    private Coroutine currentReload;

    void Awake()
    {
        ReloadAnimationHash = Animator.StringToHash(reloadAnimationName);
        MovementAnimationHash = Animator.StringToHash(movementAnimationName);
    }

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster += Event_WeaponHolster;
        GameEventSystem.OnPlayerReloadStarted += Event_ReloadStarted;
        GameEventSystem.OnPlayerAttackStarted += Event_AttackStarted;
        GameEventSystem.OnPlayerSmokeJointStarted += Event_SmokeJointStarted;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster -= Event_WeaponHolster;
        GameEventSystem.OnPlayerReloadStarted -= Event_ReloadStarted;
        GameEventSystem.OnPlayerSmokeJointStarted -= Event_SmokeJointStarted;
    }

    void Event_WeaponDraw(WeaponController weaponController) => this.weaponController = weaponController;
    void Event_SmokeJointStarted() => CancelReload();
    void Event_WeaponHolster()
    {
        CancelReload();
        weaponController = null;
    }

    void Event_AttackStarted()
    {
        if (weaponController == null || weaponController.CurrentMagazineAmmo <= 0) return;

        CancelReload();
    }

    void Event_ReloadStarted()
    {
        if (weaponController == null || weaponController.IsReloading || weaponController.IsLocked || weaponController.ReloadBase == null)
        {
            return;
        }

        if (weaponController.CurrentMagazineAmmo >= weaponController.GetStat(StatType.MagazineSize) || weaponController.CurrentWeaponAmmo <= 0)
        {
            return;
        }

        weaponController.IsReloading = true;

        currentReload = StartCoroutine(weaponController.ReloadBase.StartReload(this, weaponController, PlayReloadEjectSound));
    }

    void CancelReload()
    {
        if (weaponController == null || !weaponController.IsReloading || currentReload == null) return;

        StopCoroutine(currentReload);
        weaponController.ReloadBase.CancelReload(this, weaponController);
    }

    private void PlayReloadEjectSound(int index)
    {
        switch (index)
        {
            case 0:
                SoundManager.Instance?.PlayReloadEjectSound(reloadAudioSource, weaponController.WeaponData.TotalType);
                break;
            case 1:
                SoundManager.Instance?.PlayReloadPocketSound(reloadAudioSource);
                break;
            case 2:
                SoundManager.Instance?.PlayReloadHomeSound(reloadAudioSource, weaponController.WeaponData.TotalType);
                break;
        }
    }
}
