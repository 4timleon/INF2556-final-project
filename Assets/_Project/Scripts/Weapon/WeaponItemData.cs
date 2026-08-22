using UnityEngine;

public enum WeaponType
{
    Pistol,
    SMG,
    Shotgun,
    AssaultRifle,
    Sniper,
    RPG
}

[CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Scriptable Objects/Shop/Weapon Item")]
public class WeaponItemData : ShopItemData
{
    [Header("Waffen Identifikation & Typ")]
    public WeaponType weaponType;
    public GameObject weaponPrefab;

    [Header("Schaden & Ballistik")]
    public float baseDamage = 20f;
    public float headshotMultiplier = 1.5f;
    public float fireRate = 0.1f;
    public float range = 50f;

    [Header("Munition & Magazin")]
    public int magazineCapacity = 30;
    public int maxReserveAmmo = 120;
    public float reloadTime = 2.5f;

    [Header("Handling & Precision")]
    public float recoilAmount = 1.0f;
    public float hipFireSpread = 2.0f;
    public float adsSpeed = 0.2f;

    [Header("Pack-a-Punch (Optional / Upgrades)")]
    public bool canBePackAPunched = true;
    public WeaponItemData packAPunchUpgrade; // Referenz auf die gepunchte Version dieser Waffe
}