using System;
using EditorAttributes;
using UnityEngine;

public abstract class PerkBase : MonoBehaviour, IInteractable
{
    [Title("General:", 14)]
    [SerializeField] protected string perkName;
    [SerializeField] protected PerkType perkType;
    [SerializeField] protected int cost;
    [Space]
    [Title("Materials:", 14)]
    [SerializeField] protected Material poweredMaterial;
    [SerializeField] protected Material unpoweredMaterial;
    [SerializeField] protected MeshRenderer mainMeshRenderer;
    [Space]
    [Title("References:", 14)]
    [SerializeField] protected PlayerSmoke playerSmoke;
    [Space]
    [Header("Sound")]
    [SerializeField] private AudioSource smokeAudioSource;
    [SerializeField] private AudioSource perkTakeAudioSource;
    [SerializeField] private AudioSource machineHumAudioSource;

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

    void Event_PowerChanged(bool state)
    {
        HasPower = state;

        mainMeshRenderer.sharedMaterial = state ? poweredMaterial : unpoweredMaterial;

        if (!HasPower)
        {
            machineHumAudioSource.Stop();
        }
        else
        {
            SoundManager.Instance?.PlayMachineHumSound(machineHumAudioSource);
        }

    }

    void Event_PlayerPreDeath(bool state)
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
            playerSmoke.SmokeJoint(ApplyPerk, smokeAudioSource);
            WasConsumed = true;
            GameData.Money -= cost;
            GameEventSystem.OnPerkAquired?.Invoke(perkType);

            if (perkTakeAudioSource.isPlaying)
            {
                perkTakeAudioSource.Stop();
            }
            SoundManager.Instance?.PlayPerkTakeSound(perkTakeAudioSource);
        }
    }

    protected abstract void ApplyPerk();
    protected abstract void RevertPerk();
}
