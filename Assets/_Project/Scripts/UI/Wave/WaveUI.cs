using UnityEngine;
using TMPro;
using System.Collections;

public class WaveUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI waveNumberText;
    [SerializeField] private float animationDuration = 5f;
    [SerializeField] private AudioSource waveStartAudioSource;

    private void Start() {
        //Update Text
        waveNumberText.text = GameData.Wave.ToString();

        //Set Alpha to 0
        Color c = waveNumberText.color;
        c.a = 0f;
        waveNumberText.color = c;

        //Wait then Fade
        StartCoroutine(InitialWave(2f));
    }

    private void OnEnable() {
        GameEventSystem.OnWaveEnded += UpdateWaveNumber;
    }

    private void OnDisable() {
        GameEventSystem.OnWaveEnded -= UpdateWaveNumber;
    }

    private void UpdateWaveNumber() {
        int wave = GameData.Wave;

        if (wave > 1) {
            UpdateVisuals(wave);
        }
    }

    private void UpdateVisuals(int wave) {
        TextFadeOut(() => {
            waveNumberText.text = wave.ToString();

            TextFadeIn();
        });
    }

    private void TextFadeOut(System.Action onComplete) {
        LeanTween.cancel(waveNumberText.gameObject);

        LeanTween.value(waveNumberText.gameObject, waveNumberText.color.a, 0f, animationDuration)
            .setOnUpdate((float value) => {
                Color c = waveNumberText.color;
                c.a = value;
                waveNumberText.color = c;
            })
            .setOnComplete(onComplete);
    }

    private void TextFadeIn() {
        LeanTween.cancel(waveNumberText.gameObject);

        if (!waveStartAudioSource.isPlaying) {
            waveStartAudioSource.Play();
        }

        Color startColor = new Color(1f, 1f, 1f, 0f);
        Color targetColor = new Color(0.6f, 0f, 0f, 1f);

        LeanTween.value(waveNumberText.gameObject, startColor, targetColor, animationDuration)
            .setOnUpdate((Color value) => {
                waveNumberText.color = value;
            })
            .setOnComplete(() => {
                GameEventSystem.OnWaveStarted?.Invoke();
            });
    }

    private IEnumerator InitialWave(float duration) {
        yield return new WaitForSeconds(duration);

        TextFadeIn();
    }
}