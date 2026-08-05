using System.Collections;
using UnityEngine;
using UnityEngine.AI;
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

    [Space]
    [Header("Zombie Movement Type")]
    [SerializeField, Range(0, 5)] private float speed = 0;
    [SerializeField] private float slowSpeed = 1;
    [SerializeField] private float fastSpeed = 3;
    [SerializeField] private float runSpeed = 5;

    [Space]
    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
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
    private bool useFirstAttack = true;
    private int attackIndex = 0;

    //Initial Wait Variables
    private bool initialWaitStarted;
    private bool initialWaitFinished;

    private bool noHead = false;

    private void Start() {
        zombieAnimations = GetComponent<ZombieAnimations>();
    }

    private void Update() {
        UpdateMovementState();

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
    }

    #region Movement
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

    public void TakeDamage() {

    }

    public void Die() {
        if (health <= 0f) {
            int deathIndex = Random.Range(0, 3);

            StopAgent();
            zombieAnimations.SetDeathIndex(deathIndex);
            zombieAnimations.SetDeath();
        }
    }

    public void StopAgent() {
        agent.isStopped = true;
        pauseMovement = true;
        speed = 0;
    }

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

    //Wait a few seconds before chasing the player and playing the walk animation
    private IEnumerator InitialWait() {
        yield return new WaitForSeconds(initialIdleWaitTime);
        initialWaitFinished = true;
        CurrentMovementState = MovementState.Walk;
        speed = slowSpeed;
    }
}
