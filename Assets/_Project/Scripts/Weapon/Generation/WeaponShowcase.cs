using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShowcase : MonoBehaviour
{
    [SerializeField] private WeaponBuilder weaponBuilder;
    [SerializeField] private PlayerArmory playerArmory;

    private GameObject generatedWeapon;

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            WeaponController weaponController = weaponBuilder.BuildWeapon();
            weaponController.transform.SetParent(transform);
            weaponController.transform.position = Vector3.zero;

            playerArmory.AddWeapon(weaponController);
        }
    }
}
