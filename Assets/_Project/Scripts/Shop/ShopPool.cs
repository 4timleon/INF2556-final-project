using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopPool", menuName = "Scriptable Objects/Shop/Shop Pool")]
public class ShopPool : ScriptableObject
{
    [System.Serializable]
    public class RarityWeight
    {
        public ItemRarity rarity;
        public AnimationCurve weightByWave; // Kurve: X-Achse = Welle, Y-Achse = Gewichtung/Wahrscheinlichkeit
    }

    public List<ShopItemData> availableWeapons = new List<ShopItemData>();
    public List<ShopItemData> availableModules = new List<ShopItemData>();
    public List<RarityWeight> rarityWeights = new List<RarityWeight>();

    public ShopItemData GetRandomItem(List<ShopItemData> sourcePool, int currentWave)
    {
        if (sourcePool == null || sourcePool.Count == 0) return null;

        // 1. Wahrscheinlichkeiten für die aktuelle Welle berechnen
        Dictionary<ItemRarity, float> currentWeights = new Dictionary<ItemRarity, float>();
        float totalWeight = 0f;

        foreach (var rw in rarityWeights)
        {
            float weight = Mathf.Max(0f, rw.weightByWave.Evaluate(currentWave));
            currentWeights[rw.rarity] = weight;
            totalWeight += weight;
        }

        // 2. Zufällige Seltenheit auswürfeln
        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        ItemRarity selectedRarity = ItemRarity.Common;

        foreach (var entry in currentWeights)
        {
            cumulative += entry.Value;
            if (randomValue <= cumulative)
            {
                selectedRarity = entry.Key;
                break;
            }
        }

        // 3. Alle Items filtern, die diese Seltenheit haben
        List<ShopItemData> matchingItems = sourcePool.FindAll(item => item.rarity == selectedRarity);

        // Fallback: Falls es in der gewürfelten Seltenheit keine Items gibt, nimm ein beliebiges Item
        if (matchingItems.Count == 0)
        {
            return sourcePool[Random.Range(0, sourcePool.Count)];
        }

        return matchingItems[Random.Range(0, matchingItems.Count)];
    }
}