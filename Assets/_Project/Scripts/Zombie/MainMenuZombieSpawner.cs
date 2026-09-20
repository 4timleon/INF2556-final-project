using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MainMenuZombieSpawner : MonoBehaviour
{
    [Header("Path/Spawn1")]
    [SerializeField] private Transform path1;
    [SerializeField] private Transform spawnPoint1;

    [Header("Path/Spawn2")]
    [SerializeField] private Transform path2;
    [SerializeField] private Transform spawnPoint2;

    [Header("Zombie")]
    [SerializeField] private Zombie zombiePrefab;

    private void Start()
    {
        StartCoroutine(RandomSpawn());
    }

    private IEnumerator RandomSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(8f, 15f));

            bool randomValue = Random.value > 0.5f;

            Transform randomSpawnPoint = randomValue ? spawnPoint1 : spawnPoint2;
            Transform randomPath = randomValue ? path1 : path2;

            NavMesh.SamplePosition(randomSpawnPoint.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas);

            Zombie zombieInstance = Instantiate(zombiePrefab, hit.position, Quaternion.identity);
            zombieInstance.SetTarget(randomPath.position);
            zombieInstance.SetMainMenuState(true);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
