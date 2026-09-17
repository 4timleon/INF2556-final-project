using System;
using TMPro;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [Header("Price")]
    [SerializeField] private GameObject priceContainer;
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("Repair")]
    [SerializeField] private GameObject repairContainer;

    private void Start()
    {
        priceContainer.SetActive(false);
        repairContainer.SetActive(false);
    }

    private void OnEnable()
    {
        GameEventSystem.OnShowRepairText += GameEventSystem_OnShowRepairText;
        GameEventSystem.OnShowPriceText += GameEventSystem_OnShowPriceText;
        GameEventSystem.OnHidePriceText += GameEventSystem_OnHidePriceText;
    }

    private void OnDisable()
    {
        GameEventSystem.OnShowRepairText -= GameEventSystem_OnShowRepairText;
        GameEventSystem.OnShowPriceText -= GameEventSystem_OnShowPriceText;
        GameEventSystem.OnHidePriceText -= GameEventSystem_OnHidePriceText;
    }

    private void GameEventSystem_OnShowRepairText(bool state)
    {
        ShowRepair(state);
    }

    private void GameEventSystem_OnShowPriceText(float amount)
    {
        ShowPrice(true);
        priceText.text = amount.ToString();
    }

    private void GameEventSystem_OnHidePriceText()
    {
        ShowPrice(false);
    }

    private void ShowRepair(bool state)
    {
        repairContainer.SetActive(state);
    }

    private void ShowPrice(bool state)
    {
        priceContainer.SetActive(state);
    }

}
