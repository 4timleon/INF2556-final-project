using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingOverlayController : MonoBehaviour {
    [SerializeField] private Image _overlayImage;
    [SerializeField] private Slider _loadingBar;
    [SerializeField] private TextMeshProUGUI _percentText;
    [SerializeField] private float _fadeDuration = 0.4f;

    [SerializeField] private CanvasGroup _loadingBarCanvasGroup;

    public void SetInstant(bool visible) {
        Color color = _overlayImage.color;
        color.a = visible ? 1f : 0f;
        _overlayImage.color = color;

        _loadingBarCanvasGroup.alpha = visible ? 1f : 0f;

        _overlayImage.gameObject.SetActive(visible);
        _loadingBar.gameObject.SetActive(false);
        _percentText.gameObject.SetActive(false);
    }

    public void SetLoadingProgress(float progress) {
        float clampedProgress = Mathf.Clamp01(progress);

        if (_loadingBar != null) {
            _loadingBar.normalizedValue = clampedProgress;
        }

        if (_percentText != null) {
            _percentText.text = $"{clampedProgress:P0}";
        }
    }

    public async Awaitable FadeInAsync(CancellationToken ct) {
        _overlayImage.gameObject.SetActive(true);
        _loadingBar.gameObject.SetActive(true);
        _percentText.gameObject.SetActive(true);

        await FadeAlphaAsync(1f, ct);
    }

    public async Awaitable FadeOutAsync(CancellationToken ct) {
        await FadeAlphaAsync(0f, ct);

        _overlayImage.gameObject.SetActive(false);
        _loadingBar.gameObject.SetActive(false);
        _percentText.gameObject.SetActive(false);
    }

    private async Awaitable FadeAlphaAsync(float targetAlpha, CancellationToken ct) {
        float startAlpha = _overlayImage.color.a;
        float startLoadingBarAlpha = _loadingBarCanvasGroup.alpha;
        float elapsedTime = 0f;

        ct.ThrowIfCancellationRequested();

        while (elapsedTime < _fadeDuration) {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / _fadeDuration);

            //overlay alpha
            Color overlayColor = _overlayImage.color;
            overlayColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            _overlayImage.color = overlayColor;

            _loadingBarCanvasGroup.alpha = Mathf.Lerp(startLoadingBarAlpha, targetAlpha, t);

            await Awaitable.NextFrameAsync(ct);
        }

        Color finalColor = _overlayImage.color;
        finalColor.a = targetAlpha;
        _overlayImage.color = finalColor;
        _loadingBarCanvasGroup.alpha = targetAlpha;
    }

}
