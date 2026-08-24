using System.Collections.Generic;
using UnityEngine;
using WeaponNamespace;

public class WeaponController : MonoBehaviour
{
    public WeaponData WeaponData { get; private set; }
    public Transform LeftHandTarget { get; private set; }
    public GripType LeftHandGripType { get; private set; }
    public float LeftHandGripDistance { get; private set; }

    public void Initialize(WeaponData weaponData)
    {
        WeaponData = weaponData;

        CalculateLeftHandTarget();
    }

    void CalculateLeftHandTarget()
    {
        Dictionary<ModuleType, ModuleBase> modules = WeaponData.Modules;

        ModuleBase body = modules[ModuleType.Body];

        float currentScore = 0;
        LeftHandTarget = body.HandPlacement;
        LeftHandGripType = body.GripType;

        foreach (ModuleBase module in modules.Values)
        {
            if (!module.HasHandPlacement)
            {
                continue;
            }

            float bodyX = body.transform.position.x;
            float moduleX = module.HandPlacement.position.x;
            float divFactor = module.ModuleType == ModuleType.Body ? 18f : 6f;

            float dst = Mathf.Abs(bodyX - moduleX) / divFactor;

            float score = module.PlacementBias - dst;

            if (score > currentScore)
            {
                LeftHandTarget = module.HandPlacement;
                LeftHandGripType = module.GripType;
                LeftHandGripDistance = dst;
                currentScore = score;
            }
        }
    }
}
