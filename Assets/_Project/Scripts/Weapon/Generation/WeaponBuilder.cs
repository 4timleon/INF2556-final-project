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
        weaponController.Initialize(new WeaponData());

        ModuleBase bodyPrefab = moduleDatabase.GetRandomBodyModule();

        BuildWeaponRecursivly(weaponController, bodyPrefab, weaponController.transform);

        return weaponController;
    }

    void BuildWeaponRecursivly(WeaponController weaponController, ModuleBase modulePrefab, Transform parent)
    {
        if (ModulFilter(weaponController, modulePrefab))
        {
            return;
        }

        ModuleBase moduleInstance = Instantiate(modulePrefab, Vector3.zero, Quaternion.identity);
        moduleInstance.transform.SetParent(parent);
        moduleInstance.transform.localPosition = Vector3.zero;
        moduleInstance.transform.localRotation = Quaternion.identity;

        weaponController.WeaponData.Modules.Add(moduleInstance.ModuleType, moduleInstance);

        List<ModuleSocket> sockets = moduleInstance.ModuleSockets;

        for (int i = 0; i < sockets.Count; i++)
        {
            ModuleSocket socket = sockets[i];

            ModuleBase nextModulePrefab = moduleDatabase.GetRandomModule(socket.ModuleType, socket.AcceptedJointSizes);

            if (nextModulePrefab != null)
            {
                BuildWeaponRecursivly(weaponController, nextModulePrefab, socket.Joint);
            }
        }
    }

    bool ModulFilter(WeaponController weaponController, ModuleBase nextModule)
    {
        WeaponData weaponData = weaponController.WeaponData;

        //prevent multiple sights
        if (nextModule.ModuleType == ModuleType.Sight && weaponData.Modules.ContainsKey(ModuleType.Sight))
        {
            return true;
        }

        return false;
    }
}
