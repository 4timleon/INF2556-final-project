using System.Collections.Generic;
using UnityEngine;
using WeaponNamespace;

public class WeaponData
{
    public List<StatModifier> StatModifiers { get; private set; }
    public Dictionary<ModuleType, ModuleBase> Modules { get; private set; }

    public WeaponData()
    {
        StatModifiers = new();
        Modules = new();
    }
}
