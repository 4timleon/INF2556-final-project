using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPartType
{
    Head,
    Body,
    BodyTop,
    Hip,
    LArmPalm,
    LForearm,
    LUpperarm,
    LLegCalf,
    LLegThigh,
    RArmPalm,
    RForearm,
    RUpperarm,
    RLegCalf,
    RLegThigh,
}

public class DetachController : MonoBehaviour
{
    [SerializeField] private List<Detach> detachables;
    [SerializeField] private float headThreshold = 30;
    [SerializeField] private float armThreshold = 20;
    [SerializeField] private float legThreshold = 40;
    [SerializeField] private Detach leftPalmBodyPart;
    [SerializeField] private Detach rightPalmBodyPart;

    [Header("Points per Body Part")]
    [SerializeField] private int headPoints = 20;
    [SerializeField] private int normalPoints = 10;

    [Header("Dissolve Effect")]
    [SerializeField] private ParticleSystem dissolveParticle;

    private Zombie zombie;
    private ZombieAnimations zombieAnimations;

    private bool hasLostLeg = false;

    private void Start()
    {
        zombie = GetComponent<Zombie>();
        zombieAnimations = GetComponent<ZombieAnimations>();
    }

    public void Hit(Detach bodyPart, float damageValue, Vector3 direction, Collider collider)
    {
        if (bodyPart.IsDetached || zombie.IsDead) return;

        if (bodyPart.BodyPartType != BodyPartType.Head)
        {
            zombie.TakeDamage(damageValue);
        }

        bodyPart.TakeBodyPartDamage(damageValue);
        zombieAnimations.SetIsHit();

        switch (bodyPart.BodyPartType)
        {
            case BodyPartType.Head:
                if (bodyPart.DamageTaken >= headThreshold)
                {
                    bodyPart.DetachMesh(direction);
                    zombie.SetNoHead();
                    collider.enabled = false;
                }
                zombie.TakeDamage(damageValue * 1.5f);
                GameData.Money += headPoints;
                GameEventSystem.OnMoneyUpdated?.Invoke();
                return;
            case BodyPartType.LLegCalf or BodyPartType.RLegCalf:
                if (bodyPart.DamageTaken >= legThreshold && !hasLostLeg)
                {
                    bodyPart.DetachMesh(direction);
                    zombieAnimations.SetLostLeg();
                    hasLostLeg = true;
                    collider.enabled = false;
                }
                break;
            case BodyPartType.LForearm:
                if (bodyPart.DamageTaken >= armThreshold)
                {
                    bodyPart.DetachMesh(direction);
                    leftPalmBodyPart.gameObject.SetActive(false);
                    collider.enabled = false;
                }
                break;
            case BodyPartType.RForearm:
                if (bodyPart.DamageTaken >= armThreshold)
                {
                    bodyPart.DetachMesh(direction);
                    rightPalmBodyPart.gameObject.SetActive(false);
                    collider.enabled = false;
                }
                break;
            default:
                break;
        }

        GameData.Money += normalPoints;
        GameEventSystem.OnMoneyUpdated?.Invoke();
    }

    public void DissolveZombie()
    {
        foreach (Detach bodyPart in detachables)
        {
            if (bodyPart != null)
            {
                StartCoroutine(bodyPart.DissolveBodyPart());
            }
        }

        StartCoroutine(WaitUntilDestroy());
    }

    private IEnumerator WaitUntilDestroy()
    {
        yield return new WaitForSeconds(1f);

        dissolveParticle.Play();

        while (dissolveParticle.isPlaying)
        {
            yield return null;
        }

        Destroy(gameObject);
    }

}
