using System;
using System.Collections.Generic;
using UnityEngine;
using WeaponNamespace;

public class WeaponData
{
    public readonly Dictionary<StatType, float> Stats;
    public readonly Dictionary<ModuleType, ModuleBase> Modules;

    public ModuleBase LastAddedModule { get; set; }

    public WeaponType TotalType { get; private set; }
    public Rarity TotalRarity { get; private set; }
    public int TotalCost { get; private set; }

    private Dictionary<WeaponType, int> typeBiases;

    private int raritySize;

    public WeaponData()
    {
        Stats = new();
        Modules = new();

        TotalType = WeaponType.AssaultRifle;

        typeBiases = new();

        raritySize = 0;
    }

    public void AddModule(ModuleBase moduleBase)
    {
        Modules.Add(moduleBase.ModuleType, moduleBase);

        raritySize += (int)moduleBase.Rarity;

        TotalCost += moduleBase.ModuleCost;

        AddTypeBias(moduleBase);
        AddStat(moduleBase);

        TotalRarity = CalculateTotalRarity();
        LastAddedModule = moduleBase;
    }

    public void FinalizeStats()
    {
        float accuracy = CalculateRMS(StatType.Accuracy);
        float handling = CalculateRMS(StatType.Handling);

        Stats[StatType.Accuracy] = accuracy;
        Stats[StatType.Handling] = handling;
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
                if (statType == StatType.Accuracy || statType == StatType.Handling)
                {
                    value *= value;
                }

                Stats[statType] += value;
            }
            else
            {
                Stats.Add(statType, value);
            }
        }
    }

    float CalculateRMS(StatType statType)
    {
        return Mathf.Sqrt(Stats[statType] / (float)Modules.Count);
    }

    void AddTypeBias(ModuleBase moduleBase)
    {
        WeaponType weaponType = moduleBase.BelongingType;
        int bias = moduleBase.TypeBias;

        if (typeBiases.ContainsKey(weaponType))
        {
            typeBiases[weaponType] += bias;
        }
        else
        {
            typeBiases.Add(weaponType, bias);
        }

        if (typeBiases.ContainsKey(TotalType))
        {
            if (typeBiases[weaponType] >= typeBiases[TotalType])
            {
                TotalType = weaponType;
            }
        }
        else
        {
            TotalType = weaponType;
        }
    }

    Rarity CalculateTotalRarity()
    {
        float averageRarity = (float)raritySize / Modules.Count;
        int totalRarity = Mathf.RoundToInt(averageRarity);

        return (Rarity)Mathf.Clamp(totalRarity, 1, 4);
    }
}
