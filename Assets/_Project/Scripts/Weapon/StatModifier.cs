using System;
using EditorAttributes;
using UnityEngine;
using WeaponNamespace;

[Serializable]
public class StatModifier
{
    public StatType StatType;
    [Range(1f, 100f)] public float Value;
}
