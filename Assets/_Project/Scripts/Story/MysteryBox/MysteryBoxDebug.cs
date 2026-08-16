using System.Collections.Generic;
using UnityEngine;

public class MysteryBoxDebug : MonoBehaviour
{
    private void OnEnable()
    {
        GameEventSystem.OnWeaponWon += HandleWeaponWon;
    }

    private void OnDisable()
    {
        GameEventSystem.OnWeaponWon -= HandleWeaponWon;
    }

    private void HandleWeaponWon(WeaponType weaponType, List<ModuleType> modules)
    {
        string moduleList = modules.Count == 0 ? "keine Module" : string.Join(", ", modules);
        Debug.Log($"Waffe gewonnen: {weaponType} mit {modules.Count} Modulen ({moduleList})");
    }
}