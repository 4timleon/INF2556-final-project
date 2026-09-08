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
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster -= Event_WeaponHolster;
        GameEventSystem.OnPlayerReloadStarted -= Event_ReloadStarted;
    }

    void Event_WeaponDraw(WeaponController weaponController) => this.weaponController = weaponController;
    void Event_WeaponHolster()
    {
        CancelReload();
        weaponController = null;
    }

    void Event_AttackStarted()
    {
        if (weaponController == null || weaponController.CurrentAmmo <= 0) return;

        CancelReload();
    }

    void Event_ReloadStarted()
    {
        if (weaponController == null || weaponController.IsReloading || weaponController.ReloadBase == null) return;

        if (weaponController.CurrentAmmo >= weaponController.GetStat(StatType.MagazineSize))
        {
            return;
        }

        weaponController.IsReloading = true;

        currentReload = StartCoroutine(weaponController.ReloadBase.StartReload(this, weaponController));
    }

    void CancelReload()
    {
        if (weaponController == null || !weaponController.IsReloading || currentReload == null) return;

        StopCoroutine(currentReload);
        weaponController.ReloadBase.CancelReload(this, weaponController);
    }
}
