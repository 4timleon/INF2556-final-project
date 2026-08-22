using UnityEngine;

[CreateAssetMenu(fileName = "Perk_Stamina", menuName = "Scriptable Objects/Perks/Stamina Perk")]
public class StaminaPerk : PerkData
{
    [Header("Stamina Settings")]
    [Tooltip("Faktor für den Ausdauerverbrauch (0.5 = halbiert)")]
    public float staminaDrainMultiplier = 0.5f;

    public override void ApplyPerk(GameObject player)
    {
        // TODO anpassen auf Player Zeugs
        //if (player.TryGetComponent<PlayerStamina>(out var staminaComp))
        {
            //staminaComp.ApplyStaminaDrainMultiplier(staminaDrainMultiplier);
        }
    }

    public override void RemovePerk(GameObject player)
    {
        //if (player.TryGetComponent<PlayerStamina>(out var staminaComp))
        {
            //staminaComp.ApplyStaminaDrainMultiplier(1.0f);
        }
    }
}