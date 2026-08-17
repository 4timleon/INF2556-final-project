using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour {

    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundEffectSlider;
    [SerializeField] private TextMeshProUGUI masterText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private TextMeshProUGUI soundEffectText;

    private void Start() {
        masterText.text = (masterSlider.value * 100).ToString("F0");
        musicText.text = (musicSlider.value * 100).ToString("F0");
        soundEffectText.text = (soundEffectSlider.value * 100).ToString("F0");

        masterSlider.onValueChanged.AddListener(value => { masterText.text = (value * 100).ToString("F0"); });
        musicSlider.onValueChanged.AddListener(value => { musicText.text = (value * 100).ToString("F0"); });
        soundEffectSlider.onValueChanged.AddListener(value => { soundEffectText.text = (value * 100).ToString("F0"); });
    }

    private void OnDisable() {
        masterSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        soundEffectSlider.onValueChanged.RemoveAllListeners();
    }
}
