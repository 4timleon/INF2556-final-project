using System;
using System.Collections;
using EditorAttributes;
using UnityEngine;
using WeaponNamespace;
using Random = UnityEngine.Random;

public class PlayerShoot : MonoBehaviour
{
    [Title("References:", 14)]
    [SerializeField] private RecoilController recoilController;
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private Camera playerCamera;
    [Space]
    [Title("Raycast:", 14)]
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float maxRange = 100f;
    [Space]
    [Title("Tracer", 14)]
    [SerializeField] private GameObject tracerPrefab;
    [SerializeField] private float tracerSpeed = 250f;

    private WeaponController weaponController;

    private float roundsPerSecond;
    private int bulletAmount;
    private float damage;
    private float recoil;
    private float accuracy;
    private float spread;

    private bool isPerformingAttack;
    private float nextAttackTimestamp;
    private bool isAuto;

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster += Event_WeaponHolster;
        GameEventSystem.OnPlayerAttackStarted += Event_AttackStarted;
        GameEventSystem.OnPlayerAttackEnded += Event_AttackEnded;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster -= Event_WeaponHolster;
        GameEventSystem.OnPlayerAttackStarted -= Event_AttackStarted;
        GameEventSystem.OnPlayerAttackEnded -= Event_AttackEnded;
    }

    void Event_WeaponDraw(WeaponController weaponController)
    {
        this.weaponController = weaponController;

        nextAttackTimestamp = Time.time;

        roundsPerSecond = weaponController.GetStat(StatType.Firerate);
        bulletAmount = (int)weaponController.GetStat(StatType.BulletAmount);
        damage = weaponController.GetStat(StatType.Damage);
        recoil = 1f - (weaponController.GetStat(StatType.Handling) / 100f);
        accuracy = 1f - (weaponController.GetStat(StatType.Accuracy) / 100f);
        spread = accuracy / 2f;

        isAuto = IsAutoFire();
    }

    void Event_WeaponHolster() => weaponController = null;

    void Event_AttackStarted()
    {
        if (weaponController == null || weaponController.IsReloading) return;

        if (isAuto)
        {
            isPerformingAttack = true;
        }
        else
        {
            if (Time.time >= nextAttackTimestamp)
            {
                Shoot();
                nextAttackTimestamp = Time.time + (1f / roundsPerSecond);
            }
        }
    }

    void Event_AttackEnded() => isPerformingAttack = false;

    void Update()
    {
        if (weaponController == null)
        {
            return;
        }

        spread = playerInputs.IsMoving ? accuracy : accuracy / 2f;

        if (!isPerformingAttack)
        {
            return;
        }

        if (weaponController.IsReloading)
        {
            isPerformingAttack = false;
            return;
        }

        if (Time.time >= nextAttackTimestamp)
        {
            float interval = 1f / roundsPerSecond;

            if (Time.time > nextAttackTimestamp + interval)
            {
                nextAttackTimestamp = Time.time;
            }

            nextAttackTimestamp += interval;

            Shoot();
        }
    }

    void Shoot()
    {
        if (weaponController.CurrentAmmo <= 0)
        {
            GameEventSystem.OnPlayerReloadStarted?.Invoke();
            return;
        }

        recoilController.ApplyRigRecoil(recoil, recoil);
        recoilController.ApplyCameraRecoil(recoil, recoil);
        weaponController.CurrentAmmo--;

        for (int i = 0; i < bulletAmount; i++)
        {
            Vector2 spreadOffset = Random.insideUnitCircle * spread;
            Vector3 shootDirection = playerCamera.transform.forward +
                                     (playerCamera.transform.right * spreadOffset.x) +
                                     (playerCamera.transform.up * spreadOffset.y);

            shootDirection.Normalize();

            Vector3 targetPoint;

            if (Physics.Raycast(playerCamera.transform.position, shootDirection, out RaycastHit hit, maxRange, hitMask))
            {
                targetPoint = hit.point;

                if (hit.collider.TryGetComponent<BodyPart>(out var bodyPart))
                {
                    bodyPart.TakeDamage(damage);
                }
            }
            else
            {
                targetPoint = playerCamera.transform.position + (shootDirection * maxRange);
            }

            StartCoroutine(MoveTracer(targetPoint));
        }
    }

    //todo: durch pooling ersetzen (hatte kein bock)
    IEnumerator MoveTracer(Vector3 targetPoint)
    {
        Vector3 tracerStart;

        if (weaponController.TryGetModule(ModuleType.Muzzle, out var muzzleBase))
        {
            tracerStart = muzzleBase.transform.position;
        }
        else
        {
            tracerStart = weaponController.GetModule(ModuleType.Barrel).transform.position;
        }

        GameObject tracer = Instantiate(tracerPrefab, tracerStart, Quaternion.identity);

        while (tracer != null && Vector3.Distance(tracer.transform.position, targetPoint) > 0.1f)
        {
            tracer.transform.position = Vector3.MoveTowards(tracer.transform.position, targetPoint, tracerSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(tracer);
    }

    bool IsAutoFire()
    {
        WeaponType weaponType = weaponController.WeaponData.TotalType;

        return weaponType == WeaponType.AssaultRifle || weaponType == WeaponType.SubmachineGun;
    }
}
