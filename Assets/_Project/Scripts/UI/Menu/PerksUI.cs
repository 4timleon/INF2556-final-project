using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PerkType
{
    Health,
    Stamina,
    Revive,
    ThridHand,
}

public class PerksUI : MonoBehaviour
{

    [SerializeField] private PerkVisual health;
    [SerializeField] private PerkVisual stamina;
    [SerializeField] private PerkVisual revive;
    [SerializeField] private PerkVisual thirdHand;

    private GameObject healthPerkInstance;
    private GameObject staminaPerkInstance;
    private GameObject revivePerkInstance;
    private GameObject thirdHandPerkInstance;

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
        RemovePerks();
    }

    private void ApplyPerkAnimation(PerkType perk)
    {
        switch (perk)
        {
            case PerkType.Health:
                healthPerkInstance = health.SpawnAndAnimatePerk(transform);
                break;
            case PerkType.Stamina:
                staminaPerkInstance = stamina.SpawnAndAnimatePerk(transform);
                break;
            case PerkType.Revive:
                revivePerkInstance = revive.SpawnAndAnimatePerk(transform);
                break;
            case PerkType.ThridHand:
                thirdHandPerkInstance = thirdHand.SpawnAndAnimatePerk(transform);
                break;
        }
    }

    private void RemovePerks()
    {
        if (healthPerkInstance != null) Destroy(healthPerkInstance);
        if (staminaPerkInstance != null) Destroy(staminaPerkInstance);
        if (revivePerkInstance != null) Destroy(revivePerkInstance);
        if (thirdHandPerkInstance != null) Destroy(thirdHandPerkInstance);
    }

}
