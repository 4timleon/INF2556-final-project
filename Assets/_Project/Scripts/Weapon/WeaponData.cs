using System.Collections.Generic;
using UnityEngine;
using WeaponNamespace;

public class WeaponData
{
    public readonly Dictionary<StatType, float> Stats;
    public readonly Dictionary<ModuleType, ModuleBase> Modules;

    public Rarity TotalRarity { get; private set; }
    public int TotalCost { get; private set; }

    private int raritySize;

    public WeaponData()
    {
        Stats = new();
        Modules = new();

        raritySize = 0;
    }

    public void AddModule(ModuleBase moduleBase)
    {
        Modules.Add(moduleBase.ModuleType, moduleBase);

        raritySize += (int)moduleBase.Rarity;

        TotalCost += moduleBase.ModuleCost;

        AddStat(moduleBase);

        TotalRarity = CalculateTotalRarity();
    }

    void AddStat(ModuleBase moduleBase)
    {
        List<StatModifier> statModifiers = moduleBase.StatModifiers;

        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier statModifier = statModifiers[i];

            StatType statType = statModifier.StatType;
            float value = statModifier.Value;

            if (Stats.ContainsKey(statType))
            {
                Stats[statType] += value;
            }
            else
            {
                Stats.Add(statType, value);
            }
        }
    }

    Rarity CalculateTotalRarity()
    {
        float averageRarity = (float)raritySize / Modules.Count;
        int totalRarity = Mathf.RoundToInt(averageRarity);

        return (Rarity)Mathf.Clamp(totalRarity, 1, 4);
    }
}
