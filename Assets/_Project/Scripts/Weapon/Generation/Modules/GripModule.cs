using EditorAttributes;
using UnityEngine;

public class GripModule : ModuleBase
{
    [Space]
    [Title("Grip", 14)]
    [SerializeField] private Void v1;

    [SerializeField] private Transform trigger;
    [SerializeField] private Transform pivot;

    public override void PullTrigger()
    {
        //todo: animate trigger
    }

    public Transform GetPivot()
    {
        return pivot;
    }
}
