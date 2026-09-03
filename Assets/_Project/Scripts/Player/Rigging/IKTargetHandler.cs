using System;
using UnityEngine;

public class IKTargetHandler : MonoBehaviour
{
    [SerializeField] private Transform ikTarget;

    [SerializeField] private float dipAmount = 0.06f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeInOutQuad;

    [SerializeField] private float jitterFrequency = 20f;
    [SerializeField] private float jitterStrength = 0.003f;

    private Transform activeHandTarget;
    private Transform previousHandTarget;
    private LTDescr activeTween;
    private bool isTransitioning;

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponTransform += Event_WeaponTransform;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponTransform -= Event_WeaponTransform;
    }

    private void Event_WeaponTransform(WeaponController weaponController) => SetHandTarget(weaponController.LeftHandTarget);

    private void Update()
    {
        if (activeHandTarget == null || isTransitioning)
        {
            return;
        }

        SnapToSockets();
    }

    private void SnapToSockets()
    {
        if (activeHandTarget != null && ikTarget != null)
        {
            ikTarget.SetPositionAndRotation(activeHandTarget.position, activeHandTarget.rotation);
        }
    }

    public void SetHandTarget(Transform target, float duration = -1f, Action onComplete = null)
    {
        if (activeTween != null)
        {
            LeanTween.cancel(activeTween.uniqueId);
            activeTween = null;
        }

        previousHandTarget = activeHandTarget;
        activeHandTarget = target;

        if (activeHandTarget == null)
        {
            isTransitioning = false;
            return;
        }

        if (duration <= 0f)
        {
            isTransitioning = false;
            SnapToSockets();
            return;
        }

        Vector3 fallbackStartPos = ikTarget != null ? ikTarget.position : activeHandTarget.position;
        Quaternion fallbackStartRot = ikTarget != null ? ikTarget.rotation : activeHandTarget.rotation;

        isTransitioning = true;

        activeTween = LeanTween.value(gameObject, 0f, 1f, duration)
            .setEase(easeType)
            .setOnUpdate((float progress) =>
            {
                if (activeHandTarget == null || ikTarget == null) return;

                Vector3 fromPos = previousHandTarget != null ? previousHandTarget.position : fallbackStartPos;
                Quaternion fromRot = previousHandTarget != null ? previousHandTarget.rotation : fallbackStartRot;

                Vector3 basePos = Vector3.Lerp(fromPos, activeHandTarget.position, progress);
                Quaternion baseRot = Quaternion.Slerp(fromRot, activeHandTarget.rotation, progress);

                float dip = Mathf.Sin(progress * Mathf.PI) * dipAmount;
                Vector3 arcOffset = Vector3.down * dip;

                float noiseMask = Mathf.Sin(progress * Mathf.PI);
                float noiseX = (Mathf.PerlinNoise(Time.time * jitterFrequency, 0f) - 0.5f) * 2f;
                float noiseY = (Mathf.PerlinNoise(0f, Time.time * jitterFrequency) - 0.5f) * 2f;
                float noiseZ = (Mathf.PerlinNoise(Time.time * jitterFrequency, Time.time * jitterFrequency) - 0.5f) * 2f;
                Vector3 jitterOffset = new Vector3(noiseX, noiseY, noiseZ) * (jitterStrength * noiseMask);

                ikTarget.SetPositionAndRotation(basePos + arcOffset + jitterOffset, baseRot);
            })
            .setOnComplete(() =>
            {
                isTransitioning = false;
                SnapToSockets();
                activeTween = null;
                onComplete?.Invoke();
            });
    }
}