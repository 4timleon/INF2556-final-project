using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WaveManager : MonoBehaviour, IInititalizeStart {

    [SerializeField] private Player playerReference;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private SpawnPoint[] spawnPoints;
    [SerializeField] private float timeBetweenSpawns = 1.0f;
    //[SerializeField] private float timeBetweenWaves = 5.0f;

    [Header("Wave Scaling")]
    [SerializeField] private int baseZombieCount = 6;
    [SerializeField] private float zombiesMultiplier = 1.3f;

    [Header("Probability Curves (X = Wave, Y = Weight)")]
    [SerializeField] private AnimationCurve slowWeightCurve = AnimationCurve.Linear(1, 100, 10, 5);
    [SerializeField] private AnimationCurve fastWeightCurve = AnimationCurve.Linear(1, 0, 10, 50);
    [SerializeField] private AnimationCurve runWeightCurve = AnimationCurve.Linear(1, 0, 10, 80);

    [Header("Current State")]
    [SerializeField] private int currentWave = 0;
    //private bool isSpawning = false;

    private int currentZombiesToSpawn = 0;

    //Zombies of the current Wave
    private List<Zombie> currentZombies = new();

    public int StartPriority => 1000;

    public async Awaitable InititalizeStart(CancellationToken ct) {
        ct.ThrowIfCancellationRequested();

        await Awaitable.NextFrameAsync(ct);

        StartNextWave();
    }

    //Can be used to Start the next Wave after all Zombies got killed
    public void StartNextWave() {
        GameEventSystem.OnWaveStarted?.Invoke();
        currentWave++;
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine() {
        //isSpawning = true;
        int zombiesToSpawn = Mathf.RoundToInt(baseZombieCount * Mathf.Pow(zombiesMultiplier, currentWave - 1));
        currentZombiesToSpawn = zombiesToSpawn;

        Debug.Log($"Wave {currentWave} started, Zombies spawned: {zombiesToSpawn}");

        for (int i = 0; i < zombiesToSpawn; i++) {
            Zombie.MovementState zombieType = GetRandomZombieSpeed(currentWave);
            SpawnZombie(zombieType);

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    //Calculates the weights of the Zombies depending on the current Wave
    //The more Waves the more faster and less slower Zombies appear
    private Zombie.MovementState GetRandomZombieSpeed(int wave) {
        float slowWeight = Mathf.Max(0f, slowWeightCurve.Evaluate(wave));
        float fastWeight = Mathf.Max(0f, fastWeightCurve.Evaluate(wave));
        float runWeight = Mathf.Max(0f, runWeightCurve.Evaluate(wave));

        float totalWeight = slowWeight + fastWeight + runWeight;

        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < slowWeight) {
            return Zombie.MovementState.Walk;
        } else if (randomValue < slowWeight + fastWeight) {
            return Zombie.MovementState.Fast;
        } else {
            return Zombie.MovementState.Run;
        }
    }

    private void SpawnZombie(Zombie.MovementState type) {
        SpawnPoint spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 randomSpawnPoint = spawnPoint.GetRandomSpawnPosition();

        Zombie zombie = Instantiate(zombiePrefab, randomSpawnPoint, Quaternion.identity).GetComponent<Zombie>();
        currentZombies.Add(zombie);

        zombie.SetTarget(playerReference);
        zombie.SetSpeed(type);

        if (spawnPoint.OutOfGround) {
            zombie.StartOutOfGroundAnimation();
        } else {
            zombie.StartMovement();
        }
    }

    public void RemoveZombieFromList(Zombie zombie) {
        currentZombies.Remove(zombie);
        currentZombiesToSpawn--;

        if (currentZombies.Count == 0 && currentZombiesToSpawn == 0) {
            GameEventSystem.OnWaveEnded?.Invoke();
            StartNextWave();
        }
    }

    private void OnEnable() {
        GameEventSystem.OnEnemyKilled += RemoveZombieFromList;
    }

    private void OnDisable() {
        GameEventSystem.OnEnemyKilled -= RemoveZombieFromList;
    }

}
