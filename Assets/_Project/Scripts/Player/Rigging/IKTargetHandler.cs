using UnityEngine;

[RequireComponent(typeof(RigController))]
public class IKTargetHandler : MonoBehaviour
{
    [SerializeField] private Transform ikTarget;

    private Transform activeHandTarget;

    public void SetHandTarget(WeaponController weaponController)
    {
        activeHandTarget = weaponController.LeftHandTarget;
    }

    void Update()
    {
        if (activeHandTarget != null)
        {
            ikTarget.SetPositionAndRotation(activeHandTarget.position, activeHandTarget.rotation);
        }
    }
}
