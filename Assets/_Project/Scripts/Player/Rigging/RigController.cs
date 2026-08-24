using UnityEngine;
using WeaponNamespace;

[RequireComponent(typeof(IKTargetHandler), typeof(IKDisplacementHandler))]
public class RigController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string gripBlend = "GripBlend";
    [SerializeField] private Transform rightHandSocket;

    private IKTargetHandler targetHandler;
    private IKDisplacementHandler displacementHandler;

    void Start()
    {
        targetHandler = GetComponent<IKTargetHandler>();
        displacementHandler = GetComponent<IKDisplacementHandler>();
    }

    public void TransformWeapon(WeaponController weaponController)
    {
        Transform weaponTransform = weaponController.transform;

        weaponTransform.SetParent(rightHandSocket);
        weaponTransform.localPosition = Vector3.zero;
        weaponTransform.localRotation = Quaternion.identity;
        weaponTransform.localScale = Vector3.one;

        targetHandler.SetHandTarget(weaponController);
        displacementHandler.SetDisplacementTarget(weaponController);

        ChangeGripType(weaponController);
    }

    void ChangeGripType(WeaponController weaponController)
    {
        GripType gripType = weaponController.LeftHandGripType;
        animator.SetFloat(gripBlend, (float)gripType);
    }
}
