using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SmokeTripFilter : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private Volume smokeTripVolume;
    [SerializeField] private float speed = 1.0f;
    [SerializeField, Range(0f, 1f)] private float intensity = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool turnOn = false;

    private LiftGammaGain liftGammaGain;
    private bool isHigh = false;

    private void Start()
    {
        if (smokeTripVolume != null && smokeTripVolume.profile != null)
        {
            smokeTripVolume.weight = 0f;
            smokeTripVolume.profile.TryGet(out liftGammaGain);
        }

        if (liftGammaGain == null)
            Debug.LogError("Kein Lift, Gamma, Gain in Volume");
    }

    private void Update()
    {
        if (!turnOn || (liftGammaGain == null && !isHigh)) return;

        //Calculate the angle with the time
        float angle = Time.time * speed;

        //Calculates the cirular color shift
        float r = Mathf.Sin(angle) * intensity;
        float g = Mathf.Sin(angle + (Mathf.PI / 4f)) * intensity;
        float b = Mathf.Sin(angle + (2f * Mathf.PI / 4f)) * intensity;

        liftGammaGain.gamma.value = new Vector4(r, g, b, 0f);
    }

    public void SetIsHigh(bool state)
    {
        isHigh = state;
        smokeTripVolume.weight = 1f;
    }
}
