using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using WeaponNamespace;

public abstract class ModuleBase : MonoBehaviour
{
    [Title("General:", 14)]
    [SerializeField] private Void v00;
    [Space]
    [SerializeField] public WeaponType BelongingType;
    [SerializeField] public int TypeBias = 1;
    [Space]
    [SerializeField] public Rarity Rarity;
    [SerializeField] public ModuleType ModuleType;
    [SerializeField] public JointSize ModuleJointSize;
    [Space]
    [SerializeField] public int ModuleCost;
    [Space]
    [Line(GUIColor.Gray, 1, 2f)]
    [SerializeField] private Void v0;
    [Space]
    [SerializeField, DataTable()] public List<StatModifier> StatModifiers;
    [Space]
    [SerializeField, DataTable()] public List<ModuleSocket> ModuleSockets;

    public virtual void PullTrigger() { }
}
