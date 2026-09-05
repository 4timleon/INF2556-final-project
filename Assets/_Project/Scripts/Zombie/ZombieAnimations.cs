using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieAnimations : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private float locomotionBlendSpeed = 10f;
    [SerializeField] private int idleAnimationsAmount;
    [SerializeField] private int walkAnimationsAmount;
    [SerializeField] private int runAnimationsAmount;

    //Movement
    private int speedHash = Animator.StringToHash("speed");
    private int idleTypeHash = Animator.StringToHash("idleType");
    private int walkTypeHash = Animator.StringToHash("walkType");
    private int runTypeHash = Animator.StringToHash("runType");

    //OutOfGround
    private int outOfGroundHash = Animator.StringToHash("OutOfGround");

    //Leg Lost
    private int lostLegHash = Animator.StringToHash("lostLeg");
    private int hasLostLegHash = Animator.StringToHash("hasLostLeg");

    //Attack
    private int attackIndexHash = Animator.StringToHash("AttackIndex");
    private int attackHash = Animator.StringToHash("Attack");

    //Jump
    private int jumpUpHash = Animator.StringToHash("JumpUp");
    private int jumpDownHash = Animator.StringToHash("JumpDown");

    //Death
    private int deathIndexHash = Animator.StringToHash("deathIndex");
    private int deathHash = Animator.StringToHash("death");

    private int isEatingHash = Animator.StringToHash("isEating");
    private int isScreamingHash = Animator.StringToHash("isScreaming");
    private int isHitHash = Animator.StringToHash("isHit");

    private Zombie zombie;
    private float currentBlendInput = 0;
    private float idleIndex;
    private float walkIndex;
    private float runIndex;

    private float tempIndex;

    private void Start()
    {
        zombie = GetComponent<Zombie>();
        idleIndex = Random.Range(0, idleAnimationsAmount);
        walkIndex = Random.Range(0, walkAnimationsAmount);
        runIndex = Random.Range(0, runAnimationsAmount);
    }

    private void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        currentBlendInput = Mathf.Lerp(currentBlendInput, zombie.Speed, locomotionBlendSpeed * Time.deltaTime);
        animator.SetFloat(speedHash, currentBlendInput);

        switch (zombie.CurrentMovementState)
        {
            case Zombie.MovementState.Idle:
                animator.SetFloat(idleTypeHash, idleIndex);
                break;
            case Zombie.MovementState.Walk:
                animator.SetFloat(walkTypeHash, walkIndex);
                break;
            case Zombie.MovementState.Run:
                animator.SetFloat(runTypeHash, runIndex);
                break;
        }
    }

    public void SetTemporaryWalk(int index)
    {
        switch (zombie.CurrentMovementState)
        {
            case Zombie.MovementState.Idle:
                tempIndex = idleIndex;
                idleIndex = index;
                break;
            case Zombie.MovementState.Walk:
                tempIndex = walkIndex;
                walkIndex = index;
                break;
            case Zombie.MovementState.Run:
                tempIndex = runIndex;
                runIndex = index;
                break;
        }
    }

    public void SetMovementBack()
    {
        switch (zombie.CurrentMovementState)
        {
            case Zombie.MovementState.Idle:
                idleIndex = tempIndex;
                break;
            case Zombie.MovementState.Walk:
                walkIndex = tempIndex;
                break;
            case Zombie.MovementState.Run:
                runIndex = tempIndex;
                break;
        }
    }

    public void SetOutOfGroundBool(bool state)
    {
        animator.SetBool(outOfGroundHash, state);
    }

    public void SetAttackIndex(int value)
    {
        animator.SetInteger(attackIndexHash, value);
    }

    public void SetAttackTrigger()
    {
        animator.SetTrigger(attackHash);
    }

    public void SetJumpUpTrigger()
    {
        animator.SetTrigger(jumpUpHash);
    }

    public void SetJumpDownTrigger()
    {
        animator.SetTrigger(jumpDownHash);
    }

    public void SetDeathIndex(int value)
    {
        animator.SetInteger(deathIndexHash, value);
    }

    public void SetDeath()
    {
        animator.SetTrigger(deathHash);
    }

    public void SetLostLeg()
    {
        animator.SetTrigger(lostLegHash);
        animator.SetTrigger(hasLostLegHash);
        zombie.StopAgent();
        zombie.StopMovement(true);
    }

    public void SetIsEating(bool state)
    {
        animator.SetBool(isEatingHash, state);
    }

    public void SetIsScreaming(bool state)
    {
        animator.SetBool(isScreamingHash, state);
    }

    public void SetIsHit()
    {
        animator.SetTrigger(isHitHash);
    }

    public void SetAnimator(bool state)
    {
        animator.enabled = state;
    }

    #region Animation Events
    public void LostLegFinished()
    {
        zombie.StartAgent(Zombie.MovementState.Walk);
        zombie.StopMovement(false);
    }

    public void AttackStarted()
    {
        zombie.PlayAttackSound();
    }

    public void TriggerDamage()
    {
        zombie.DealDamage();
    }

    public void AttackFinished()
    {
        zombie.SetIsAttacking(false);
    }

    #endregion


}
