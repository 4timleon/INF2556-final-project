using UnityEngine;
using WeaponNamespace;

public class MysteryBoxTestDebug : MonoBehaviour
{
    private void OnEnable()
    {
        GameEventSystem.OnWeaponWon += HandleWeaponWon;
    }

    private void OnDisable()
    {
        GameEventSystem.OnWeaponWon -= HandleWeaponWon;
    }

    private void HandleWeaponWon(WeaponType weaponType, System.Collections.Generic.List<ModuleType> modules)
    {
        string result = modules.Count == 0 ? "Keine Module" : string.Join(", ", modules);
        Debug.Log($"Waffe gewonnen mit {modules.Count} Modulen: {result}");
    }
}