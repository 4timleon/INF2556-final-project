using System;

namespace WeaponNamespace
{
    public enum JointSize
    {
        None = 0,         // 0000 0000
        Micro = 1 << 0,   // 0000 0001
        Small = 1 << 1,   // 0000 0010
        Medium = 1 << 2,  // 0000 0100
        Large = 1 << 3,   // 0000 1000
        Oversized = 1 << 4// 0001 0000
    }

    [Flags]
    public enum JointSizeFilter
    {
        None = 0,         // 0000 0000
        Micro = 1 << 0,   // 0000 0001
        Small = 1 << 1,   // 0000 0010
        Medium = 1 << 2,  // 0000 0100
        Large = 1 << 3,   // 0000 1000
        Oversized = 1 << 4// 0001 0000
    }

    public enum ModuleType
    {
        Body,
        Barrel,
        Handguard,
        Stock,
        Magazine,
        Grip,
        Muzzle,
        Sight,
        DownAttachment,
        SideAttachment
    }

    public enum WeaponType
    {
        AssaultRifle,
        SubmachineGun,
        SniperRifle,
        Pistol,
        Shotgun
    }

    public enum Rarity
    {
        Common,
        Rare,
        Legendary,
        Undead
    }
}