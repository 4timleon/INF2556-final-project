using EditorAttributes;
using UnityEngine;

public class ThirdHandPerk : PerkBase
{
    [SerializeField] private Void v0;
    [Space]
    [Title("Third Hand:", 14)]
    [SerializeField] private PlayerArmory playerArmory;

    protected override void ApplyPerk()
    {
        playerArmory.SetMaxWeaponAmount(3);
    }

    protected override void RevertPerk()
    {
        playerArmory.SetMaxWeaponAmount(2);
    }
}
