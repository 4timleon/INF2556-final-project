using System;
using UnityEngine;

public class IKDisplacementHandler : MonoBehaviour
{
    [SerializeField] private Vector3 baseOffset;
    [SerializeField] private float maxZ;
    [SerializeField] private float minZ;
    [SerializeField] private Transform displacementTarget;

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponTransform += Event_WeaponTransform;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponTransform -= Event_WeaponTransform;
    }

    private void Event_WeaponTransform(WeaponController weaponController)
    {
        float zShift = Mathf.Clamp((-1f) * weaponController.LeftHandGripDistance, minZ, maxZ);

        displacementTarget.localPosition = new Vector3(baseOffset.x, baseOffset.y, zShift);
    }
}
