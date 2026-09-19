using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class WaveManager : MonoBehaviour, IInititalizeStart
{

    [SerializeField] private Player playerReference;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private List<SpawnPoint> spawnPoints;
    [SerializeField] private float timeBetweenSpawns = 1.0f;

    [Header("Wave Scaling")]
    [SerializeField] private int baseZombieCount = 6;
    [SerializeField] private int maxSimultaneousZombies = 60;
    [SerializeField] private float zombiesMultiplier = 1.3f;

    [Header("Zombie Health Scaling")]
    [SerializeField] private float healthIncreasePerWave = 25f;

    [Header("Probability Curves (X = Wave, Y = Weight)")]
    [SerializeField] private AnimationCurve slowWeightCurve = AnimationCurve.Linear(1, 100, 10, 5);
    [SerializeField] private AnimationCurve fastWeightCurve = AnimationCurve.Linear(1, 0, 10, 50);
    [SerializeField] private AnimationCurve runWeightCurve = AnimationCurve.Linear(1, 0, 10, 80);

    [Header("Current State (Debug), Default: 1")]
    [SerializeField] private int currentWave = 1;
    private bool isSpawning = false;

    private int maxZombiesToSpawn;
    private int remainingZombiesToSpawn;

    //Zombies of the current Wave
    private List<Zombie> currentZombies = new();

    //Coroutine
    private Coroutine spawnTimerCoroutine;

    public int StartPriority => 1000;

    private void Awake()
    {
        GameData.Wave = currentWave;
    }

    public async Awaitable InititalizeStart(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        await Awaitable.NextFrameAsync(ct);

        GameEventSystem.OnWaveStarted += GameEventSystem_OnWaveStarted;

        maxZombiesToSpawn = 0;
    }

    private void GameEventSystem_OnWaveStarted()
    {
        StartNextWave();
    }

    private void GameEventSystem_OnDoorOpened(DoorController controller)
    {
        if (controller.spawnPoints.Count > 0)
            spawnPoints.AddRange(controller.spawnPoints);
    }

    //Can be used to Start the next Wave after all Zombies got killed
    public void StartNextWave()
    {
        currentWave++;
        GameData.Wave = currentWave;

        //Max Zombies at the current Wave
        remainingZombiesToSpawn = Mathf.RoundToInt(baseZombieCount * Mathf.Pow(zombiesMultiplier, currentWave - 1));
        Debug.Log($"Wave {currentWave} started, Zombies spawned: {remainingZombiesToSpawn}");
        maxZombiesToSpawn = remainingZombiesToSpawn;

        isSpawning = true;
        TrySpawnWaveRoutine();
    }

    private void TrySpawnWaveRoutine()
    {
        if (!isSpawning || remainingZombiesToSpawn <= 0) return;
        if (currentZombies.Count >= maxSimultaneousZombies) return;

        //Spawn Zombie
        Zombie.MovementState zombieType = GetRandomZombieSpeed(currentWave);
        SpawnZombie(zombieType);
        remainingZombiesToSpawn--;
        if (remainingZombiesToSpawn <= 0)
        {
            isSpawning = false;
            return;
        }

        //Debug.Log("Spawning a Zombie! : " + currentZombies.Count);

        if (spawnTimerCoroutine != null) StopCoroutine(spawnTimerCoroutine);
        spawnTimerCoroutine = StartCoroutine(SpawnDelayRoutine());
    }

    private IEnumerator SpawnDelayRoutine()
    {
        yield return new WaitForSeconds(timeBetweenSpawns);
        TrySpawnWaveRoutine();
    }

    //Calculates the weights of the Zombies depending on the current Wave
    //The more Waves the more faster and less slower Zombies appear
    private Zombie.MovementState GetRandomZombieSpeed(int wave)
    {
        float slowWeight = Mathf.Max(0f, slowWeightCurve.Evaluate(wave));
        float fastWeight = Mathf.Max(0f, fastWeightCurve.Evaluate(wave));
        float runWeight = Mathf.Max(0f, runWeightCurve.Evaluate(wave));

        float totalWeight = slowWeight + fastWeight + runWeight;

        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < slowWeight)
        {
            return Zombie.MovementState.Walk;
        }
        else if (randomValue < slowWeight + fastWeight)
        {
            return Zombie.MovementState.Fast;
        }
        else
        {
            return Zombie.MovementState.Run;
        }
    }

    private void SpawnZombie(Zombie.MovementState type)
    {
        SpawnPoint spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        Vector3 randomSpawnPoint = spawnPoint.GetRandomSpawnPosition();

        Zombie zombie = Instantiate(zombiePrefab, randomSpawnPoint, Quaternion.identity).GetComponent<Zombie>();
        currentZombies.Add(zombie);

        zombie.SetTarget(playerReference);
        zombie.SetSpeed(type);

        float healthForCurrentWave = zombie.BaseHealth + healthIncreasePerWave * (currentWave - 1);
        zombie.SetHealth(healthForCurrentWave);

        if (spawnPoint.OutOfGround)
        {
            zombie.StartOutOfGroundAnimation();
        }
        else
        {
            zombie.StartMovement();
        }
    }

    public void RemoveZombieFromList(Zombie zombie)
    {
        currentZombies.Remove(zombie);
        maxZombiesToSpawn--;

        TrySpawnWaveRoutine();

        if (currentZombies.Count == 0 && maxZombiesToSpawn == 0)
        {
            GameEventSystem.OnWaveEnded?.Invoke();
        }
    }

    private void OnEnable()
    {
        GameEventSystem.OnEnemyKilled += RemoveZombieFromList;
        GameEventSystem.OnDoorOpened += GameEventSystem_OnDoorOpened;
    }

    private void OnDisable()
    {
        GameEventSystem.OnEnemyKilled -= RemoveZombieFromList;
        GameEventSystem.OnWaveStarted -= GameEventSystem_OnWaveStarted;
        GameEventSystem.OnDoorOpened -= GameEventSystem_OnDoorOpened;
    }
}
