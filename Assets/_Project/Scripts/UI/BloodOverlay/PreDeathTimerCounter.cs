using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreDeathTimerCounter : MonoBehaviour {
    [SerializeField] private Image imageFill;
    [SerializeField] private TextMeshProUGUI counterTextMesh;
    [SerializeField] private GameObject container;

    private void OnEnable() {
        container.SetActive(false);
        Player.OnPreDeathCounter += StartCounter;
    }

    private void OnDisable() {
        Player.OnPreDeathCounter -= StartCounter;

        LeanTween.cancel(gameObject);
    }

    private void StartCounter(float totalDuration) {
        container.SetActive(true);

        LeanTween.cancel(gameObject);

        imageFill.fillAmount = 0f;

        counterTextMesh.text = Mathf.CeilToInt(totalDuration).ToString();

        LeanTween.value(gameObject, 0f, 1f, totalDuration)
            .setOnUpdate((float value) => {
                imageFill.fillAmount = value;

                float remainingTime = value * totalDuration;

                counterTextMesh.text = Mathf.CeilToInt(remainingTime).ToString();
            })
            .setEase(LeanTweenType.linear)
            .setIgnoreTimeScale(true)
            .setOnComplete(() => {
                container.SetActive(false);
                GameEventSystem.OnPlayerPreDeath?.Invoke(false);
            });
    }
}
