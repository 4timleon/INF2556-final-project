using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class LoadingOverlayController : MonoBehaviour {
    [SerializeField] private Image _overlayImage;
    [SerializeField] private float _fadeDuration = 0.4f;

    public void SetInstant(bool visible) {
        Color color = _overlayImage.color;
        color.a = visible ? 1f : 0f;
        _overlayImage.color = color;

        _overlayImage.gameObject.SetActive(visible);
    }

    public async Awaitable FadeInAsync(CancellationToken ct) {
        _overlayImage.gameObject.SetActive(true);

        await FadeAlphaAsync(1f, ct);
    }

    public async Awaitable FadeOutAsync(CancellationToken ct) {
        await FadeAlphaAsync(0f, ct);

        _overlayImage.gameObject.SetActive(false);
    }

    private async Awaitable FadeAlphaAsync(float targetAlpha, CancellationToken ct) {
        float startAlpha = _overlayImage.color.a;
        float elapsedTime = 0f;

        ct.ThrowIfCancellationRequested();

        while (elapsedTime < _fadeDuration) {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / _fadeDuration);

            Color color = _overlayImage.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            _overlayImage.color = color;

            await Awaitable.NextFrameAsync(ct);
        }

        Color finalColor = _overlayImage.color;
        finalColor.a = targetAlpha;
        _overlayImage.color = finalColor;
    }
}
