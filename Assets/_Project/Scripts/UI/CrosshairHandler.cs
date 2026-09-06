using System;
using EditorAttributes;
using UnityEngine;
using WeaponNamespace;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class CrosshairHandler : MonoBehaviour
{
    [Title("References:", 14)]
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Canvas canvas;
    [Space]
    [Title("Crosshair Settings:", 14)]
    [SerializeField] private float minSize = 25f;
    [SerializeField] private float smoothSpeed = 12f;

    private RectTransform rectTransform;
    private RectTransform canvasRect;
    private CanvasGroup canvasGroup;

    private float accuracyNormalized;
    private float currentVisualSpread;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = canvas.GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        currentVisualSpread = minSize;
        rectTransform.sizeDelta = new Vector2(minSize, minSize);
    }

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
        GameEventSystem.OnPlayerPreDeath += Event_PreDeath;
        GameEventSystem.OnPlayerDeath += Event_PlayerDeath;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerPreDeath -= Event_PreDeath;
        GameEventSystem.OnPlayerDeath -= Event_PlayerDeath;
    }

    void Update()
    {
        float currentSpread = playerInputs.IsMoving ? accuracyNormalized : (accuracyNormalized / 5f);

        float targetDiameter;

        float halfFovRad = playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float canvasHeight = canvasRect.rect.height;
        float spreadDiameter = currentSpread * (canvasHeight / Mathf.Tan(halfFovRad));

        targetDiameter = minSize + spreadDiameter;

        currentVisualSpread = Mathf.Lerp(currentVisualSpread, targetDiameter, Time.deltaTime * smoothSpeed);
        rectTransform.sizeDelta = new Vector2(currentVisualSpread, currentVisualSpread);
    }

    void Event_WeaponDraw(WeaponController weaponController)
    {
        accuracyNormalized = 1f - (weaponController.GetStat(StatType.Accuracy) / 100f);
    }

    void Event_PreDeath(bool started) => ToggleCrosshair(!started);
    void Event_PlayerDeath(float c) => ToggleCrosshair(false);

    void ToggleCrosshair(bool status)
    {
        float to = status ? 1f : 0f;

        LeanTween.value(canvasGroup.alpha, to, .5f).setOnUpdate((value) =>
        {
            canvasGroup.alpha = value;
        });
    }
}