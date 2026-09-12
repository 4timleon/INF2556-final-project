using System.Collections;
using EditorAttributes;
using UnityEngine;
using WeaponNamespace;

[CreateAssetMenu(fileName = "NewMagazineReload", menuName = "Weapon/Reload/Magazine")]
public class MagazineReload : ReloadBase
{
    [Title("Time Weights:", 14)]
    [Range(0f, 1f)][SerializeField] private float weightGrabMag = 0.21f;
    [Range(0f, 1f)][SerializeField] private float weightEjectMag = 0.16f;
    [Range(0f, 1f)][SerializeField] private float weightPouchDelay = 0.18f;
    [Range(0f, 1f)][SerializeField] private float weightInsertMag = 0.26f;
    [Range(0f, 1f)][SerializeField] private float weightReturnToGrip = 0.19f;
    [Range(0f, 1f)][SerializeField] private float timingVariance = 0.15f;

    private struct MagazineTiming
    {
        public float timeToGrabMag;
        public float timeToEjectMag;
        public float pouchDelay;
        public float timeToInsertMag;
        public float timeReturnToGrip;
    }

    public override IEnumerator StartReload(ReloadController controller, WeaponController weapon)
    {
        Transform gripSocket = weapon.LeftHandTarget;
        var magModule = weapon.WeaponData.Modules[ModuleType.Magazine];
        Transform magTransform = magModule.transform;
        Transform magPlacement = magModule.HandPlacement;

        Vector3 magHomePos = magTransform.localPosition;
        Vector3 magEjectedPos = magHomePos + Vector3.down * 5f;

        MagazineTiming timing = CalculateReloadTimes(weapon.WeaponData.Stats[StatType.ReloadTime]);

        controller.Animator.CrossFadeInFixedTime(controller.ReloadAnimationHash, 0.15f, controller.ReloadLayer);

        bool stepFinished = false;
        controller.TargetHandler.SetHandTarget(magPlacement, timing.timeToGrabMag, () => stepFinished = true);
        yield return new WaitUntil(() => stepFinished);

        stepFinished = false;
        controller.RecoilController.ApplyRigRecoil(0, 0.5f);
        LeanTween.moveLocal(magTransform.gameObject, magEjectedPos, timing.timeToEjectMag)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => stepFinished = true);
        yield return new WaitUntil(() => stepFinished);

        yield return new WaitForSeconds(timing.pouchDelay);

        stepFinished = false;
        LeanTween.moveLocal(magTransform.gameObject, magHomePos, timing.timeToInsertMag)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() =>
            {
                stepFinished = true;
                controller.RecoilController.ApplyRigRecoil(0.25f, 1f);
            });
        yield return new WaitUntil(() => stepFinished);

        yield return new WaitForSeconds(0.05f);

        controller.Animator.CrossFadeInFixedTime(controller.MovementAnimationHash, 0.2f, controller.ReloadLayer);

        stepFinished = false;
        controller.TargetHandler.SetHandTarget(gripSocket, timing.timeReturnToGrip, () => stepFinished = true);
        yield return new WaitUntil(() => stepFinished);

        int magSize = (int)weapon.WeaponData.Stats[StatType.MagazineSize];

        int bulletsNeeded = magSize - weapon.CurrentMagazineAmmo;

        int bulletsToReload = Mathf.Min(bulletsNeeded, weapon.CurrentWeaponAmmo);

        weapon.CurrentMagazineAmmo += bulletsToReload;

        weapon.CurrentWeaponAmmo -= bulletsToReload;

        GameEventSystem.OnPlayerReloadTick?.Invoke();
        GameEventSystem.OnPlayerReloadEnded?.Invoke();
        weapon.IsReloading = false;
    }

    public override void CancelReload(ReloadController controller, WeaponController weapon)
    {
        Transform gripSocket = weapon.LeftHandTarget;
        var magModule = weapon.WeaponData.Modules[ModuleType.Magazine];
        Transform magTransform = magModule.transform;

        LeanTween.cancel(magTransform.gameObject);

        controller.Animator.CrossFadeInFixedTime(controller.MovementAnimationHash, 0.15f, controller.ReloadLayer);


        controller.TargetHandler.SetHandTarget(gripSocket, 0.15f, () =>
        {
            magTransform.localPosition = Vector3.zero;
            GameEventSystem.OnPlayerReloadEnded?.Invoke();
            weapon.IsReloading = false;
        });
    }

    private MagazineTiming CalculateReloadTimes(float totalReloadTime)
    {
        float wGrab = weightGrabMag * Random.Range(1f - timingVariance, 1f + timingVariance);
        float wEject = weightEjectMag * Random.Range(1f - timingVariance, 1f + timingVariance);
        float wPouch = weightPouchDelay * Random.Range(1f - timingVariance, 1f + timingVariance);
        float wInsert = weightInsertMag * Random.Range(1f - timingVariance, 1f + timingVariance);
        float wReturn = weightReturnToGrip * Random.Range(1f - timingVariance, 1f + timingVariance);

        float totalWeight = wGrab + wEject + wPouch + wInsert + wReturn;

        return new MagazineTiming
        {
            timeToGrabMag = (wGrab / totalWeight) * totalReloadTime,
            timeToEjectMag = (wEject / totalWeight) * totalReloadTime,
            pouchDelay = (wPouch / totalWeight) * totalReloadTime,
            timeToInsertMag = (wInsert / totalWeight) * totalReloadTime,
            timeReturnToGrip = (wReturn / totalWeight) * totalReloadTime
        };
    }
}