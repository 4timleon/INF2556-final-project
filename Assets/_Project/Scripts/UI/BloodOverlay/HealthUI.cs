using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour {
    [SerializeField] private Image bloodImage;
    [SerializeField] private float maxAlpha = 0.85f;

    private void OnEnable() {
        GameEventSystem.OnPlayerHealthChanged += UpdateBlood;
        GameEventSystem.OnPlayerPreDeath += PreDeathScreen;
    }

    private void OnDisable() {
        GameEventSystem.OnPlayerHealthChanged -= UpdateBlood;
        GameEventSystem.OnPlayerPreDeath -= PreDeathScreen;
    }

    private void UpdateBlood(float current, float max) {
        LeanTween.cancel(gameObject);

        float healthPercent = current / max;
        float intensity = (1f - healthPercent) * maxAlpha;

        LeanTween.value(gameObject, bloodImage.color.a, intensity, 0.25f)
            .setOnUpdate((float value) => {
                Color c = bloodImage.color;
                c.a = value;
                bloodImage.color = c;
            });
    }

    private void PreDeathScreen(bool state) {
        if (state) {
            bloodImage.color = new Color(0.3f, 0.3f, 0.3f);
        } else {
            bloodImage.color = new Color(1f, 1f, 1f);
        }
    }

}