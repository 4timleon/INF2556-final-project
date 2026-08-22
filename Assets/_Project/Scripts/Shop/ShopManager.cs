using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Pool Config")]
    [SerializeField] private ShopPool shopPool;

    [Header("Reroll Settings")]
    [SerializeField] private int baseRerollPrice = 100;
    [SerializeField] private int rerollPriceIncrease = 150;

    private int currentRerollCount = 0;
    private int currentWave = 1;

    public ShopItemData[] CurrentWeapons { get; private set; } = new ShopItemData[3];
    public ShopItemData[] CurrentModules { get; private set; } = new ShopItemData[3];

    public static event Action OnShopUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GenerateNewSelection();
    }

    public int GetCurrentRerollPrice()
    {
        return baseRerollPrice + (currentRerollCount * rerollPriceIncrease);
    }

    public void OnNewWaveStart(int newWave)
    {
        currentWave = newWave;
        currentRerollCount = 0;
        GenerateNewSelection();
    }

    public bool TryRerollShop()
    {
        int price = GetCurrentRerollPrice();

        if (ScoreManager.Instance.SpendScore(price))
        {
            currentRerollCount++;
            GenerateNewSelection();
            return true;
        }

        return false;
    }

    private void GenerateNewSelection()
    {
        for (int i = 0; i < 3; i++)
        {
            CurrentWeapons[i] = shopPool.GetRandomItem(shopPool.availableWeapons, currentWave);
            CurrentModules[i] = shopPool.GetRandomItem(shopPool.availableModules, currentWave);
        }

        OnShopUpdated?.Invoke();
    }

    public bool BuyWeapon(int index, GameObject player)
    {
        if (index < 0 || index >= CurrentWeapons.Length) return false;
        ShopItemData item = CurrentWeapons[index];
        if (item == null) return false;

        if (ScoreManager.Instance.SpendScore(item.basePrice))
        {
            // TODO spieler die waffe geben
            Debug.Log($"Waffe gekauft: {item.itemName}");
            CurrentWeapons[index] = null;
            OnShopUpdated?.Invoke();
            return true;
        }
        return false;
    }

    public bool BuyModule(int index, GameObject player)
    {
        if (index < 0 || index >= CurrentModules.Length) return false;
        ShopItemData item = CurrentModules[index];
        if (item == null) return false;

        if (ScoreManager.Instance.SpendScore(item.basePrice))
        {
            // TODO spieler modul geben
            Debug.Log($"Modul gekauft: {item.itemName}");
            CurrentModules[index] = null;
            OnShopUpdated?.Invoke();
            return true;
        }
        return false;
    }
}