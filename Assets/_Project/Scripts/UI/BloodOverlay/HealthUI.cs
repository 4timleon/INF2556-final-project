using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image bloodImage;
    [SerializeField] private float maxAlpha = 0.85f;

    private void OnEnable()
    {
        GameEventSystem.OnPlayerHealthChanged += UpdateBlood;
    }

    private void OnDisable()
    {
        GameEventSystem.OnPlayerHealthChanged -= UpdateBlood;
    }

    private void UpdateBlood(int current, int max)
    {
        float healthPercent = (float)current / max;
        float intensity = (1f - healthPercent) * maxAlpha;

        Color c = bloodImage.color;
        c.a = intensity;
        bloodImage.color = c;
    }
}