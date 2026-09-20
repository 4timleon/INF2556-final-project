using System.Collections;
using System.Linq;
using EditorAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MerchantUI : MonoBehaviour
{
    [Title("References In Scene", 14)]
    [SerializeField] private Void v1;
    [Header("General:")]
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private GeneralInputHandler playerInputHandler;
    [SerializeField] private Canvas canvas;
    [Space]
    [Header("Weapon:")]
    [SerializeField] private WeaponBuilder weaponBuilder;
    [SerializeField] private PlayerArmory playerArmory;
    [SerializeField] private int uiRenderLayer = 9;
    [SerializeField] private Transform leftWeaponSocket;
    [SerializeField] private Transform middleWeaponSocket;
    [SerializeField] private Transform rightWeaponSocket;
    [Space]
    [Title("References In Prefab", 14)]
    [SerializeField] private Void v2;
    [Header("UI:")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private CardUI card;
    [SerializeField] private RectTransform[] cardPivots;
    [SerializeField] private RectTransform deckPivot;
    [Space]
    [Header("Animation:")]
    [SerializeField] private Button buyButton;
    [SerializeField] private AudioSource cardSlideAudioSource;
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;

    [Space]
    [Header("Sound")]
    [SerializeField] private AudioSource buyAudioSource;

    private CardUI[] previousCards;

    private bool[] isAnimating;
    private bool[] isSelected;

    public readonly Rect[] RenderRects = { new Rect(0, 0, 0.333f, 1), new Rect(0.333f, 0, 0.333f, 1), new Rect(0.666f, 0, 0.333f, 1) };
    public Transform[] RenderSockets { get; private set; }

    private void Awake()
    {
        isAnimating = new bool[cardPivots.Length];
        isSelected = new bool[cardPivots.Length];
        previousCards = new CardUI[cardPivots.Length];

        RenderSockets = new Transform[]
        {
            leftWeaponSocket,
            middleWeaponSocket,
            rightWeaponSocket
        };
    }

    private void Start()
    {
        rerollButton.onClick.AddListener(() => RerollWeaponCards());
        GameEventSystem.OnWaveStarted += Event_WaveStarted;
    }

    private void OnEnable()
    {
        System.Array.Clear(isAnimating, 0, isAnimating.Length);
    }

    private void Event_WaveStarted()
    {
        System.Array.Clear(isAnimating, 0, isAnimating.Length);
        RerollWeaponCards(true);
    }

    private void OnDisable()
    {
        LeanTween.cancelAll();
    }

    private void OnDestroy()
    {
        rerollButton.onClick.RemoveListener(() => RerollWeaponCards());
    }

    private void BuyWeapon(CardUI cardUI)
    {
        if (isAnimating.All(a => a)) return;

        WeaponController weaponController = cardUI.WeaponController;

        if (GameData.Money < weaponController.WeaponData.TotalCost)
        {
            return;
        }

        playerArmory.AddWeapon(weaponController);

        cardUI.RemoveWeaponController();

        int slotIndex = cardUI.SlotIndex;

        ClearPreviousCard(slotIndex);
        menuManager.CloseMerchantMenu();

        if (buyAudioSource.isPlaying)
            buyAudioSource.Stop();
        SoundManager.Instance?.PlayMerchantPaidSound(buyAudioSource);

        GameData.Money -= weaponController.WeaponData.TotalCost;

        SpawnCard(slotIndex);
    }

    public void RerollWeaponCards(bool free = false)
    {
        if (!free && GameData.Money < 500)
        {
            return;
        }

        if (isAnimating.All(a => a)) return;

        //SetAllSelected(false);

        for (int i = 0; i < cardPivots.Length; i++)
        {
            ClearPreviousCard(i);
        }

        if (cardSlideAudioSource.isPlaying)
        {
            cardSlideAudioSource.Stop();
        }
        SoundManager.Instance?.PlayCardSlideSound(cardSlideAudioSource);

        for (int i = 0; i < cardPivots.Length; i++)
        {
            SpawnCard(i);
        }

        if (!free) GameData.Money -= 500;
    }

    private CardUI SpawnCard(int index)
    {
        CardUI cardInstance = Instantiate(card, transform);
        RectTransform cardRect = cardInstance.GetComponent<RectTransform>();
        cardRect.anchoredPosition = deckPivot.anchoredPosition;
        cardRect.rotation = deckPivot.rotation;

        previousCards[index] = cardInstance;

        cardInstance.Initialize(this, playerArmory, weaponBuilder, index, BuyWeapon);

        SetChildrenLayer(cardInstance.WeaponController.transform, uiRenderLayer);

        AnimateCard(cardInstance, cardPivots[index].anchoredPosition, index);

        return cardInstance;
    }

    private void ClearPreviousCard(int slotIndex)
    {
        if (previousCards[slotIndex] != null)
        {
            LeanTween.cancel(previousCards[slotIndex].gameObject);
            if (previousCards[slotIndex].WeaponController != null) Destroy(previousCards[slotIndex].WeaponController.gameObject);
            Destroy(previousCards[slotIndex].gameObject);
            previousCards[slotIndex] = null;
        }
    }

    void SetChildrenLayer(Transform root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }

    private void AnimateCard(CardUI card, Vector2 targetAnchoredPosition, int slotIndex)
    {
        card.SetAnimate(true);
        isAnimating[slotIndex] = true;
        card.ResetFrontSideRotation();

        //Tween position to target
        LeanTween.moveLocal(card.gameObject, targetAnchoredPosition, duration)
            .setEase(easeType)
            .setOnComplete(() =>
            {
                isAnimating[slotIndex] = false;
                card.SetAnimate(false);
                card.FlipCard(SetIsAnimating);
            })
            .setIgnoreTimeScale(true);

        //Tween rotation to target
        LeanTween.rotate(card.gameObject, Vector3.zero, duration)
            .setEase(easeType)
            .setIgnoreTimeScale(true);
    }

    public void SetCardPositionAndRotation()
    {
        for (int i = 0; i < previousCards.Length; i++)
        {
            if (previousCards[i] != null)
            {
                CardUI card = previousCards[i].GetComponent<CardUI>();
                previousCards[i].transform.position = cardPivots[i].transform.position;
                previousCards[i].transform.rotation = cardPivots[i].transform.rotation;

                if (isAnimating[i])
                    card.FlipCard(SetIsAnimating);
            }
        }
    }

    private void SetIsAnimating(int slotIndex, bool state)
    {
        if (slotIndex >= 0 && slotIndex < 3) isAnimating[slotIndex] = state;
    }

    public bool GetIsSelected(int slotIndex)
    {
        if (isSelected[slotIndex])
        {
            return true;
        }

        return false;
    }

    /* private void SetSelected(int slotIndex)
    {
        SetAllSelected(false);

        isSelected[slotIndex] = true;

        for (int i = 0; i < previousCards.Length; i++)
        {
            if (i == slotIndex) continue;

            previousCards[i].GetComponent<CardUI>().ResetScale();
        }
    }

    private void SetAllSelected(bool state)
    {
        for (int i = 0; i < isSelected.Length; i++)
        {
            isSelected[i] = state;
        }
    } */
}
