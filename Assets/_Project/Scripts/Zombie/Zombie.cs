using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Zombie : MonoBehaviour {
    public enum MovementState {
        Idle,
        Walk,
        Fast,
        Run
    }

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private bool initial;
    [SerializeField] private Rigidbody[] joints;
    //[SerializeField] private Collider[] colliders;

    [Space]
    [Header("Zombie Movement Type")]
    [SerializeField, Range(0, 5)] private float speed = 0;
    [SerializeField] private float slowSpeed = 1;
    [SerializeField] private float fastSpeed = 3;
    [SerializeField] private float runSpeed = 5;

    [Space]
    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField, Range(1f, 10f)] private float initialIdleWaitTime = 3f;

    [Space]
    [Header("Health")]
    [SerializeField] private float health = 100;

    public float Speed => speed;
    public MovementState CurrentMovementState { get; private set; }
    private bool pauseMovement = false;

    private ZombieAnimations zombieAnimations;

    //Attack Variables
    private float nextAttackTime;
    private float attackCooldown = 1f;
    private bool useFirstAttack = true;
    private int attackIndex = 0;

    //Initial Wait Variables
    private bool initialWaitStarted;
    private bool initialWaitFinished;

    //No Head variables
    private bool noHead = false;
    private float nextDamageTaken = 1f;
    private float damageCooldown = 1f;

    //Death
    public bool IsDead { get; private set; } = false;

    private void Start() {
        zombieAnimations = GetComponent<ZombieAnimations>();

        foreach (var joint in joints) joint.isKinematic = true;

    }

    private void Update() {
        UpdateMovementState();

        //Give Players time at the start
        if (initial && !initialWaitFinished) {
            if (!initialWaitStarted) {
                initialWaitStarted = true;
                StartCoroutine(InitialWait());
            }

            agent.isStopped = true;
            agent.ResetPath();
            speed = 0f;
            return;
        }

        CheckPlayer();

        if (Keyboard.current.digit1Key.wasPressedThisFrame) {
            TakeDamage(100f);
        }

        //Damage Zombie if head is detached
        if (noHead && !IsDead) {
            if (Time.time >= nextDamageTaken) {
                TakeDamage(5f);
                nextDamageTaken = Time.time + damageCooldown;
                //Debug.Log("Damage Taken! HP left: " + health);
            }
        }
    }

    #region Movement

    //Check if Player is in attack range. If not then move
    private void CheckPlayer() {
        if (pauseMovement) {
            return;
        }

        //Set Animations
        float sqrDistance = (player.transform.position - transform.position).sqrMagnitude;
        if (sqrDistance <= attackRange * attackRange) {
            //Stop and Attack
            agent.isStopped = true;
            speed = 0;

            if (Time.time >= nextAttackTime) {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        } else {
            //Set Target
            speed = slowSpeed;
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
            agent.speed = speed;
        }
    }

    //Dynamic Movement State Change with speed
    private void UpdateMovementState() {
        if (speed <= 0f) {
            CurrentMovementState = MovementState.Idle;
        } else if (speed <= slowSpeed) {
            CurrentMovementState = MovementState.Walk;
        } else if (speed <= fastSpeed) {
            CurrentMovementState = MovementState.Fast;
        } else {
            CurrentMovementState = MovementState.Run;
        }
    }
    #endregion

    //Starts Attacking the Player when in range
    private void Attack() {
        if (useFirstAttack) {
            zombieAnimations.SetAttackIndex(attackIndex);
            zombieAnimations.SetAttackTrigger();
        } else {
            zombieAnimations.SetAttackIndex(attackIndex);
            zombieAnimations.SetAttackTrigger();
        }

        attackIndex = (attackIndex + 1) % 2;
        useFirstAttack = !useFirstAttack;
    }

    public void DealDamage() {

    }

    //Zombie Takes Damage by given value
    public void TakeDamage(float value) {
        if (IsDead) return;

        health -= value;

        if (health <= 0f) {
            Die();
        }
    }

    //Starts one of 3 Death Animations and activates Ragdoll
    public void Die() {
        int deathIndex = Random.Range(0, 3);

        StopAgent();

        zombieAnimations.SetDeathIndex(deathIndex);
        zombieAnimations.SetDeath();

        IsDead = true;

        StartCoroutine(StartRagdoll(1f));
    }

    //Stops the NavMesh Agent
    public void StopAgent() {
        agent.isStopped = true;
        pauseMovement = true;
        speed = 0;
    }

    //Starts the NavMeshAgent and sets the speed
    public void StartAgent(MovementState state) {
        agent.isStopped = false;
        pauseMovement = false;

        switch (state) {
            case MovementState.Walk:
                speed = slowSpeed;
                break;
            case MovementState.Fast:
                speed = fastSpeed;
                break;
            case MovementState.Run:
                speed = runSpeed;
                break;
        }
    }

    public void SetNoHead() {
        noHead = true;
    }

    //Wait a few seconds before chasing the player and playing the walk animation
    private IEnumerator InitialWait() {
        yield return new WaitForSeconds(initialIdleWaitTime);
        initialWaitFinished = true;
        CurrentMovementState = MovementState.Walk;
        speed = slowSpeed;
    }

    private IEnumerator StartRagdoll(float seconds) {
        yield return new WaitForSeconds(seconds);

        zombieAnimations.SetAnimator(false);
        foreach (var joint in joints) joint.isKinematic = false;
    }
}
