using UnityEngine;

[CreateAssetMenu(fileName = "Perk_Revive", menuName = "Scriptable Objects/Perks/Revive Perk")]
public class RevivePerk : PerkData
{
    [Header("Revive Settings")]
    public float invincibilityDuration = 3.0f;

    public override void ApplyPerk(GameObject player)
    {
        if (player.TryGetComponent<PlayerHealth>(out var healthComp))
        {
            healthComp.EnableSelfRevive(invincibilityDuration, this);
        }
    }

    public override void RemovePerk(GameObject player)
    {
        if (player.TryGetComponent<PlayerHealth>(out var healthComp))
        {
            healthComp.DisableSelfRevive();
        }
    }
}