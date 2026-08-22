using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public static event Action<int> OnScoreChanged; // aktueller punktestand
    public static event Action<int> OnPointsEarned; // für +10 usw. einblendungen wenn man punkte macht

    [Header("Score Stats")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int totalScoreEarned = 0;
    [SerializeField] private int totalKills = 0;

    public int CurrentScore => currentScore;
    public int TotalScoreEarned => totalScoreEarned;
    public int TotalKills => totalKills;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int points, bool isKill = false)
    {
        if (points <= 0) return;

        currentScore += points;
        totalScoreEarned += points;

        if (isKill)
        {
            totalKills++;
        }

        OnScoreChanged?.Invoke(currentScore);
        OnPointsEarned?.Invoke(points);
    }

    public bool SpendScore(int amount)
    {
        if (amount > currentScore)
        {
            return false;
        }

        currentScore -= amount;
        OnScoreChanged?.Invoke(currentScore);
        return true;
    }
}