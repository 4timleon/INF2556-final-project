using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using WeaponNamespace;

public abstract class ModuleBase : MonoBehaviour
{
    [Title("General:", 14)]
    [SerializeField] private Void v000;
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
    [SerializeField] private Void v00;
    [Space]
    [SerializeField] public bool HasHandPlacement;
    [SerializeField, EnableField(nameof(HasHandPlacement)), IndentProperty(20f)] public Transform HandPlacement;
    [SerializeField, EnableField(nameof(HasHandPlacement)), IndentProperty(20f)] public GripType GripType;
    [SerializeField, EnableField(nameof(HasHandPlacement)), IndentProperty(20f)] public int PlacementBias = 1;
    [Space]
    [SerializeField] public bool HasHighestPoint;
    [SerializeField, EnableField(nameof(HasHighestPoint)), IndentProperty(20f)] public bool HasIronSight;
    [SerializeField, EnableField(nameof(HasHighestPoint)), IndentProperty(20f)] public Transform HighestPoint;
    [Space]
    [SerializeField] public bool HasMuzzlePoint;
    [SerializeField, EnableField(nameof(HasMuzzlePoint)), IndentProperty(20f)] public Transform MuzzlePoint;
    [Line(GUIColor.Gray, 1, 2f)]
    [SerializeField] private Void v0;
    [Space]
    [SerializeField, DataTable()] public List<StatModifier> StatModifiers;
    [Space]
    [SerializeField, DataTable()] public List<ModuleSocket> ModuleSockets;

    public virtual void PullTrigger() { }

    public ModuleSocket GetModuleSocket(ModuleType moduleType)
    {
        foreach (ModuleSocket moduleSocket in ModuleSockets)
        {
            if (moduleSocket.ModuleType == moduleType)
            {
                return moduleSocket;
            }
        }

        return null;
    }
}
