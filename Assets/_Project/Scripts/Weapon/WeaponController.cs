using System.Collections.Generic;
using UnityEngine;
using WeaponNamespace;

public class WeaponController : MonoBehaviour
{
    public WeaponData WeaponData { get; private set; }

    public int CurrentAmmo { get; set; }

    public bool IsReloading { get; set; }

    public Transform LeftHandTarget { get; private set; }
    public GripType LeftHandGripType { get; private set; }
    public float LeftHandGripDistance { get; private set; }
    public ReloadBase ReloadBase { get; private set; }

    public Bounds VisualBounds { get; private set; }

    public void Initialize(WeaponData weaponData)
    {
        WeaponData = weaponData;

        ReloadBase = ((BodyModule)WeaponData.Modules[ModuleType.Body]).ReloadBase;

        CurrentAmmo = (int)WeaponData.Stats[StatType.MagazineSize];

        CalculateLeftHandTarget();
        UpdateVisualBounds();
    }

    public ModuleBase GetModule(ModuleType moduleType)
    {
        if (WeaponData.Modules.TryGetValue(moduleType, out var value))
        {
            return value;
        }

        return null;
    }

    public bool TryGetModule(ModuleType moduleType, out ModuleBase moduleBase)
    {
        if (WeaponData.Modules.TryGetValue(moduleType, out moduleBase))
        {
            return true;
        }

        return false;
    }

    public float GetStat(StatType statType)
    {
        if (WeaponData.Stats.TryGetValue(statType, out float value))
        {
            return value;
        }

        return 1;
    }

    public void CenterOnSocket(Transform targetSocket)
    {
        transform.SetParent(null);
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;

        Bounds rawBounds = UpdateVisualBounds();
        Vector3 weaponSize = rawBounds.size;

        Vector3 targetSize = targetSocket.lossyScale;

        float scaleX = weaponSize.x > 0 ? targetSize.x / weaponSize.x : 1f;
        float scaleY = weaponSize.y > 0 ? targetSize.y / weaponSize.y : 1f;
        float scaleZ = weaponSize.z > 0 ? targetSize.z / weaponSize.z : 1f;

        float scaleFactor = Mathf.Min(scaleX, Mathf.Min(scaleY, scaleZ));

        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        transform.rotation = targetSocket.rotation;

        Bounds finalBounds = UpdateVisualBounds();

        Vector3 centerOffset = finalBounds.center - transform.position;
        transform.position = targetSocket.position - centerOffset;

        transform.SetParent(targetSocket, true);
    }

    Bounds UpdateVisualBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        bool hasBounds = false;

        foreach (Renderer rend in renderers)
        {
            if (!rend.enabled || rend is ParticleSystemRenderer)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = rend.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(rend.bounds);
            }
        }

        VisualBounds = bounds;
        return VisualBounds;
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