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
        transform.position = targetSocket.position;
        transform.rotation = targetSocket.rotation;

        Bounds bounds = UpdateVisualBounds();

        Vector3 centerOffset = bounds.center - transform.position;

        transform.position = targetSocket.position - centerOffset;
        transform.SetParent(targetSocket, true);
    }

    Bounds UpdateVisualBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            VisualBounds = new Bounds(transform.position, Vector3.zero);
            return VisualBounds;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            if (!renderers[i].enabled || renderers[i] is ParticleSystemRenderer)
            {
                continue;
            }

            bounds.Encapsulate(renderers[i].bounds);
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
