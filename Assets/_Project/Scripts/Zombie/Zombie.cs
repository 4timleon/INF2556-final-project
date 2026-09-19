using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Transform armBarricadeTransform;
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
    [SerializeField] private float barricadePlayerAttackRange = 2f;
    [SerializeField] private float damage = 25f;

    [Space]
    [Header("Health")]
    [SerializeField] private float health = 100;

    [Space]
    [Header("Sound")]
    [SerializeField] private AudioSource zombieAudioSource;

    [Space]
    [Header("Points")]
    [SerializeField] private int deathPoints = 100;

    [Space]
    [Header("Debug")]
    [SerializeField] private bool debug = false;

    public float Speed => speed;
    public bool IsAttackingBarricade => isAttackingBarricade;
    public MovementState CurrentMovementState { get; private set; }

    private ZombieAnimations zombieAnimations;
    private DetachController detachController;

    //Target
    private Player player;
    private Vector3 targetTransform;

    //Attack Variables
    private bool isAttacking = false;
    private bool isAttackingPlayerBarricade = false;
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

    //NavMesh Movement
    private bool pauseMovement = false;
    private float checkTimer = 0f;
    private float checkInterval = 0.5f;

    //NavMesh link animation
    private bool isProcessingOffMeshLink = false;
    private bool wasOnOffMeshLink = false;
    private Coroutine offMeshLinkCoroutine;
    private bool cancelOffMeshLink;
    private Vector3 windowClimbEndPosition;
    private int jumpAreaID;
    private int climbAreaID;

    //Barricade
    private BarricadeController barricade = null;
    private bool isAttackingBarricade;

    //Death
    public bool IsDead { get; private set; } = false;

    //Sound Cooldown;
    private float randomWait;

    //Main Menu Spawn Variable
    private bool mainMenuState = false;

    private void Awake()
    {
        jumpAreaID = NavMesh.GetAreaFromName("Jump");
        climbAreaID = NavMesh.GetAreaFromName("Climb");
        zombieAnimations = GetComponent<ZombieAnimations>();
        detachController = GetComponent<DetachController>();
    }

    private void OnDisable()
    {
        System.Array.Clear(hitColliders, 0, hitColliders.Length);
    }

    private void Start()
    {
        foreach (var joint in joints) joint.isKinematic = true;

        randomWait = Random.Range(2f, 5f);
        checkTimer = Random.Range(0f, 0.5f);
    }

    private void Update()
    {
        if (mainMenuState)
        {
            if (targetTransform != null)
            {
                StartAgent(MovementState.Walk);
                CheckNextPosition(targetTransform, slowSpeed);

                if (Vector3.Distance(transform.position, targetTransform) < 5f)
                {
                    Destroy(gameObject);
                }
            }
            return;
        }

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

        if (player != null && !player.IsInPreDeath && !player.IsDead && !IsDead && !startOutOfGround)
        {
            if (!agent.isOnOffMeshLink && !isAttackingBarricade)
            {
                CheckPlayer();
            }

            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                CheckIfBetterPathExists();
            }

            if (isAttacking)
            {
                SlowDownMovementOnAttack();
            }
        }
        else
        {
            StopAgent();
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
        if (isOnLink && !wasOnOffMeshLink && !isProcessingOffMeshLink)
        {
            OffMeshLinkData link = agent.currentOffMeshLinkData;

            if (offMeshLinkCoroutine != null)
            {
                StopCoroutine(offMeshLinkCoroutine);
            }

            cancelOffMeshLink = false;
            isProcessingOffMeshLink = true;

            if (ShouldJumpOnLink(link))
            {
                offMeshLinkCoroutine = StartCoroutine(DoOffMeshJumpLink(link));
            }
            else if (ShouldClimbOnLink(link))
            {
                offMeshLinkCoroutine = StartCoroutine(DoOffMeshClimbLink(link));
            }
        }

        wasOnOffMeshLink = isOnLink;
    }

    private IEnumerator DoOffMeshJumpLink(OffMeshLinkData link)
    {
        agent.Warp(link.startPos);

        Vector3 dir = (link.endPos - link.startPos).normalized;
        dir.y = 0;
        agent.updateRotation = false;
        transform.rotation = Quaternion.LookRotation(dir);

        StopAgent();

        //Animation for the Jump links
        if (link.endPos.y > transform.position.y)
        {
            zombieAnimations.SetJumpUpTrigger();
        }
        else
        {
            zombieAnimations.SetJumpDownTrigger();
        }

        yield return StartCoroutine(LerpToPosition(link.startPos, link.endPos, 0.6f));

        //Complete Link
        CleanUpOffMeshLink();
    }

    private IEnumerator DoOffMeshClimbLink(OffMeshLinkData link)
    {
        agent.Warp(link.startPos);

        Vector3 dir = (link.endPos - link.startPos).normalized;
        dir.y = 0;
        agent.updateRotation = false;
        transform.rotation = Quaternion.LookRotation(dir);

        StopAgent();

        if (barricade != null && !barricade.GetAllPlanksDestroyed())
        {
            isAttackingBarricade = true;
        }

        //Debug.Log((offMeshLinkCoroutine != null) + " | " + isAttackingBarricade + " | " + cancelOffMeshLink);
        //Wait and Attack the Barricade until it is broken up completely or gets canceled
        while (isAttackingBarricade && !cancelOffMeshLink)
        {
            if (Time.time >= nextAttackTime)
            {
                if (barricade.PlayerInRange)
                {
                    zombieAnimations.SetWindowAttack(true);
                    if (isAttackingPlayerBarricade)
                    {
                        TryDealPlayerDamage(armBarricadeTransform, barricadePlayerAttackRange);
                    }
                }
                else
                {
                    zombieAnimations.SetWindowAttack(false);
                    isAttackingPlayerBarricade = false;
                    Attack();
                }

                nextAttackTime = Time.time + attackCooldown;
            }
            yield return null;
        }

        zombieAnimations.SetWindowAttack(false);
        isAttackingPlayerBarricade = false;
        isAttackingBarricade = false;

        //Was the link canceled? Then set it finished!
        if (cancelOffMeshLink)
        {
            CleanUpOffMeshLink();
            yield break;
        }

        yield return new WaitForSeconds(1f);

        //Barricade broken up
        //Debug.Log("Playing Window Climb Animation");
        zombieAnimations.SetWindowClimb();//Animation for the Window links

        windowClimbEndPosition = link.endPos;
        yield return null;
    }

    // When the Climb Animation ends an Animation Event Triggers and executes this Function, 
    // warping the Zombie to the end and cleaning up the link
    public void FinishClimbAnimation()
    {
        if (cancelOffMeshLink)
        {
            CleanUpOffMeshLink();
            return;
        }

        agent.Warp(windowClimbEndPosition);
        CleanUpOffMeshLink();
    }


    private IEnumerator LerpToPosition(Vector3 start, Vector3 end, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            if (cancelOffMeshLink) yield break;

            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(start, end, smoothT);
            yield return null;
        }

        transform.position = end;
    }

    public void CleanUpOffMeshLink()
    {
        if (agent.isOnOffMeshLink)
        {
            agent.CompleteOffMeshLink();
        }

        agent.updateRotation = true;
        ContinueMovement();
        offMeshLinkCoroutine = null;
        isProcessingOffMeshLink = false;
    }

    private bool ShouldJumpOnLink(OffMeshLinkData link)
    {
        if (link.owner is NavMeshLink navMeshLink) return navMeshLink.area == jumpAreaID;
        return false;
    }

    private bool ShouldClimbOnLink(OffMeshLinkData link)
    {
        if (link.owner is NavMeshLink navMeshLink) return navMeshLink.area == climbAreaID;
        return false;
    }

    //Check if Player is in attack range. If not then move
    private void CheckPlayer()
    {
        // An interrupted attack may not fire AttackFinished, so do not keep the attack state forever.
        if (isAttacking && Time.time >= nextAttackTime)
        {
            isAttacking = false;
        }

        //Set Animations
        if (CheckPlayerInRange() && !pauseMovement)
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

            CheckNextPosition(player.transform.position, speed);
        }
    }

    private void CheckNextPosition(Vector3 target, float speed)
    {
        agent.SetDestination(target);
        agent.speed = speed;
    }

    private bool CheckPlayerInRange()
    {
        float sqrDistance = (player.transform.position - transform.position).sqrMagnitude;
        if (sqrDistance <= attackRange * attackRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void CheckIfBetterPathExists()
    {
        if (!isAttackingBarricade) return;

        //Filter
        int climbAreaMask = 1 << climbAreaID;
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = ~climbAreaMask //All Areas except "Climb"
        };

        NavMeshPath altPath = new NavMeshPath();

        if (NavMesh.CalculatePath(transform.position, player.transform.position, filter, altPath))
        {
            if (altPath.status == NavMeshPathStatus.PathComplete)
            {
                float altPathCost = GetPathCost(altPath);
                if (altPathCost < 15f)
                {//If Path shorter then cancel barricade attack
                    //Debug.Log(altPathCost);
                    //Debug.Log("Barricade Canceled");

                    zombieAnimations.SetWindowAttack(false);
                    isAttackingPlayerBarricade = false;
                    isAttackingBarricade = false;
                    barricade = null;
                    cancelOffMeshLink = true;

                    if (offMeshLinkCoroutine != null)
                    {
                        StopCoroutine(offMeshLinkCoroutine);
                        offMeshLinkCoroutine = null;
                    }

                    StartAgent(CurrentMovementState);
                    agent.SetPath(altPath);
                }
            }
        }
    }

    private float GetPathCost(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
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

        transform.forward = (player.transform.position - transform.position).normalized;

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
        if (noHead) return;

        //Play Sound
        if (zombieAudioSource.isPlaying)
        {
            zombieAudioSource.Stop();
        }
        SoundManager.Instance?.PlayZombieAttackSound(zombieAudioSource);
    }

    public void DealDamage()
    {
        if (isAttackingBarricade)
        {
            //Debug.Log("Breaking Next Plank");
            barricade.BreakNextPlank();

            if (barricade.GetAllPlanksDestroyed())
            {
                zombieAnimations.SetWindowAttack(false);
                isAttackingPlayerBarricade = false;
                isAttackingBarricade = false;
            }

            return;
        }

        TryDealPlayerDamage(centerTransform, armAttackRange);
    }

    private void TryDealPlayerDamage(Transform transform, float radius)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, radius, hitColliders, playerLayer);

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

    private void OnDrawGizmosSelected()
    {
        if (debug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(centerTransform.position, armAttackRange);
        }
    }

    //Zombie Takes Damage by given value
    public void TakeDamage(float value)
    {
        if (IsDead) return;

        health -= value;
        health = Mathf.Max(health, 0);

        /*if (zombieAudioSource.isPlaying)
        {
            zombieAudioSource.Stop();
        }
        SoundManager.Instance.PlayZombieHitSound(zombieAudioSource); */

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
        GameData.Money += deathPoints;
        GameEventSystem.OnMoneyUpdated?.Invoke();

        StartCoroutine(StartRagdoll(1f));
    }

    public void Dissolve()
    {
        detachController.DissolveZombie();
    }

    //Start OutOfGround Animation
    public void StartOutOfGroundAnimation()
    {
        StopAgent();
        StopMovement(true);
        startOutOfGround = true;
        zombieAnimations.SetOutOfGroundBool(true);
    }

    public void ContinueMovement()
    {
        StartAgent(CurrentMovementState);
        StopMovement(false);
        startOutOfGround = false;
    }

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

    public void SetTarget<T>(T target)
    {
        switch (target)
        {
            case Player player:
                this.player = player;
                break;
            case Vector3 targetTransform:
                this.targetTransform = targetTransform;
                break;
            case null:
                Debug.Log("Das Objekt ist null.");
                break;
            default:
                Debug.Log($"Unbekannter Typ: {typeof(T)}");
                break;
        }
    }

    public void SetMainMenuState(bool state)
    {
        mainMenuState = state;
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

    public void BarricadeReached(BarricadeController barricade)
    {
        this.barricade = barricade;

        if (barricade != null && !barricade.GetAllPlanksDestroyed())
        {
            isAttackingBarricade = true;
        }
    }

    public void SetBarricadePlayerAttacking(bool state)
    {
        isAttackingPlayerBarricade = state;
    }

    public void ClearTargetBarricade()
    {
        this.barricade = null;
        zombieAnimations.SetWindowAttack(false);
        isAttackingPlayerBarricade = false;
        isAttackingBarricade = false;
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
