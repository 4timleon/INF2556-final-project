using UnityEngine;

[CreateAssetMenu(fileName = "Perk_Reload", menuName = "Scriptable Objects/Perks/Reload Perk")]
public class ReloadPerk : PerkData
{
    [Header("Reload Settings")]
    [Tooltip("Faktor für die Nachladezeit (0.5 = doppelt so schnell)")]
    public float reloadSpeedMultiplier = 0.5f;

    public override void ApplyPerk(GameObject player)
    {
        //if (player.TryGetComponent<PlayerWeaponHandler>(out var weaponComp))
        {
            //weaponComp.ApplyReloadMultiplier(reloadSpeedMultiplier);
        }
    }

    public override void RemovePerk(GameObject player)
    {
        //if (player.TryGetComponent<PlayerWeaponHandler>(out var weaponComp))
        {
            //weaponComp.ApplyReloadMultiplier(1.0f);
        }
    }
}