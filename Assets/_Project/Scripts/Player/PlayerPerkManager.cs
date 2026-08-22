using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPerkManager : MonoBehaviour
{
    public static event Action<PerkData> OnPerkAdded;
    public static event Action<PerkData> OnPerkRemoved;

    private readonly HashSet<PerkData> activePerks = new HashSet<PerkData>();

    public bool HasPerk(PerkData perk)
    {
        return activePerks.Contains(perk);
    }

    public bool UnlockPerk(PerkData perk)
    {
        if (perk == null || activePerks.Contains(perk))
        {
            return false;
        }

        activePerks.Add(perk);
        perk.ApplyPerk(gameObject);

        OnPerkAdded?.Invoke(perk);
        return true;
    }

    public void RemovePerk(PerkData perk)
    {
        if (activePerks.Remove(perk))
        {
            perk.RemovePerk(gameObject);
            OnPerkRemoved?.Invoke(perk);
        }
    }

    public void ClearAllPerks()
    {
        foreach (var perk in activePerks)
        {
            perk.RemovePerk(gameObject);
            OnPerkRemoved?.Invoke(perk);
        }
        activePerks.Clear();
    }
}