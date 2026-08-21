using EditorAttributes;
using UnityEngine;

public class GripModule : ModuleBase
{
    [Space]
    [Title("Grip", 14)]
    [SerializeField] private Void v1;

    [SerializeField] private Transform trigger;

    public override void PullTrigger()
    {
        //todo: animate trigger
    }
}
