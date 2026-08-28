using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IPointerClickHandler {

    public enum WeaponRarity {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [Serializable]
    public struct RarityEntry {
        public WeaponRarity rarity;
        public Sprite sprite;
    }

    [Header("References")]
    [SerializeField] private MerchantCardStats cardStats;
    [SerializeField] private Transform cardVisual;
    [SerializeField] private RectTransform rayCastHitbox;
    [SerializeField] private Outline outline;

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

    //Initialize
    private MerchantUI merchantUI;
    private int slotIndex;

    private RectTransform rectTransform;
    private Quaternion targetRotation;
    private Vector2 pointerPosition;
    private Camera pointerCamera;

    private bool isFlipped = false;
    private bool isInAnimation = false;

    private readonly StringBuilder stringBuilder = new StringBuilder(16);

    private Quaternion frontSideTransform;
    private Quaternion backSideTransform;

    private bool pointerEntered = false;
    private Quaternion defaultRotation;

    private Vector3 targetScale = new Vector3(1.1f, 1.1f, 1.1f);
    private float scaleDuration = 0.2f;

    //Cache a Function for later
    private Action<int> onAction;

    private void Start() {
        rectTransform = GetComponent<RectTransform>();
        defaultRotation = Quaternion.Euler(0f, -180f, 0f);
        cardVisual.localRotation = Quaternion.identity;

        frontSide.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        frontSideTransform = frontSide.transform.localRotation;

        backSideTransform = backSide.transform.rotation;
        backSideTransform.y = 0f;
        backSide.transform.rotation = backSideTransform;

        frontSide.SetActive(isFlipped);
        backSide.SetActive(!isFlipped);
    }

    private void Update() {
        if (isInAnimation) return;

        LookAtMouse();
    }

    public void Initialize(MerchantUI merchantUI, int slotIndex, Action<int> onAction) {
        this.merchantUI = merchantUI;
        this.slotIndex = slotIndex;
        this.onAction = onAction;
    }

    private void ApplyStats(int priceValue, WeaponRarity rarity) {
        //Change Rarity Background
        SetRarityBackground(rarity);

        //Card Stats
        //cardStats.SetStats();

        //Price Number
        stringBuilder.Clear();
        stringBuilder.Append(priceValue);
        priceNumber.SetText(stringBuilder);
    }

    private void SetRarityBackground(WeaponRarity rarity) {
        foreach (RarityEntry rarityEntry in rarityList) {
            if (rarityEntry.rarity == rarity) {
                rarityBackground.sprite = rarityEntry.sprite;
            }
        }
    }

    private void LookAtMouse() {
        if (pointerEntered) {
            //Mouse Position relative to the Card center (-1 to +1)
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

            targetRotation = Quaternion.Euler(tiltX, tiltY, 0f);
        } else {
            targetRotation = defaultRotation;
        }

        cardVisual.localRotation = Quaternion.Lerp(cardVisual.localRotation, targetRotation, Time.unscaledDeltaTime * smoothSpeed);
    }

    public void FlipCard() {
        float targetAngle = isFlipped ? 0f : 180f;

        //Rotate until 90°, then deactivate the back Side and activate front Side
        LeanTween.rotateY(cardVisual.gameObject, 90f, flipDuration)
            .setOnComplete(() => {
                //Switch sides
                isFlipped = !isFlipped;
                frontSide.SetActive(isFlipped);
                backSide.SetActive(!isFlipped);

                //Rotate the other 90°
                LeanTween.rotateY(cardVisual.gameObject, targetAngle, flipDuration)
                    .setOnComplete(() => {
                        //Move Price Tag down
                        LeanTween.move(priceContainer.gameObject, priceContainerPivot, 0.5f)
                            .setEase(easeType)
                            .setIgnoreTimeScale(true);
                    })
                    .setIgnoreTimeScale(true);
            })
            .setIgnoreTimeScale(true);

    }

    public void SetAnimate(bool state) {
        isInAnimation = state;
    }

    public void ResetFrontSideRotation() {
        frontSide.transform.localRotation = frontSideTransform;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!merchantUI.GetIsSelected(slotIndex)) {
            SetScale();
        }

        pointerEntered = true;
        pointerPosition = eventData.position;
        pointerCamera = eventData.enterEventCamera;
        outline.enabled = true;
    }

    public void OnPointerMove(PointerEventData eventData) {
        pointerPosition = eventData.position;
        pointerCamera = eventData.enterEventCamera;
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!merchantUI.GetIsSelected(slotIndex)) {
            ResetScale();
        }

        pointerEntered = false;
        outline.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData) {
        onAction(slotIndex);
    }

    private void SetScale() {
        LeanTween.cancel(cardVisual.gameObject);
        LeanTween.scale(cardVisual.gameObject, targetScale, scaleDuration)
            .setIgnoreTimeScale(true);
    }

    public void ResetScale() {
        LeanTween.cancel(cardVisual.gameObject);
        LeanTween.scale(cardVisual.gameObject, Vector3.one, scaleDuration)
            .setIgnoreTimeScale(true);
    }
}
