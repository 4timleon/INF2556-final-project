using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MerchantUI : MonoBehaviour
{

    [SerializeField] private GeneralInputHandler playerInputHandler;
    [SerializeField] private Button rerollButton;
    [SerializeField] private CardUI card;
    [SerializeField] private RectTransform[] cardPivots;
    [SerializeField] private RectTransform deckPivot;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Button buyButton;
    [SerializeField] private AudioSource cardSlideAudioSource;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;

    private GameObject[] previousCards;

    private bool[] isAnimating;
    private bool[] isSelected;

    private void Awake()
    {
        isAnimating = new bool[cardPivots.Length];
        isSelected = new bool[cardPivots.Length];
        previousCards = new GameObject[cardPivots.Length];
    }

    private void OnEnable()
    {
        System.Array.Clear(isAnimating, 0, isAnimating.Length);
        rerollButton.onClick.AddListener(RerollWeaponCards);
        buyButton.onClick.AddListener(BuyWeapon);
    }

    private void OnDisable()
    {
        LeanTween.cancelAll();
        rerollButton.onClick.RemoveListener(RerollWeaponCards);
        buyButton.onClick.RemoveListener(BuyWeapon);
    }

    private void BuyWeapon()
    {
        if (isAnimating.All(a => a)) return;

        foreach (GameObject card in previousCards)
        {
            CardUI cardUI = card.GetComponent<CardUI>();
            if (cardUI.Outline.enabled)
            {
                //Give Weapon
            }
        }
    }

    public void RerollWeaponCards()
    {
        if (isAnimating.All(a => a)) return;

        SetAllSelected(false);

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
            CardUI cardInstance = Instantiate(card, transform);
            RectTransform cardRect = cardInstance.GetComponent<RectTransform>();
            cardRect.anchoredPosition = deckPivot.anchoredPosition;
            cardRect.rotation = deckPivot.rotation;

            previousCards[i] = cardInstance.gameObject;

            cardInstance.Initialize(this, i, SetSelected);

            AnimateCard(cardInstance, cardPivots[i].anchoredPosition, i);
        }
    }

    private void ClearPreviousCard(int slotIndex)
    {
        if (previousCards[slotIndex] != null)
        {
            LeanTween.cancel(previousCards[slotIndex]);
            Destroy(previousCards[slotIndex]);
            previousCards[slotIndex] = null;
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
                card.FlipCard();
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
                    card.FlipCard();
            }
        }
    }

    public bool GetIsSelected(int slotIndex)
    {
        if (isSelected[slotIndex])
        {
            return true;
        }

        return false;
    }

    private void SetSelected(int slotIndex)
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
    }
}
