using UnityEngine;

public class ZombieAnimations : MonoBehaviour {

    [SerializeField] private Animator animator;

    private bool isWalking = false;
    private bool isRunning = false;

    private int walkingSpeed = Animator.StringToHash("walkSpeed");
    private int runningSpeed = Animator.StringToHash("runSpeed");
    private int isWalkingAnim = Animator.StringToHash("isWalking");
    private int isRunningAnim = Animator.StringToHash("isRunning");
    private int lostLeg = Animator.StringToHash("lostLeg");
    private int isEating = Animator.StringToHash("isEating");
    private int isScreaming = Animator.StringToHash("isScreaming");
    private int isHit = Animator.StringToHash("isHit");
    private int attackIndex = Animator.StringToHash("AttackIndex");
    private int attack = Animator.StringToHash("Attack");

    private Zombie zombie;

    private void Start() {
        zombie = GetComponent<Zombie>();
    }

    public void SetWalkSpeed(float value) {
        animator.SetFloat(walkingSpeed, value);
    }

    public void SetRunningSpeed(float value) {
        animator.SetFloat(runningSpeed, value);
    }

    public float GetWalkSpeed() {
        return walkingSpeed;
    }

    public float GetRunningSpeed() {
        return runningSpeed;
    }

    public void SetIsWalking(bool state) {
        if (state) {
            isWalking = true;
        } else {
            isWalking = false;
        }

        animator.SetBool(isWalkingAnim, state);
    }

    public void SetIsRunning(bool state) {
        if (state) {
            isRunning = true;
        } else {
            isRunning = false;
        }

        animator.SetBool(isRunningAnim, state);
    }

    public void SetAttackIndex(int value) {
        animator.SetInteger(attackIndex, value);
    }

    public void SetAttackTrigger() {
        animator.SetTrigger(attack);
    }

    public void SetLostLeg(bool state) {
        animator.SetBool(lostLeg, state);
    }

    public void SetIsEating(bool state) {
        animator.SetBool(isEating, state);
    }

    public void SetIsScreaming(bool state) {
        animator.SetBool(isScreaming, state);
    }

    public void SetIsHit(bool state) {
        animator.SetBool(isHit, state);
    }

    public bool GetIsWalking() {
        return isWalking;
    }

    public bool GetIsRunning() {
        return isRunning;
    }

}
