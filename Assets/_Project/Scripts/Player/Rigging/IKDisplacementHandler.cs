using System;
using UnityEngine;

public class IKDisplacementHandler : MonoBehaviour
{
    [SerializeField] private Vector3 idleOffset;
    [SerializeField] private float maxIdleZ;
    [SerializeField] private float minIdleZ;
    [Space]
    [SerializeField] private Vector3 adsOffset;
    [SerializeField] private float maxAimY;
    [SerializeField] private float minAimY;
    [Space]
    [SerializeField] private Transform displacementTarget;
    [SerializeField] private Transform player;

    private Vector3 idlePosition;
    private Vector3 aimPosition;

    private LTDescr aimTween;

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponTransform += Event_WeaponTransform;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponTransform -= Event_WeaponTransform;
    }

    void Event_WeaponTransform(WeaponController weaponController)
    {
        float idleZ = Mathf.Clamp((-1f) * weaponController.LeftHandGripDistance, minIdleZ, maxIdleZ);
        float aimY = Mathf.Clamp(idleOffset.y + weaponController.HighestModuleDistance, minAimY, maxAimY);
        float aimZ = idleZ > ((minIdleZ / 2f) + 0.05f) ? adsOffset.z / 2f : adsOffset.z;

        idlePosition = new Vector3(idleOffset.x, idleOffset.y, idleZ);
        aimPosition = new Vector3(adsOffset.x, aimY, aimZ);

        displacementTarget.localPosition = idlePosition;
    }

    public void StartADS(float adsSpeed)
    {
        LeanTween.cancel(displacementTarget.gameObject);

        aimTween = LeanTween.moveLocal(displacementTarget.gameObject, aimPosition, adsSpeed);
    }

    public void EndADS(float adsSpeed)
    {
        LeanTween.cancel(displacementTarget.gameObject);

        aimTween = LeanTween.moveLocal(displacementTarget.gameObject, idlePosition, adsSpeed);
    }
}
