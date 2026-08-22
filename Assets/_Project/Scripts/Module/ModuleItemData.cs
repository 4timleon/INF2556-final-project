using UnityEngine;

public enum ModuleType
{
    Sight,
    Barrel,
    Magazine,
    Stock
}

[CreateAssetMenu(fileName = "NewModule", menuName = "Scriptable Objects/Shop/Module Item")]
public class ModuleItemData : ShopItemData
{
    [Header("Welcher Slot?")]
    public ModuleType moduleType;

    [Header("Stat-Veränderungen")]
    [Tooltip("1.0 = unverändert, 1.2 = +20% Schaden")]
    public float damageMultiplier = 1.0f;

    [Tooltip("1.0 = unverändert, 0.8 = -20% Rückstoß")]
    public float recoilMultiplier = 1.0f;

    [Tooltip("Zusätzliche Kugeln im Magazin")]
    public int bonusMagazineCapacity = 0;

    [Tooltip("1.0 = unverändert, 0.8 = 20% schneller Nachladen")]
    public float reloadSpeedMultiplier = 1.0f;

    [Tooltip("Bonus auf Präzision beim Hüftfeuer")]
    public float hipFireAccuracyBonus = 0f;
}