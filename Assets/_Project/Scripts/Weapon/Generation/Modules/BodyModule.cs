using EditorAttributes;
using UnityEngine;

public class BodyModule : ModuleBase
{
    [Space]
    [Title("Body", 14)]
    [SerializeField] private Void v1;

    [field: SerializeField] public ReloadBase ReloadBase { get; private set; }

    public bool HasMagazine => (ReloadBase is MagazineReload) && (ReloadBase != null);
    [SerializeField, EnableField(nameof(HasMagazine))] public bool HasGripMagazine = false;
    [SerializeField, DisableField(nameof(HasMagazine)), IndentProperty(20)] public Transform ChamberPlacement;
}
