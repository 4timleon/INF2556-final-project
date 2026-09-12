using UnityEngine;
using UnityEngine.Animations.Rigging;
using EZCameraShake;
using Random = UnityEngine.Random;

public class RecoilController : MonoBehaviour
{
    [SerializeField] private Rig rotationRig;
    [SerializeField] private Rig positionRig;

    [SerializeField] private float snappiness = 50f;
    [SerializeField] private float returnSpeed = 10f;

    private float targetPosWeight;
    private float targetRotWeight;

    public void ApplyRigRecoil(float positionStrength, float rotationStrength)
    {
        if (positionStrength == 0 && rotationStrength == 0) return;

        targetPosWeight = Mathf.Clamp01(targetPosWeight + positionStrength);
        targetRotWeight = Mathf.Clamp01(targetRotWeight + rotationStrength);

        CameraShaker.Instance.ShakeOnce(rotationStrength, rotationStrength, .1f, .5f);
    }

    public void ApplyCameraRecoil(float positionStrength, float rotationStrength)
    {
        if (positionStrength == 0 && rotationStrength == 0) return;

        float recoilPitch = rotationStrength / 5f;
        float recoilYaw = Random.Range(-1f, 1f) * (rotationStrength / 5f);

        GameEventSystem.OnPlayerWeaponRecoil?.Invoke(recoilPitch, recoilYaw);
    }

    void Update()
    {
        targetPosWeight = Mathf.Lerp(targetPosWeight, 0f, returnSpeed * Time.deltaTime);
        targetRotWeight = Mathf.Lerp(targetRotWeight, 0f, returnSpeed * Time.deltaTime);

        positionRig.weight = Mathf.Lerp(positionRig.weight, targetPosWeight, snappiness * Time.deltaTime);
        rotationRig.weight = Mathf.Lerp(rotationRig.weight, targetRotWeight, snappiness * Time.deltaTime);

        if (positionRig.weight < 0.001f && targetPosWeight < 0.001f) positionRig.weight = 0f;
        if (rotationRig.weight < 0.001f && targetRotWeight < 0.001f) rotationRig.weight = 0f;
    }
}