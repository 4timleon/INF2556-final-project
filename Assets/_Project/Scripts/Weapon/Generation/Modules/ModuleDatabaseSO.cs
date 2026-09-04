using System;
using System.Collections.Generic;
using UnityEngine;
using WeaponNamespace;
using Random = System.Random;

[CreateAssetMenu(fileName = "NewModuleDatabase", menuName = "Weapon/Module Database")]
public class ModuleDatabaseSO : ScriptableObject
{
    [SerializeField] private List<ModuleBase> Body;
    [SerializeField] private List<ModuleBase> Barrel;
    [SerializeField] private List<ModuleBase> Handguard;
    [SerializeField] private List<ModuleBase> Stock;
    [SerializeField] private List<ModuleBase> Grip;
    [SerializeField] private List<ModuleBase> Muzzle;
    [SerializeField] private List<ModuleBase> Magazine;
    [SerializeField] private List<ModuleBase> Sight;
    [SerializeField] private List<ModuleBase> DownAttachment;
    [SerializeField] private List<ModuleBase> SideAttachment;

    private Random random;

    private Dictionary<(ModuleType, JointSize), List<ModuleBase>> categorizedModules;

    public void Initialize()
    {
        random = new Random();
        categorizedModules = new();

        CategorizeModuleLists();
    }

    public ModuleBase GetRandomModule(ModuleType moduleType, JointSizeFilter acceptedJointSizes)
    {
        List<ModuleBase> validCandidates = new List<ModuleBase>();

        foreach (JointSize size in Enum.GetValues(typeof(JointSize)))
        {
            JointSizeFilter sizeAsFilter = (JointSizeFilter)size;

            if (sizeAsFilter == JointSizeFilter.None) continue;

            if (acceptedJointSizes.HasFlag(sizeAsFilter))
            {
                var key = (moduleType, size);

                if (categorizedModules.TryGetValue(key, out var modulesOfThisSize))
                {
                    validCandidates.AddRange(modulesOfThisSize);
                }
            }
        }

        if (validCandidates.Count > 0)
        {
            return validCandidates[random.Next(validCandidates.Count)];
        }

        return null;
    }

    public ModuleBase GetRandomBodyModule()
    {
        return Body[random.Next(Body.Count)];
    }

    void CategorizeModuleLists()
    {
        foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
        {
            List<ModuleBase> moduleBases = GetListByType(moduleType);

            for (int i = 0; i < moduleBases.Count; i++)
            {
                ModuleBase moduleBase = moduleBases[i];

                ModuleType type = moduleBase.ModuleType;
                JointSize size = moduleBase.ModuleJointSize;

                var key = (type, size);

                if (!categorizedModules.ContainsKey(key))
                {
                    categorizedModules[key] = new List<ModuleBase>();
                }

                categorizedModules[key].Add(moduleBase);
            }
        }
    }

    List<ModuleBase> GetListByType(ModuleType moduleType) => moduleType switch
    {
        ModuleType.Body => Body,
        ModuleType.Barrel => Barrel,
        ModuleType.Handguard => Handguard,
        ModuleType.Stock => Stock,
        ModuleType.Grip => Grip,
        ModuleType.Muzzle => Muzzle,
        ModuleType.Magazine => Magazine,
        ModuleType.Sight => Sight,
        ModuleType.DownAttachment => DownAttachment,
        ModuleType.SideAttachment => SideAttachment,
        _ => throw new ArgumentOutOfRangeException(nameof(moduleType), $"Not expected module type: {moduleType}"),
    };
}
