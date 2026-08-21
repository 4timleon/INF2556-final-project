using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public WeaponData WeaponData { get; private set; }

    public void Initialize(WeaponData weaponData)
    {
        WeaponData = weaponData;
    }
}
