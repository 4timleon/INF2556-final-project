using System.Collections;
using EditorAttributes;
using UnityEngine;
using WeaponNamespace;

[CreateAssetMenu(fileName = "NewBulletReload", menuName = "Weapon/Reload/Bullet")]
public class BulletReload : ReloadBase
{
    [Title("Bullet Prefabs", 14)]
    [SerializeField] private GameObject shotgunBullet;
    [SerializeField] private GameObject sniperBullet;
    [Space]
    [Title("Time Weights:", 14)]
    [Range(0f, 1f)][SerializeField] private float weightReachPouch = 0.35f;
    [Range(0f, 1f)][SerializeField] private float weightGrabPause = 0.10f;
    [Range(0f, 1f)][SerializeField] private float weightInsertShell = 0.45f;
    [Range(0f, 1f)][SerializeField] private float weightLockDelay = 0.10f;
    [Range(0f, 1f)][SerializeField] private float timingVariance = 0.12f;

    private struct ShellTiming
    {
        public float timeReachPouch;
        public float grabPause;
        public float timeInsertShell;
        public float lockDelay;
    }

    public override IEnumerator StartReload(ReloadController controller, WeaponController weapon)
    {
        Transform gripSocket = weapon.LeftHandTarget;
        Transform bulletPouch = controller.BulletPouchTarget;
        Transform chamberPlacement = GetChamberPlacement(weapon);

        if (chamberPlacement == null || bulletPouch == null)
        {
            weapon.IsReloading = false;
            yield break;
        }

        GameObject bulletInstance = Instantiate(GetBullet(weapon), controller.BulletPouchSocket);
        bulletInstance.transform.localScale = Vector3.one / 100f;
        bulletInstance.transform.localPosition = Vector3.zero;
        bulletInstance.transform.localRotation = Quaternion.identity;
        bulletInstance.SetActive(false);

        controller.Animator.CrossFadeInFixedTime(controller.ReloadAnimationHash, 0.15f, controller.ReloadLayer);

        float baseTimePerShell = weapon.WeaponData.Stats[StatType.ReloadTime];

        bool stepFinished = false;

        while (weapon.CurrentMagazineAmmo < weapon.WeaponData.Stats[StatType.MagazineSize] && weapon.CurrentWeaponAmmo > 0)
        {
            ShellTiming timing = CalculateReloadTimes(baseTimePerShell);

            stepFinished = false;
            controller.TargetHandler.SetHandTarget(bulletPouch, timing.timeReachPouch, () => stepFinished = true);
            yield return new WaitUntil(() => stepFinished);

            bulletInstance.SetActive(true);
            if (timing.grabPause > 0f) yield return new WaitForSeconds(timing.grabPause);

            stepFinished = false;
            controller.TargetHandler.SetHandTarget(chamberPlacement, timing.timeInsertShell, () => stepFinished = true);
            yield return new WaitUntil(() => stepFinished);

            controller.RecoilController.ApplyRigRecoil(0, 0.35f);
            bulletInstance.SetActive(false);
            if (timing.lockDelay > 0f) yield return new WaitForSeconds(timing.lockDelay);

            weapon.CurrentMagazineAmmo++;
            weapon.CurrentWeaponAmmo--;

            GameEventSystem.OnPlayerReloadTick?.Invoke();
        }

        controller.Animator.CrossFadeInFixedTime(controller.MovementAnimationHash, 0.2f, controller.ReloadLayer);

        controller.TargetHandler.SetHandTarget(gripSocket, 0.15f, () =>
        {
            Destroy(bulletInstance);

            GameEventSystem.OnPlayerReloadEnded?.Invoke();
            weapon.IsReloading = false;
        });
    }

    public override void CancelReload(ReloadController controller, WeaponController weapon)
    {
        Transform gripSocket = weapon.LeftHandTarget;

        controller.Animator.CrossFadeInFixedTime(controller.MovementAnimationHash, 0.2f, controller.ReloadLayer);

        controller.TargetHandler.SetHandTarget(gripSocket, 0.15f, () =>
        {
            foreach (Transform t in controller.BulletPouchSocket)
            {
                Destroy(t.gameObject);
            }

            GameEventSystem.OnPlayerReloadEnded?.Invoke();
            weapon.IsReloading = false;
        });

    }

    private ShellTiming CalculateReloadTimes(float totalReloadTime)
    {
        float wReach = weightReachPouch * Random.Range(1f - timingVariance, 1f + timingVariance);
        float wGrab = weightGrabPause * Random.Range(1f - timingVariance, 1f + timingVariance);
        float wInsert = weightInsertShell * Random.Range(1f - timingVariance, 1f + timingVariance);
        float wLock = weightLockDelay * Random.Range(1f - timingVariance, 1f + timingVariance);

        float totalWeight = wReach + wGrab + wInsert + wLock;

        return new ShellTiming
        {
            timeReachPouch = (wReach / totalWeight) * totalReloadTime,
            grabPause = (wGrab / totalWeight) * totalReloadTime,
            timeInsertShell = (wInsert / totalWeight) * totalReloadTime,
            lockDelay = (wLock / totalWeight) * totalReloadTime
        };
    }

    private Transform GetChamberPlacement(WeaponController weapon)
    {
        if (weapon.WeaponData.Modules.TryGetValue(ModuleType.Body, out var bodyMod) && bodyMod is BodyModule body)
        {
            return body.ChamberPlacement;
        }
        return null;
    }

    private GameObject GetBullet(WeaponController weapon)
    {
        return weapon.WeaponData.TotalType == WeaponType.Shotgun ? shotgunBullet : sniperBullet;
    }
}