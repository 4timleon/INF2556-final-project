using System;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Zombie : MonoBehaviour
{
    public enum MovementState
    {
        Idle,
        Walk,
        Fast,
        Run
    }

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private bool initial;
    [SerializeField] private Rigidbody[] joints;
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform centerTransform;
    [SerializeField] private LayerMask playerLayer;
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
    [SerializeField] private float armAttackRange = 0.5f;
    [SerializeField] private float damage = 25f;

    [Space]
    [Header("Health")]
    [SerializeField] private float health = 100;

    [Space]
    [Header("Sound")]
    [SerializeField] private AudioSource zombieAudioSource;

    public float Speed => speed;
    public MovementState CurrentMovementState { get; private set; }
    private bool pauseMovement = false;

    private Player player;
    private ZombieAnimations zombieAnimations;

    //Attack Variables
    private bool isAttacking = false;
    private float nextAttackTime;
    private float attackCooldown = 1f;
    private bool useFirstAttack = true;
    private int attackIndex = 0;
    private MovementState tempMovementState;

    //Attack Collision
    private Collider[] hitColliders = new Collider[10];

    //OutOfGround Variables
    private bool startOutOfGround = false;

    //Initial Wait Variables
    private bool initialWaitStarted;
    private bool initialWaitFinished;

    //No Head variables
    private bool noHead = false;
    private float nextDamageTaken = 1f;
    private float damageCooldown = 1f;

    //NavMesh link animation
    private bool wasOnOffMeshLink = false;
    private int jumpAreaID;

    //Death
    public bool IsDead { get; private set; } = false;

    //Sound Cooldown;
    private float randomWait;

    private void Awake()
    {
        jumpAreaID = NavMesh.GetAreaFromName("Jump");
        zombieAnimations = GetComponent<ZombieAnimations>();
    }

    private void OnDisable()
    {
        System.Array.Clear(hitColliders, 0, hitColliders.Length);
    }

    private void Start()
    {
        foreach (var joint in joints) joint.isKinematic = true;

        randomWait = Random.Range(2f, 5f);
    }

    private void Update()
    {
        //UpdateMovementState();
        HandleOffMeshLinkAnimation();

        //Give Players time at the start
        if (initial && !initialWaitFinished)
        {
            if (!initialWaitStarted)
            {
                initialWaitStarted = true;
                StartCoroutine(InitialWait());
            }

            agent.isStopped = true;
            agent.ResetPath();
            speed = 0f;
            return;
        }

        if (!player.IsInPreDeath && !player.IsDead && !IsDead && !startOutOfGround)
        {
            CheckPlayer();

            if (isAttacking)
            {
                SlowDownMovementOnAttack();
            }
            else
            {
                ResetMovementAfterAttack();
            }
        }
        else
        {
            StopAgent();
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            TakeDamage(100f);
        }

        //Damage Zombie if head is detached
        if (noHead && !IsDead)
        {
            if (Time.time >= nextDamageTaken)
            {
                TakeDamage(5f);
                nextDamageTaken = Time.time + damageCooldown;
                //Debug.Log("Damage Taken! HP left: " + health);
            }
        }

        //Random Zombie Soundsd
        if (!noHead && !IsDead && !isAttacking)
        {
            randomWait -= Time.deltaTime;
            if (randomWait <= 0)
            {
                SoundManager.Instance?.PlayZombieChaseSound(zombieAudioSource);

                randomWait = Random.Range(4f, 8f);
            }
        }
    }

    #region Movement

    private void HandleOffMeshLinkAnimation()
    {
        bool isOnLink = agent.isOnOffMeshLink;
        if (isOnLink && !wasOnOffMeshLink)
        {
            OffMeshLinkData link = agent.currentOffMeshLinkData;

            StopAllCoroutines();
            StartCoroutine(DoOffMeshLink(link));
        }

        wasOnOffMeshLink = isOnLink;
    }

    private IEnumerator DoOffMeshLink(OffMeshLinkData link)
    {
        var dir = (link.endPos - link.startPos).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);

        if (ShouldJumpOnLink(link) && link.endPos.y > transform.position.y)
        {
            zombieAnimations.SetJumpUpTrigger();
        }
        else
        {
            zombieAnimations.SetJumpDownTrigger();
        }

        var duration = 0.6f;
        var time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(link.startPos, link.endPos, smoothT);
            yield return null;
        }

        transform.position = link.endPos;
        agent.CompleteOffMeshLink();
    }

    private bool ShouldJumpOnLink(OffMeshLinkData link)
    {
        if (link.owner is NavMeshLink navMeshLink)
        {
            return navMeshLink.area == jumpAreaID;
        }

        return false;
    }

    //Check if Player is in attack range. If not then move
    private void CheckPlayer()
    {
        //Set Animations
        float sqrDistance = (player.transform.position - transform.position).sqrMagnitude;
        if (sqrDistance <= attackRange * attackRange && !pauseMovement)
        {
            //Stop and Attack
            StopAgent();

            if (Time.time >= nextAttackTime && !isAttacking)
            {
                Attack();

                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            //Set Target
            StartAgent(CurrentMovementState);
            isAttacking = false;

            agent.SetDestination(player.transform.position);
            agent.speed = speed;
        }
    }

    private void SlowDownMovementOnAttack()
    {
        CurrentMovementState = MovementState.Walk;
        speed = slowSpeed;

        zombieAnimations.SetTemporaryWalk(0);
    }

    public void ResetMovementAfterAttack()
    {
        StartAgent(tempMovementState);

        zombieAnimations.SetMovementBack();
    }
    #endregion

    //Starts Attacking the Player when in range
    private void Attack()
    {
        isAttacking = true;

        if (useFirstAttack)
        {
            zombieAnimations.SetAttackIndex(attackIndex);
            zombieAnimations.SetAttackTrigger();
        }
        else
        {
            zombieAnimations.SetAttackIndex(attackIndex);
            zombieAnimations.SetAttackTrigger();
        }

        attackIndex = (attackIndex + 1) % 2;
        useFirstAttack = !useFirstAttack;
    }

    public void PlayAttackSound()
    {
        //Play Sound
        if (zombieAudioSource.isPlaying)
        {
            zombieAudioSource.Stop();
        }
        SoundManager.Instance?.PlayZombieAttackSound(zombieAudioSource);
    }

    public void DealDamage()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(centerTransform.position, armAttackRange, hitColliders, playerLayer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = hitColliders[i];

            if (hit.TryGetComponent(out Player player))
            {
                player.TakeDamage(damage);
                return;
            }
        }
    }

    /* private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerTransform.position, armAttackRange);
    } */

    //Zombie Takes Damage by given value
    public void TakeDamage(float value)
    {
        if (IsDead) return;

        health -= value;
        health = Mathf.Max(health, 0);

        if (zombieAudioSource.isPlaying)
        {
            zombieAudioSource.Stop();
        }
        SoundManager.Instance.PlayZombieHitSound(zombieAudioSource);

        if (health <= 0f)
        {
            Die();
        }
    }

    //Starts one of 3 Death Animations and activates Ragdoll
    public void Die()
    {
        if (IsDead) return;
        IsDead = true;

        int deathIndex = Random.Range(0, 3);

        StopAgent();
        StopMovement(true);

        zombieAnimations.SetDeathIndex(deathIndex);
        zombieAnimations.SetDeath();

        GameEventSystem.OnEnemyKilled?.Invoke(this);

        StartCoroutine(StartRagdoll(1f));
    }

    //Start OutOfGround Animation
    public void StartOutOfGroundAnimation()
    {
        StopAgent();
        StopMovement(true);
        startOutOfGround = true;
        zombieAnimations.SetOutOfGroundBool(true);
    }

    #region Animation Event
    public void ContinueMovement()
    {
        StartAgent(CurrentMovementState);
        StopMovement(false);
        startOutOfGround = false;
    }
    #endregion

    public void StartMovement()
    {
        startOutOfGround = false;
        zombieAnimations.SetOutOfGroundBool(false);
    }

    //Stops the NavMesh Agent
    public void StopAgent()
    {
        agent.isStopped = true;
        speed = 0;
    }

    //Starts the NavMeshAgent and sets the speed
    public void StartAgent(MovementState state)
    {
        CurrentMovementState = state;

        agent.isStopped = false;

        switch (state)
        {
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

    public void StopMovement(bool state)
    {
        pauseMovement = state;
    }

    public void SetTarget(Player player)
    {
        this.player = player;
    }

    public void SetSpeed(MovementState state)
    {
        initial = false;

        StartAgent(state);

        tempMovementState = state;
    }

    public void SetIsAttacking(bool state)
    {
        isAttacking = state;
    }

    public void SetNoHead()
    {
        noHead = true;
        zombieAudioSource.Stop();
    }

    //Wait a few seconds before chasing the player and playing the walk animation
    private IEnumerator InitialWait()
    {
        yield return new WaitForSeconds(initialIdleWaitTime);
        initialWaitFinished = true;
        CurrentMovementState = MovementState.Walk;
        speed = slowSpeed;
    }

    private IEnumerator StartRagdoll(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        zombieAnimations.SetAnimator(false);
        foreach (var joint in joints) joint.isKinematic = false;
    }
}
