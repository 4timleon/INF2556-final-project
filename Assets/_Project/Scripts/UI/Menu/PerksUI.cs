using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PerkType
{
    Juggernaut,
    Skull,
    Revive,
    Quick_Reload,
}

public class PerksUI : MonoBehaviour
{

    [SerializeField] private PerkVisual juggernaut;
    [SerializeField] private PerkVisual skull;
    [SerializeField] private PerkVisual revive;
    [SerializeField] private PerkVisual quickReload;

    private GameObject revivePerkInstance;

    private void OnEnable()
    {
        GameEventSystem.OnPerkAquired += GameEventSystem_OnRevivePerkAquired;
        GameEventSystem.OnPlayerPreDeath += GameEventSystem_OnPlayerPreDeath;
    }

    private void OnDisable()
    {
        GameEventSystem.OnPerkAquired -= GameEventSystem_OnRevivePerkAquired;
        GameEventSystem.OnPlayerPreDeath -= GameEventSystem_OnPlayerPreDeath;
    }

    private void GameEventSystem_OnRevivePerkAquired(PerkType perkType)
    {
        ApplyPerkAnimation(perkType);
    }

    private void GameEventSystem_OnPlayerPreDeath(bool state)
    {
        RemoveRevivePerk();
    }

    private void ApplyPerkAnimation(PerkType perk)
    {
        switch (perk)
        {
            case PerkType.Juggernaut:
                juggernaut.SpawnAndAnimatePerk(transform);
                break;
            case PerkType.Skull:
                skull.SpawnAndAnimatePerk(transform);
                break;
            case PerkType.Revive:
                revivePerkInstance = revive.SpawnAndAnimatePerk(transform);
                break;
            case PerkType.Quick_Reload:
                quickReload.SpawnAndAnimatePerk(transform);
                break;
        }
    }

    private void RemoveRevivePerk()
    {
        if (revivePerkInstance != null)
        {
            Destroy(revivePerkInstance);
        }
    }

}
