using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WeaponNamespace;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IPointerClickHandler
{
    [Serializable]
    public struct RarityEntry
    {
        public Rarity rarity;
        public Sprite sprite;
    }

    [Header("References")]
    [SerializeField] private MerchantCardStats cardStats;
    [SerializeField] private Transform cardVisual;
    [SerializeField] private RectTransform rayCastHitbox;
    [SerializeField] private RawImage weaponRender;
    [SerializeField] private Outline outline;
    [SerializeField] private Image shineEffectImage;

    [Header("Price")]
    [SerializeField] private Transform priceContainer;
    [SerializeField] private Transform priceContainerPivot;
    [SerializeField] private TextMeshProUGUI priceNumber;

    [Header("Rarity")]
    [SerializeField] private List<RarityEntry> rarityList = new();
    [SerializeField] private Image rarityBackground;

    [Header("Flipping")]
    [SerializeField] private GameObject frontSide;
    [SerializeField] private GameObject backSide;
    [SerializeField] private float flipDuration = 0.2f;

    [Header("Tilt Settings")]
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Ease Setting")]
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;

    public Outline Outline => outline;
    public WeaponController WeaponController { get; private set; }
    public int SlotIndex { get; private set; }

    //Initialize
    private MerchantUI merchantUI;
    private WeaponBuilder weaponBuilder;
    private PlayerArmory playerArmory;

    private RectTransform rectTransform;
    private Quaternion targetRotation;
    private Vector2 pointerPosition;
    private Camera pointerCamera;
    private Material shineEffectMaterial;

    private bool isFlipped = false;
    private bool isInAnimation = false;

    private readonly StringBuilder stringBuilder = new StringBuilder(16);

    private Quaternion frontSideTransform;
    private Quaternion backSideTransform;

    private bool pointerEntered = false;
    private Quaternion defaultRotation;

    private Vector3 targetScale = new Vector3(1.1f, 1.1f, 1.1f);
    private float scaleDuration = 0.2f;

    private LTDescr scaleTween;

    //Callback Function
    private Action<CardUI> onCardClicked;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        defaultRotation = Quaternion.Euler(0f, -180f, 0f);
        cardVisual.localRotation = Quaternion.identity;

        shineEffectMaterial = new Material(shineEffectImage.material);
        shineEffectImage.material = shineEffectMaterial;

        frontSide.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        frontSideTransform = frontSide.transform.localRotation;

        backSideTransform = backSide.transform.rotation;
        backSideTransform.y = 0f;
        backSide.transform.rotation = backSideTransform;

        frontSide.SetActive(isFlipped);
        backSide.SetActive(!isFlipped);

        scaleTween = null;
    }

    private void Update()
    {
        if (isInAnimation) return;

        LookAtMouse();
    }

    public void Initialize(MerchantUI merchantUI, PlayerArmory playerArmory, WeaponBuilder weaponBuilder, int slotIndex, Action<CardUI> onCardClicked)
    {
        SlotIndex = slotIndex;

        this.merchantUI = merchantUI;
        this.onCardClicked = onCardClicked;
        this.weaponBuilder = weaponBuilder;
        this.playerArmory = playerArmory;

        weaponRender.uvRect = merchantUI.RenderRects[SlotIndex];

        GenerateWeapon();
        ApplyStats();
    }

    public void RemoveWeaponController() => WeaponController = null;

    private void GenerateWeapon()
    {
        WeaponController = weaponBuilder.BuildWeapon();
        WeaponController.transform.SetParent(merchantUI.RenderSockets[SlotIndex]);
        WeaponController.transform.localPosition = Vector3.zero;
        WeaponController.transform.localRotation = Quaternion.identity;
        WeaponController.transform.localScale = Vector3.one;
        WeaponController.CenterOnSocket(merchantUI.RenderSockets[SlotIndex]);
    }

    private void ApplyStats()
    {
        WeaponData weaponData = WeaponController.WeaponData;

        //Change Rarity Background
        SetRarityBackground(weaponData.TotalRarity);

        foreach (StatType statType in weaponData.Stats.Keys)
        {
            //Set the Stats and if Weapon Type of the Card matches with the Type of the Players Current Weapon then compare
            cardStats.SetStat(statType, playerArmory.CurrentWeaponController.GetStat(statType), WeaponController.GetStat(statType));
        }

        //Price Number
        stringBuilder.Clear();
        stringBuilder.Append(weaponData.TotalCost);
        priceNumber.SetText(stringBuilder);
    }

    private void SetRarityBackground(Rarity rarity)
    {
        foreach (RarityEntry rarityEntry in rarityList)
        {
            if (rarityEntry.rarity == rarity)
            {
                rarityBackground.sprite = rarityEntry.sprite;
            }
        }
    }

    private void LookAtMouse()
    {
        if (pointerEntered)
        {
            //Mouse Position on the Canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                pointerPosition,
                pointerCamera,
                out Vector2 localMousePos
            );

            //Normalize Position
            float normalizedX = Mathf.Clamp(localMousePos.x / (rectTransform.rect.width * 0.5f), -1f, 1f);
            float normalizedY = Mathf.Clamp(localMousePos.y / (rectTransform.rect.height * 0.5f), -1f, 1f);

            //Tilt on X- and Y-Axis
            float tiltX = normalizedY * maxTiltAngle;
            float tiltY = 180f + (normalizedX * maxTiltAngle);

            //Set Shine Effect Value
            float shineValue = Mathf.InverseLerp(-1, 1f, -normalizedX);
            shineEffectMaterial.SetFloat("_ShineValue", shineValue);

            targetRotation = Quaternion.Euler(tiltX, tiltY, 0f);
        }
        else
        {
            targetRotation = defaultRotation;
            shineEffectMaterial.SetFloat("_ShineValue", 0f);
        }

        cardVisual.localRotation = Quaternion.Lerp(cardVisual.localRotation, targetRotation, Time.unscaledDeltaTime * smoothSpeed);
    }

    public void FlipCard(Action<int, bool> onSetAnimating)
    {
        float targetAngle = isFlipped ? 0f : 180f;

        //Rotate until 90°, then deactivate the back Side and activate front Side
        LeanTween.rotateY(cardVisual.gameObject, 90f, flipDuration)
            .setOnComplete(() =>
            {
                //Switch sides
                isFlipped = !isFlipped;
                frontSide.SetActive(isFlipped);
                backSide.SetActive(!isFlipped);

                //Rotate the other 90°
                LeanTween.rotateY(cardVisual.gameObject, targetAngle, flipDuration)
                    .setOnComplete(() =>
                    {
                        //Set isAnimating false
                        onSetAnimating(SlotIndex, false);

                        //Move Price Tag down
                        LeanTween.move(priceContainer.gameObject, priceContainerPivot, 0.5f)
                            .setEase(easeType)
                            .setIgnoreTimeScale(true);
                    })
                    .setIgnoreTimeScale(true);
            })
            .setIgnoreTimeScale(true);

    }

    public void SetAnimate(bool state)
    {
        isInAnimation = state;
    }

    public void ResetFrontSideRotation()
    {
        frontSide.transform.localRotation = frontSideTransform;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!merchantUI.GetIsSelected(SlotIndex))
        {
            SetScale();
        }

        pointerEntered = true;
        pointerPosition = eventData.position;
        pointerCamera = eventData.enterEventCamera;
        outline.enabled = true;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        pointerPosition = eventData.position;
        pointerCamera = eventData.enterEventCamera;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!merchantUI.GetIsSelected(SlotIndex))
        {
            ResetScale();
        }

        pointerEntered = false;
        outline.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onCardClicked(this);
    }

    private void SetScale()
    {
        if (scaleTween != null)
        {
            LeanTween.cancel(scaleTween.uniqueId);
            scaleTween = null;
        }

        scaleTween = LeanTween.scale(cardVisual.gameObject, targetScale, scaleDuration)
            .setIgnoreTimeScale(true);
    }

    public void ResetScale()
    {
        if (scaleTween != null)
        {
            LeanTween.cancel(scaleTween.uniqueId);
            scaleTween = null;
        }

        scaleTween = LeanTween.scale(cardVisual.gameObject, Vector3.one, scaleDuration)
            .setIgnoreTimeScale(true);
    }

    private void OnDestroy()
    {
        if (shineEffectMaterial != null)
        {
            Destroy(shineEffectMaterial);
        }
    }
}
