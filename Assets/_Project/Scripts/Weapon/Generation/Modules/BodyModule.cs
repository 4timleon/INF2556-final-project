using EditorAttributes;
using UnityEngine;

public class BodyModule : ModuleBase
{
    [Space]
    [Title("Body", 14)]
    [SerializeField] private Void v1;

    [SerializeField] public bool HasMagazine = true;
    [SerializeField, DisableField(nameof(HasMagazine))] public Transform ChamberPlacement;
}
