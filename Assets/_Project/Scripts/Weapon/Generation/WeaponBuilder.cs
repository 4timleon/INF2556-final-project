using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using WeaponNamespace;

public class WeaponBuilder : MonoBehaviour
{
    [SerializeField] private ModuleDatabaseSO moduleDatabase;
    [SerializeField] private Vector3 initialPosition;

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

        weaponController.transform.position = initialPosition;
        weaponController.transform.rotation = Quaternion.identity;
        weaponController.transform.localScale = Vector3.one / 10f;

        weaponData.FinalizeStats();
        weaponController.Initialize(weaponData, this);

        return weaponController;
    }

    public bool ForcePlaceSight(WeaponData weaponData, ModuleBase moduleBase)
    {
        ModuleSocket moduleSocket = moduleBase.GetModuleSocket(ModuleType.Sight);

        if (moduleSocket == null)
        {
            return false;
        }

        ModuleBase nextSight = moduleDatabase.GetRandomModule(ModuleType.Sight, moduleBase.GetModuleSocket(ModuleType.Sight).AcceptedJointSizes);

        if (nextSight == null)
        {
            return false;
        }

        ModuleBase moduleInstance = Instantiate(nextSight, Vector3.zero, Quaternion.identity);
        moduleInstance.transform.SetParent(moduleSocket.Joint);
        moduleInstance.transform.localPosition = Vector3.zero;
        moduleInstance.transform.localRotation = Quaternion.identity;
        moduleInstance.transform.localScale = Vector3.one;

        weaponData.AddModule(moduleInstance);

        weaponData.FinalizeStats();

        return true;
    }

    void BuildWeaponRecursivly(WeaponData weaponData, ModuleBase modulePrefab, Transform parent)
    {
        if (ModuleFilter(weaponData, modulePrefab))
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

    bool ModuleFilter(WeaponData weaponData, ModuleBase nextModule)
    {
        //prevent sights
        if (nextModule.ModuleType == ModuleType.Sight)
        {
            return true;
        }

        //prevent attachments
        if (nextModule.ModuleType == ModuleType.DownAttachment || nextModule.ModuleType == ModuleType.SideAttachment)
        {
            if (Random.Range(1, 100) > 25) return true;
        }

        //prevent multiple sights
        // if (nextModule.ModuleType == ModuleType.Sight && weaponData.Modules.ContainsKey(ModuleType.Sight))
        // {
        //     return true;
        // }

        //prevent grip magazines
        if (nextModule.ModuleType == ModuleType.Magazine && weaponData.LastAddedModule != null && weaponData.LastAddedModule.ModuleType == ModuleType.Grip)
        {
            bool hasGripMagazine = ((BodyModule)weaponData.Modules[ModuleType.Body]).HasGripMagazine;
            if (!hasGripMagazine)
            {
                weaponData.LastAddedModule = null;
                return true;
            }
        }

        return false;
    }
}
