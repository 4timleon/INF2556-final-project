using UnityEngine;
using TMPro;
using System.Collections;

public class WaveUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI waveNumberText;

    public bool animationFinished { get; private set; } = false;

    private void OnEnable() {
        waveNumberText.text = GameData.Wave.ToString();

        GameEventSystem.OnWaveStarted += UpdateWaveNumber;
    }

    private void OnDisable() {
        GameEventSystem.OnWaveStarted -= UpdateWaveNumber;
    }

    private void UpdateWaveNumber() {
        int wave = GameData.Wave;

        UpdateVisuals(wave);
    }

    private void UpdateVisuals(int wave) {
        TextFadeOut(() => {
            waveNumberText.text = wave.ToString();

            TextFadeIn(() => {
                animationFinished = true;
            });
        });
    }

    private void TextFadeOut(System.Action onComplete) {
        LeanTween.cancel(waveNumberText.gameObject);

        LeanTween.value(waveNumberText.gameObject, waveNumberText.color.a, 0f, 3f)
            .setOnUpdate((float value) => {
                Color c = waveNumberText.color;
                c.a = value;
                waveNumberText.color = c;
            })
            .setOnComplete(onComplete);
    }

    private void TextFadeIn(System.Action onComplete = null) {
        LeanTween.cancel(waveNumberText.gameObject);

        Color startColor = new Color(1f, 1f, 1f, 0f);
        Color targetColor = new Color(0.6f, 0f, 0f, 1f);

        LeanTween.value(waveNumberText.gameObject, startColor, targetColor, 3f)
            .setOnUpdate((Color value) => {
                waveNumberText.color = value;
            })
            .setOnComplete(onComplete);
    }
}