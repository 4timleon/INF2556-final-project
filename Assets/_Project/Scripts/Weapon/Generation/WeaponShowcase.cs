using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShowcase : MonoBehaviour
{
    public WeaponBuilder weaponBuilder;

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
            WeaponController weaponController = weaponBuilder.BuildWeapon();
            weaponController.transform.SetParent(transform);
            weaponController.transform.position = Vector3.zero;
        }
    }
}
