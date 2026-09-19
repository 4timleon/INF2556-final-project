using EditorAttributes;
using UnityEngine;

public class HealthPerk : PerkBase
{
    [SerializeField] private Void v0;
    [Space]
    [Title("Health:", 14)]
    [SerializeField] private Player player;
    [SerializeField] private float newHealth = 200f;

    protected override void ApplyPerk()
    {
        player.IncreaseMaxHealth(newHealth);
    }

    protected override void RevertPerk()
    {
        player.ResetMaxHealth();
    }
}
