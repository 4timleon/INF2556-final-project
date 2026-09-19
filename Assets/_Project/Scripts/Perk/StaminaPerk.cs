using EditorAttributes;
using UnityEngine;

public class StaminaPerk : PerkBase
{
    [SerializeField] private EditorAttributes.Void v0;
    [Space]
    [Title("Stamina:", 14)]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float newStamina = 200f;

    protected override void ApplyPerk()
    {
        playerMovement.IncreaseMaxStamina(newStamina);
    }

    protected override void RevertPerk()
    {
        playerMovement.ResetMaxStamina();
    }
}
