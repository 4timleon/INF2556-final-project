using UnityEngine;

[RequireComponent(typeof(RigController))]
public class IKDisplacementHandler : MonoBehaviour
{
    [SerializeField] private Vector3 baseOffset;
    [SerializeField] private float maxZ;
    [SerializeField] private float minZ;
    [SerializeField] private Transform displacementTarget;

    public void SetDisplacementTarget(WeaponController weaponController)
    {
        float zShift = Mathf.Clamp((-1f) * weaponController.LeftHandGripDistance, minZ, maxZ);

        displacementTarget.localPosition = new Vector3(baseOffset.x, baseOffset.y, zShift);
    }
}
