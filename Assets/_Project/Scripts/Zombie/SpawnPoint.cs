using UnityEngine;

public class SpawnPoint : MonoBehaviour {

    [SerializeField] private BoxCollider spawnZone;

    public Vector3 GetRandomSpawnPosition() {
        Bounds bounds = spawnZone.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = bounds.min.y;
        float z = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(x, y, z);
    }
}
