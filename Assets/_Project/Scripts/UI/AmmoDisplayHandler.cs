using System;
using TMPro;
using UnityEngine;

public class AmmoDisplayHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentAmmoText;
    [SerializeField] private TextMeshProUGUI maxAmmoText;

    private WeaponController weaponController;

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponShoot += Event_WeaponShoot;
        GameEventSystem.OnPlayerReloadTick += Event_ReloadTick;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponShoot -= Event_WeaponShoot;
        GameEventSystem.OnPlayerReloadEnded -= Event_ReloadTick;
    }

    void Event_WeaponDraw(WeaponController weaponController)
    {
        this.weaponController = weaponController;

        currentAmmoText.text = weaponController.CurrentMagazineAmmo.ToString();
        maxAmmoText.text = weaponController.CurrentWeaponAmmo.ToString();
    }

    void Event_WeaponShoot(float arg1, float arg2)
    {
        currentAmmoText.text = weaponController.CurrentMagazineAmmo.ToString();
    }

    void Event_ReloadTick()
    {
        currentAmmoText.text = weaponController.CurrentMagazineAmmo.ToString();
        maxAmmoText.text = weaponController.CurrentWeaponAmmo.ToString();
    }
}
