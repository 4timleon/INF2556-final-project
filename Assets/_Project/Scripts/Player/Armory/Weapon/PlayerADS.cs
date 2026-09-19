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
    private LTDescr stockTween;

    private bool isSprinting;
    private bool isSwitching;

    private int aimAnimationHash;
    private int idleAnimationHash;

    private WeaponController weaponController;

    private GameObject weaponStock;
    private MeshRenderer[] stockRenderers;
    private readonly List<StockMaterialData> stockMaterials = new List<StockMaterialData>();
    private float currentStockFade = 1f;

    private class StockMaterialData
    {
        public Material material;
        public int colorPropId;
        public int smoothnessPropId;
        public float originalAlpha;
        public float originalSmoothness;
    }

    void Start()
    {
        aimAnimationHash = Animator.StringToHash(aimAnimationName);
        idleAnimationHash = Animator.StringToHash(idleAnimationName);
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
        if (weaponStock != null) LeanTween.cancel(weaponStock);

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

    void Event_WeaponHolster()
    {
        isSwitching = true;
        if (weaponStock != null) LeanTween.cancel(weaponStock);
    }

    void Event_WeaponDraw(WeaponController weaponController)
    {
        this.weaponController = weaponController;
        isSwitching = false;

        if (weaponStock != null) LeanTween.cancel(weaponStock);

        weaponStock = weaponController.GetModule(ModuleType.Stock)?.gameObject;
        CacheStockMaterials();
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

        TweenStockFade(0f);

        TweenFOV(adsFOV, weaponCamera, () =>
        {
            GameEventSystem.OnPlayerInSight?.Invoke(true);
        });
        TweenFOV(adsFOV, playerCamera);

        animator.CrossFadeInFixedTime(aimAnimationHash, adsSpeed, animationLayer);
        ikDisplacementHandler.StartADS(adsSpeed);
    }

    void EndADS(bool returnToNormal = true)
    {
        if (weaponController == null) return;

        TweenStockFade(1f);

        if (weaponController.IsAiming)
        {
            ikDisplacementHandler.EndADS(adsSpeed);
            TweenFOV(idleFOV, weaponCamera);
            TweenFOV(idleFOV, playerCamera);
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
        if (weaponStock == null || stockMaterials.Count == 0) return;

        LeanTween.cancel(weaponStock);
        stockTween = LeanTween.value(weaponStock, currentStockFade, to, adsSpeed).setOnUpdate((value) =>
        {
            currentStockFade = value;
            ApplyStockFade(value);
        }).setOnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    private void ApplyStockFade(float progress)
    {
        for (int i = 0; i < stockMaterials.Count; i++)
        {
            var data = stockMaterials[i];
            if (data.material == null) continue;

            if (data.material.HasProperty(data.colorPropId))
            {
                Color color = data.material.GetColor(data.colorPropId);
                color.a = Mathf.Lerp(0f, data.originalAlpha, progress);
                data.material.SetColor(data.colorPropId, color);
            }

            if (data.material.HasProperty(data.smoothnessPropId))
            {
                float smoothness = Mathf.Lerp(0f, data.originalSmoothness, progress);
                data.material.SetFloat(data.smoothnessPropId, smoothness);
            }
        }
    }

    private void CacheStockMaterials()
    {
        stockMaterials.Clear();
        currentStockFade = 1f;

        if (weaponStock == null)
        {
            stockRenderers = null;
            return;
        }

        stockRenderers = weaponStock.GetComponentsInChildren<MeshRenderer>(true);

        for (int i = 0; i < stockRenderers.Length; i++)
        {
            var r = stockRenderers[i];
            if (r == null) continue;

            Material[] mats = r.materials;
            for (int j = 0; j < mats.Length; j++)
            {
                Material mat = mats[j];
                if (mat == null) continue;

                int colorProp = mat.HasProperty("_BaseColor")
                    ? Shader.PropertyToID("_BaseColor")
                    : Shader.PropertyToID("_Color");

                int smoothnessProp = mat.HasProperty("_Smoothness")
                    ? Shader.PropertyToID("_Smoothness")
                    : Shader.PropertyToID("_Glossiness");

                float origAlpha = mat.HasProperty(colorProp) ? mat.GetColor(colorProp).a : 1f;
                float origSmoothness = mat.HasProperty(smoothnessProp) ? mat.GetFloat(smoothnessProp) : 0f;

                stockMaterials.Add(new StockMaterialData
                {
                    material = mat,
                    colorPropId = colorProp,
                    smoothnessPropId = smoothnessProp,
                    originalAlpha = origAlpha,
                    originalSmoothness = origSmoothness
                });
            }
        }
    }
}