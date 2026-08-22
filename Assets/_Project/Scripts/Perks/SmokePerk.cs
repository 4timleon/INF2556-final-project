using UnityEngine;

[CreateAssetMenu(fileName = "Perk_Smoke", menuName = "Scriptable Objects/Perks/Smoke Perk")]
public class SmokePerk : PerkData
{
    [Header("Speed Settings")]
    [Tooltip("Prozentualer Bonus auf Speed (z.B. 0.2 für +20% Speed)")]
    public float speedMultiplier = 1.2f;

    public override void ApplyPerk(GameObject player)
    {
        if (player.TryGetComponent<PlayerMovement>(out var movementComp))
        {
            movementComp.ApplySpeedMultiplier(speedMultiplier);
        }
    }

    public override void RemovePerk(GameObject player)
    {
        if (player.TryGetComponent<PlayerMovement>(out var movementComp))
        {
            movementComp.ApplySpeedMultiplier(1.0f);
        }
    }
}