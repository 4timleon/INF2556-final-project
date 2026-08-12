using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private Animator backgroundAnimator;
    [SerializeField] private int tallyThreshold = 5; // ab dieser Welle: Zahl statt Striche

    private void OnEnable()
    {
        GameEventSystem.OnWaveStarted += HandleWaveStarted;
    }

    private void OnDisable()
    {
        GameEventSystem.OnWaveStarted -= HandleWaveStarted;
    }

    private void Start()
    {
        UpdateWaveNumber();
    }

    private void HandleWaveStarted()
    {
        UpdateWaveNumber();

        if (backgroundAnimator != null)
            backgroundAnimator.SetTrigger("PlaySplatter");
    }

    private void UpdateWaveNumber()
    {
        int wave = GameData.Wave;

        if (wave <= tallyThreshold)
        {
            waveNumberText.text = BuildTallyString(wave);
        }
        else
        {
            waveNumberText.text = wave.ToString();
        }
    }

    private string BuildTallyString(int count)
    {
        return string.Join(" ", System.Linq.Enumerable.Repeat("I", count));
    }
}