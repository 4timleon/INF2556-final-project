using System.Collections;
using TMPro;
using UnityEngine;

public class TitleEffekt : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI[] ghostTitles;
    [SerializeField] private float delay = 0.1f;
    [SerializeField] private float minAlphaRange = 0.6f;
    [SerializeField] private float maxAlphaRange = 0.8f;

    private void OnEnable() {
        StartCoroutine(StartFlicker());
    }

    private void OnDisable() {
        StopCoroutine(StartFlicker());
    }

    private IEnumerator StartFlicker() {
        while (true) {
            float randomAlpha = Random.Range(minAlphaRange, maxAlphaRange);

            foreach (var ghostTitle in ghostTitles) {
                ghostTitle.alpha = randomAlpha;
            }

            yield return new WaitForSeconds(delay);
        }
    }
}
