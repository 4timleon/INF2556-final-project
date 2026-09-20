using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using WeaponNamespace;

public class PlayerADS : MonoBehaviour
{
    [Title("Camera & Rig:", 14)]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera weaponCamera;
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
    private LTDescr fadeTween;

    private bool isSprinting;
    private bool isSwitching;

    private int aimAnimationHash;
    private int idleAnimationHash;

    private WeaponController weaponController;
    private List<GameObject> stockParts;
    private List<Renderer> stockRenderers;

    private static readonly int FadePropertyId = Shader.PropertyToID("_Fade");
    private MaterialPropertyBlock propertyBlock;
    private float currentFade = 1f;

    void Start()
    {
        aimAnimationHash = Animator.StringToHash(aimAnimationName);
        idleAnimationHash = Animator.StringToHash(idleAnimationName);
        stockParts = new();
        stockRenderers = new();
        propertyBlock = new();
    }

    void OnEnable()
    {
        GameEventSystem.OnPlayerAimStarted += Event_AimStarted;
        GameEventSystem.OnPlayerAimEnded += Event_AimEnded;

        GameEventSystem.OnPlayerSprintStarted += Event_SprintStarted;
        GameEventSystem.OnPlayerSprintEnded += Event_SprintEnded;

        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster += Event_WeaponHolster;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerAimStarted -= Event_AimStarted;
        GameEventSystem.OnPlayerAimEnded -= Event_AimEnded;

        GameEventSystem.OnPlayerSprintStarted -= Event_SprintStarted;
        GameEventSystem.OnPlayerSprintEnded -= Event_SprintEnded;

        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster -= Event_WeaponHolster;
    }

    void Event_SprintStarted() => isSprinting = true;
    void Event_SprintEnded() => isSprinting = false;

    void Event_AimStarted() => StartADS();
    void Event_AimEnded() => EndADS();

    void Event_WeaponHolster() => isSwitching = true;

    void Event_WeaponDraw(WeaponController weaponController)
    {
        this.weaponController = weaponController;
        isSwitching = false;

        stockParts.Clear();
        stockRenderers.Clear();

        Transform stock = weaponController.GetModule(ModuleType.Stock)?.transform;

        if (stock == null) return;

        stockParts.Add(stock.gameObject);

        foreach (Transform t in stock)
        {
            stockParts.Add(t.gameObject);
        }

        for (int i = 0; i < stockParts.Count; i++)
        {
            if (stockParts[i] != null && stockParts[i].TryGetComponent(out Renderer r))
            {
                stockRenderers.Add(r);
            }
        }

        currentFade = 1f;
        ApplyStockFade(currentFade);
    }

    void Update()
    {
        if (weaponController == null || !weaponController.IsAiming) return;

        if (isSwitching) { EndADS(); return; }
        if (weaponController.IsLocked) { EndADS(); return; }
        if (weaponController.IsReloading) { EndADS(false); return; }
        if (isSprinting) { EndADS(); return; }
    }

    void StartADS()
    {
        if (weaponController == null || weaponController.IsReloading || weaponController.IsLocked || isSprinting || isSwitching) return;

        weaponController.IsAiming = true;

        TweenFOV(adsFOV, weaponCamera, () =>
        {
            GameEventSystem.OnPlayerInSight?.Invoke(true);
        });
        TweenFOV(adsFOV, playerCamera);

        TweenStockFade(0f);

        animator.CrossFadeInFixedTime(aimAnimationHash, adsSpeed, animationLayer);
        ikDisplacementHandler.StartADS(adsSpeed);
    }

    void EndADS(bool returnToNormal = true)
    {
        if (weaponController == null) return;

        if (weaponController.IsAiming)
        {
            ikDisplacementHandler.EndADS(adsSpeed);
            TweenFOV(idleFOV, weaponCamera);
            TweenFOV(idleFOV, playerCamera);

            TweenStockFade(1f);

            if (returnToNormal) animator.CrossFadeInFixedTime(idleAnimationHash, adsSpeed, animationLayer);
            GameEventSystem.OnPlayerInSight?.Invoke(false);
        }

        weaponController.IsAiming = false;
    }

    void TweenFOV(float to, Camera tweenCamera, Action onComplete = null)
    {
        LeanTween.cancel(tweenCamera.gameObject);
        fovTween = LeanTween.value(tweenCamera.gameObject, tweenCamera.fieldOfView, to, adsSpeed).setOnUpdate((value) =>
        {
            tweenCamera.fieldOfView = value;
        }).setOnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    void TweenStockFade(float to, Action onComplete = null)
    {
        if (stockRenderers.Count == 0) return;

        LeanTween.cancel(gameObject);
        fadeTween = LeanTween.value(gameObject, currentFade, to, adsSpeed).setOnUpdate((value) =>
        {
            currentFade = value;
            ApplyStockFade(currentFade);
        }).setOnComplete(() =>
        {
            currentFade = to;
            ApplyStockFade(currentFade);
            onComplete?.Invoke();
        });
    }

    void ApplyStockFade(float fadeValue)
    {
        if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

        for (int i = 0; i < stockRenderers.Count; i++)
        {
            if (stockRenderers[i] == null) continue;

            stockRenderers[i].GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FadePropertyId, fadeValue);
            stockRenderers[i].SetPropertyBlock(propertyBlock);
        }
    }
}