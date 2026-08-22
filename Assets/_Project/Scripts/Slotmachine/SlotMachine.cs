using System;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    [System.Serializable]
    public class SlotOutcome
    {
        public SlotResultType resultType;
        public string outcomeName;
        [Tooltip("Relative Chance für diesen Ausgang (z.B. Niete = 50, Waffe = 5)")]
        public float weight = 10f;
        public Sprite reelIcon;
    }

    public struct SpinVisualData
    {
        public Sprite[] reelSymbols;
        public SlotOutcome winningOutcome;
    }

    [Header("Basic Settings")]
    [SerializeField] private int spinCost = 1000;
    [SerializeField] private int maxUsesBeforeMove = 5;
    private int currentUses = 0;

    [Header("Outcomes & Probabilities")]
    [SerializeField] private List<SlotOutcome> possibleOutcomes = new List<SlotOutcome>();

    [Header("Outcome Parameters")]
    [SerializeField] private List<WeaponItemData> specialWeapons = new List<WeaponItemData>();
    [SerializeField] private List<PerkData> availablePerks = new List<PerkData>();
    [SerializeField] private int minPointGain = 1500;
    [SerializeField] private int maxPointGain = 3000;
    [SerializeField] private float explosionDamage = 35f;

    [Header("Locations")]
    [SerializeField] private List<Transform> possibleLocations = new List<Transform>();
    private int currentLocationIndex = 0;

    public static event Action OnSpinStarted;
    public static event Action<SpinVisualData> OnSpinCompleted;

    private bool isSpinning = false;

    public bool TrySpin(GameObject player)
    {
        if (isSpinning) return false;

        if (!ScoreManager.Instance.SpendScore(spinCost))
        {
            return false;
        }

        isSpinning = true;
        currentUses++;
        OnSpinStarted?.Invoke();

        SlotOutcome outcome = GetRandomOutcome();
        SpinVisualData visualData = GenerateReelSymbols(outcome);
        ExecuteOutcome(outcome, player);
        OnSpinCompleted?.Invoke(visualData);

        isSpinning = false;

        if (currentUses >= maxUsesBeforeMove)
        {
            MoveToNextLocation();
        }

        return true;
    }

    private SlotOutcome GetRandomOutcome()
    {
        float totalWeight = 0f;
        foreach (var o in possibleOutcomes) totalWeight += o.weight;

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        // TODO erstes passendes outcome wird zurückgegeben, deshalb wahrscheinlich lieber liste random sortieren bevor man auswählt
        foreach (var o in possibleOutcomes)
        {
            cumulative += o.weight;
            if (randomValue <= cumulative) return o;
        }

        return possibleOutcomes[0];
    }

    private SpinVisualData GenerateReelSymbols(SlotOutcome outcome)
    {
        SpinVisualData data = new SpinVisualData();
        data.reelSymbols = new Sprite[4];

        if (outcome.resultType != SlotResultType.NoWin)
        {
            data.winningOutcome = outcome;
            for (int i = 0; i < 4; i++)
            {
                data.reelSymbols[i] = outcome.reelIcon;
            }
        }
        else
        {
            data.winningOutcome = null;

            for (int i = 0; i < 4; i++)
            {
                SlotOutcome randomOutcome = possibleOutcomes[UnityEngine.Random.Range(0, possibleOutcomes.Count)];
                data.reelSymbols[i] = randomOutcome.reelIcon;
            }

            if (data.reelSymbols[0] == data.reelSymbols[1] && data.reelSymbols[1] == data.reelSymbols[2] && data.reelSymbols[2] == data.reelSymbols[3])
            {
                List<SlotOutcome> alternatives = possibleOutcomes.FindAll(o => o.reelIcon != data.reelSymbols[0]);
                if (alternatives.Count > 0)
                {
                    data.reelSymbols[3] = alternatives[UnityEngine.Random.Range(0, alternatives.Count)].reelIcon;
                }
            }
        }

        return data;
    }

    private void ExecuteOutcome(SlotOutcome outcome, GameObject player)
    {
        switch (outcome.resultType)
        {
            case SlotResultType.SpecialWeapon:
                if (specialWeapons.Count > 0)
                {
                    WeaponItemData randomWeapon = specialWeapons[UnityEngine.Random.Range(0, specialWeapons.Count)];
                    // TODO dem spieler eher die wahl geben die waffe zu equippen und nicht direkt die vorhandene waffe durch die neue ersetzen
                    player.GetComponent<PlayerWeaponHandler>()?.EquipWeapon(randomWeapon);
                }
                break;

            case SlotResultType.PointGain:
                int pointsWon = UnityEngine.Random.Range(minPointGain, maxPointGain + 1);
                ScoreManager.Instance.AddScore(pointsWon);
                break;

            case SlotResultType.NoWin:
                break;

            case SlotResultType.Explosion:
                if (player.TryGetComponent<PlayerHealth>(out var health))
                {
                    health.TakeDamage(explosionDamage);
                }
                break;

            case SlotResultType.RandomPerk:
                if (player.TryGetComponent<PlayerPerkManager>(out var perkManager) && availablePerks.Count > 0)
                {
                    PerkData randomPerk = availablePerks[UnityEngine.Random.Range(0, availablePerks.Count)];
                    perkManager.UnlockPerk(randomPerk);
                }
                break;
        }
    }

    private void MoveToNextLocation()
    {
        if (possibleLocations.Count <= 1) return;

        currentUses = 0;
        currentLocationIndex = (currentLocationIndex + 1) % possibleLocations.Count;
        
        transform.position = possibleLocations[currentLocationIndex].position;
        transform.rotation = possibleLocations[currentLocationIndex].rotation;
    }
}