using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public enum ZombieMovementType {
    Slow,
    Fast
}

public class Zombie : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private bool initial;
    [Space]
    [Header("Zombie Movement Type")]
    [SerializeField] private ZombieMovementType zombieMovementType;
    [SerializeField] private float slowSpeed;
    [SerializeField] private float fastSpeed;
    [SerializeField] private float slowAnimationSpeed;
    [SerializeField] private float fastAnimationSpeed;
    [Space]
    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;

    private ZombieAnimations zombieAnimations;
    private float nextAttackTime;
    private bool useFirstAttack = true;
    private int attackIndex = 0;

    private void Start() {
        zombieAnimations = GetComponent<ZombieAnimations>();

        if (zombieMovementType == ZombieMovementType.Slow) {
            agent.speed = slowSpeed;
        } else if (zombieMovementType == ZombieMovementType.Fast) {
            agent.speed = fastSpeed;
        }

    }

    private void Update() {
        //First Zombies
        if (initial) {
            StartCoroutine(InitialWait());
        }

        //Set Animations
        if (zombieMovementType == ZombieMovementType.Slow) {
            SetZombieWalking();
        } else if (zombieMovementType == ZombieMovementType.Fast) {
            SetZombieRunning();
        }

        //Syncronisation of speed with animations
        if (zombieAnimations.GetIsWalking()) {
            zombieAnimations.SetWalkSpeed(slowAnimationSpeed);
        } else if (zombieAnimations.GetIsRunning()) {
            zombieAnimations.SetRunningSpeed(fastAnimationSpeed);
        }

        float sqrDistance = (player.transform.position - transform.position).sqrMagnitude;
        if (sqrDistance <= attackRange * attackRange) {
            //Stop and Attack
            agent.isStopped = true;

            if (Time.time >= nextAttackTime) {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        } else {
            //Set Target
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }
    }

    //Set Zombie Walking
    private void SetZombieWalking() {
        if (agent.velocity.magnitude != 0f) {
            zombieAnimations.SetIsWalking(true);
        } else {
            zombieAnimations.SetIsWalking(false);
        }
    }

    //Set Zombie Running
    private void SetZombieRunning() {
        if (agent.velocity.magnitude != 0f) {
            zombieAnimations.SetIsRunning(true);
        } else {
            zombieAnimations.SetIsRunning(false);
        }
    }

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

    #region AnimationEvents

    public void CheckHit() {

    }

    #endregion

    //Wait for 2 seconds before attacking the player
    private IEnumerator InitialWait() {
        yield return new WaitForSeconds(5);
    }
}
