using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShowcase : MonoBehaviour
{
    [SerializeField] private WeaponBuilder weaponBuilder;
    [SerializeField] private RigController rigController;

    private GameObject generatedWeapon;

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (generatedWeapon != null)
            {
                Destroy(generatedWeapon);
            }

            WeaponController weaponController = weaponBuilder.BuildWeapon();
            weaponController.transform.SetParent(transform);
            weaponController.transform.position = Vector3.zero;

            rigController.TransformWeapon(weaponController);

            generatedWeapon = weaponController.gameObject;
        }
    }
}
