using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BodyPartType {
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

public class DetachController : MonoBehaviour {

    [SerializeField] private List<Detach> detachables;
    [SerializeField] private float damage = 10;
    [SerializeField] private float headThreshold = 30;
    [SerializeField] private float armThreshold = 20;
    [SerializeField] private float legThreshold = 40;
    [SerializeField] private Detach leftPalmBodyPart;
    [SerializeField] private Detach rightPalmBodyPart;

    private Zombie zombie;
    private ZombieAnimations zombieAnimations;

    private void Start() {
        zombie = GetComponent<Zombie>();
        zombieAnimations = GetComponent<ZombieAnimations>();
    }

    private void Update() {//Remove Later after shooting implemented
        if (!Keyboard.current.spaceKey.wasPressedThisFrame || zombie.IsDead) return;

        Detach randomBodyPart = detachables[Random.Range(0, detachables.Count)];
        Hit(randomBodyPart, damage);
    }

    public void Hit(Detach bodyPart, float value) {
        if (bodyPart.IsDetached) return;

        zombie.TakeDamage(value);
        bodyPart.TakeBodyPartDamage(value);
        zombieAnimations.SetIsHit();

        switch (bodyPart.BodyPartType) {
            case BodyPartType.Head:
                if (bodyPart.DamageTaken >= headThreshold) {
                    bodyPart.DetachMesh();
                    zombie.SetNoHead();
                }
                break;
            case BodyPartType.LLegCalf or BodyPartType.RLegCalf:
                if (bodyPart.DamageTaken >= legThreshold) {
                    bodyPart.DetachMesh();
                    zombieAnimations.SetLostLeg();
                }
                break;
            case BodyPartType.LForearm:
                if (bodyPart.DamageTaken >= armThreshold) {
                    bodyPart.DetachMesh();
                    leftPalmBodyPart.gameObject.SetActive(false);
                }
                break;
            case BodyPartType.RForearm:
                if (bodyPart.DamageTaken >= armThreshold) {
                    bodyPart.DetachMesh();
                    rightPalmBodyPart.gameObject.SetActive(false);
                }
                break;
        }
    }

    public void RemoveFromList(Detach bodyPart) {//Remove Later after shooting implemented
        detachables.Remove(bodyPart);
    }
}
