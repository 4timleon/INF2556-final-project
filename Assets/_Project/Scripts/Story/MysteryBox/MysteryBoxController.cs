using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MysteryBoxController : MonoBehaviour
{
    [Serializable]
    private class WeaponWeightEntry
    {
        public WeaponType type;
        public float weight = 10f;
        public bool hasModules = true;
    }

    [Header("Animation")]
    [SerializeField] private Animator boxAnimator;

    [Header("Settings")]
    [SerializeField] private int cost = 950;
    [SerializeField] private float cooldown = 10f;

    [Header("Weapon Type Weights")]
    [Tooltip("Waffen ohne Module (z.B. Granate) sollten hoehere Gewichte bekommen")]
    [SerializeField]
    private List<WeaponWeightEntry> weaponWeights = new List<WeaponWeightEntry>
    {
        new WeaponWeightEntry { type = WeaponType.Shotgun, weight = 10f, hasModules = true },
        new WeaponWeightEntry { type = WeaponType.SMG, weight = 10f, hasModules = true },
        new WeaponWeightEntry { type = WeaponType.Sturmgewehr, weight = 10f, hasModules = true },
        new WeaponWeightEntry { type = WeaponType.Sniper, weight = 10f, hasModules = true },
        new WeaponWeightEntry { type = WeaponType.Pistole, weight = 10f, hasModules = true },
        new WeaponWeightEntry { type = WeaponType.RPG, weight = 25f, hasModules = false },
        new WeaponWeightEntry { type = WeaponType.Granate, weight = 25f, hasModules = false },
        new WeaponWeightEntry { type = WeaponType.ChiliCheeseBurgerBait, weight = 25f, hasModules = false },
    };

    [Header("Location Change")]
    [SerializeField] private Transform[] spawnPoints;
    [Range(0f, 1f)]
    [SerializeField] private float teleportChance = 0.3f; // 30%

    [Header("Module Rarity Weights")]
    [Tooltip("Index 0 = Wahrscheinlichkeit fuer 0 Module, Index 10 = Wahrscheinlichkeit fuer alle 10 Module")]
    [SerializeField]
    private float[] moduleCountWeights = new float[]
    {
        30f, 20f, 15f, 10f, 8f, 6f, 4f, 3f, 2f, 1.5f, 0.5f
    };

    private bool playerInRange;
    private float cooldownTimer;

    private static readonly ModuleType[] AllModules = (ModuleType[])Enum.GetValues(typeof(ModuleType));

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (!playerInRange || cooldownTimer > 0f) return;

        if (GameControls.GetInputActions().Player.Interact.WasPressedThisFrame())
        {
            TryUseBox();
        }

        if (GameControls.GetInputActions().Player.Interact.WasPressedThisFrame())
        {
            Debug.Log($"Interact gedrückt. Geld: {GameData.Money}, Kosten: {cost}, InRange: {playerInRange}");
            TryUseBox();
        }
    }

    private void TryUseBox()
    {
        if (GameData.Money < cost) return;

        GameData.Money -= cost;
        cooldownTimer = cooldown;

        WeaponWeightEntry rolledWeapon = RollWeaponType();
        List<ModuleType> rolledModules = rolledWeapon.hasModules ? RollModules() : new List<ModuleType>();

        GameEventSystem.OnWeaponWon?.Invoke(rolledWeapon.type, rolledModules);

        if (boxAnimator != null)
            boxAnimator.SetTrigger("UseBox");

        TryTeleportToNewLocation();
    }

    private WeaponWeightEntry RollWeaponType()
    {
        float totalWeight = 0f;
        foreach (var entry in weaponWeights) totalWeight += entry.weight;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in weaponWeights)
        {
            cumulative += entry.weight;
            if (roll <= cumulative) return entry;
        }

        return weaponWeights[0]; // Fallback
    }

    private List<ModuleType> RollModules()
    {
        int count = RollModuleCount();

        List<ModuleType> pool = new List<ModuleType>(AllModules);
        List<ModuleType> result = new List<ModuleType>();

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    private int RollModuleCount()
    {
        float totalWeight = 0f;
        foreach (float w in moduleCountWeights) totalWeight += w;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < moduleCountWeights.Length; i++)
        {
            cumulative += moduleCountWeights[i];
            if (roll <= cumulative) return i;
        }

        return 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void TryTeleportToNewLocation()
    {
        if (spawnPoints == null || spawnPoints.Length <= 1) return; // nichts zum Wechseln da

        if (Random.value > teleportChance) return; // kein Teleport diesmal

        Transform newSpot;
        do
        {
            newSpot = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }
        while (newSpot.position == transform.position); // nicht am gleichen Punkt nochmal landen

        transform.position = newSpot.position;
        transform.rotation = newSpot.rotation;

        // TODO: hier evtl. eine "Box verschwindet/erscheint" Animation/Sound triggern
    }
}