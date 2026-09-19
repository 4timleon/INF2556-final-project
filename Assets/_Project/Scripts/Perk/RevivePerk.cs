using EditorAttributes;
using UnityEngine;

public class RevivePerk : PerkBase
{
    [SerializeField] private EditorAttributes.Void v0;
    [Space]
    [Title("Revive:", 14)]
    [SerializeField] private Player player;

    protected override void ApplyPerk()
    {
        player.ToggleReviveStatus(true);
    }

    protected override void RevertPerk()
    {
        player.ToggleReviveStatus(false);
    }
}
