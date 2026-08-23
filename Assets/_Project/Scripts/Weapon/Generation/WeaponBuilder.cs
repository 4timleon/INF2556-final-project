using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using WeaponNamespace;

public class WeaponBuilder : MonoBehaviour
{
    public ModuleDatabaseSO moduleDatabase;

    void Start()
    {
        moduleDatabase.Initialize();
    }

    public WeaponController BuildWeapon()
    {
        WeaponController weaponController = new GameObject("Weapon").AddComponent<WeaponController>();
        WeaponData weaponData = new WeaponData();
        ModuleBase bodyPrefab = moduleDatabase.GetRandomBodyModule();

        BuildWeaponRecursivly(weaponData, bodyPrefab, weaponController.transform);

        Transform pivot = (weaponData.Modules[ModuleType.Grip] as GripModule)?.GetPivot();
        Vector3 transformedPivot = (-1f) * weaponController.transform.TransformPoint(pivot.position);

        weaponData.Modules[ModuleType.Body].transform.localPosition = transformedPivot;

        weaponController.Initialize(weaponData);

        return weaponController;
    }

    void BuildWeaponRecursivly(WeaponData weaponData, ModuleBase modulePrefab, Transform parent)
    {
        if (ModulFilter(weaponData, modulePrefab))
        {
            return;
        }

        ModuleBase moduleInstance = Instantiate(modulePrefab, Vector3.zero, Quaternion.identity);
        moduleInstance.transform.SetParent(parent);
        moduleInstance.transform.localPosition = Vector3.zero;
        moduleInstance.transform.localRotation = Quaternion.identity;

        weaponData.AddModule(moduleInstance);

        List<ModuleSocket> sockets = moduleInstance.ModuleSockets;

        for (int i = 0; i < sockets.Count; i++)
        {
            ModuleSocket socket = sockets[i];

            ModuleBase nextModulePrefab = moduleDatabase.GetRandomModule(socket.ModuleType, socket.AcceptedJointSizes);

            if (nextModulePrefab != null)
            {
                BuildWeaponRecursivly(weaponData, nextModulePrefab, socket.Joint);
            }
        }
    }

    bool ModulFilter(WeaponData weaponData, ModuleBase nextModule)
    {
        //prevent multiple sights
        if (nextModule.ModuleType == ModuleType.Sight && weaponData.Modules.ContainsKey(ModuleType.Sight))
        {
            return true;
        }

        return false;
    }
}
