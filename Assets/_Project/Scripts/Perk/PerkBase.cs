using System;
using EditorAttributes;
using UnityEngine;

public abstract class PerkBase : MonoBehaviour, IInteractable
{
    [Title("General:", 14)]
    [SerializeField] protected string perkName;
    [SerializeField] protected int cost;
    [Space]
    [Title("Materials:", 14)]
    [SerializeField] protected Material poweredMaterial;
    [SerializeField] protected Material unpoweredMaterial;
    [SerializeField] protected MeshRenderer mainMeshRenderer;
    [Space]
    [Title("References:", 14)]
    [SerializeField] protected PlayerSmoke playerSmoke;

    protected bool HasPower { get; private set; } = false;
    protected bool WasConsumed { get; private set; } = false;

    void Awake()
    {
        mainMeshRenderer.sharedMaterial = unpoweredMaterial;
    }

    void OnEnable()
    {
        GameEventSystem.OnPowerChanged += Event_PowerChanged;
        GameEventSystem.OnPlayerPreDeath += Event_PlayerPreDeath;
    }

    void OnDisable()
    {
        GameEventSystem.OnPowerChanged -= Event_PowerChanged;
        GameEventSystem.OnPlayerPreDeath -= Event_PlayerPreDeath;
    }

    void Event_PowerChanged(bool obj)
    {
        HasPower = obj;

        mainMeshRenderer.sharedMaterial = obj ? poweredMaterial : unpoweredMaterial;
    }

    void Event_PlayerPreDeath(bool obj)
    {
        RevertPerk();
        WasConsumed = false;
    }

    public string GetText()
    {
        if (WasConsumed) return "Perk already consumed";
        if (!HasPower) return "Power must be on";

        return $"Press <sprite name=\"keyboard_e\"> to smoke {perkName} | {cost}$";
    }

    public void OnInteractStarted()
    {
        if (!WasConsumed && GameData.Money >= cost && HasPower)
        {
            playerSmoke.SmokeJoint(ApplyPerk);
            WasConsumed = true;
        }
    }

    protected abstract void ApplyPerk();
    protected abstract void RevertPerk();
}
