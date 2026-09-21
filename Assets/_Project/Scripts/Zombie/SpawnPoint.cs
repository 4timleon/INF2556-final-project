using UnityEngine;
using UnityEngine.AI;

public class SpawnPoint : MonoBehaviour
{
    public enum ColliderType
    {
        Box,
        Sphere
    }

    [SerializeField] private Collider spawnZone;
    [SerializeField] private bool outOfGround = false;

    [Header("Collider Select")]
    [SerializeField] private ColliderType chosenCollider = ColliderType.Box;

    public bool OutOfGround => outOfGround;

    /* private void OnValidate()
    {
        if (Application.isPlaying) return;

        UnityEditor.EditorApplication.delayCall += UpdateCollider;
    } */

    private void UpdateCollider()
    {
        if (this == null) return;

        Collider[] existingColliders = GetComponents<Collider>();

        foreach (Collider collider in existingColliders)
        {
            if (chosenCollider == ColliderType.Box && collider is BoxCollider) return;
            if (chosenCollider == ColliderType.Sphere && collider is SphereCollider) return;

            DestroyImmediate(collider);
        }

        Collider newCollider = null;

        switch (chosenCollider)
        {
            case ColliderType.Box:
                newCollider = gameObject.AddComponent<BoxCollider>();
                break;
            case ColliderType.Sphere:
                newCollider = gameObject.AddComponent<SphereCollider>();
                break;
        }

        if (newCollider != null)
        {
            spawnZone = newCollider;
            newCollider.isTrigger = true;
        }
    }

    public Vector3 GetRandomSpawnPosition()
    {
        if (spawnZone == null) return Vector3.zero;

        Vector3 randomPoint = Vector3.zero;

        if (spawnZone is BoxCollider box)
        {
            Vector3 randomLocalPoint = new Vector3(
                Random.Range(-box.size.x / 2f, box.size.x / 2f + box.center.x),
                box.center.y,
                Random.Range(-box.size.z / 2f, box.size.z / 2f + box.center.z)
            );

            randomPoint = box.transform.TransformPoint(randomLocalPoint);
        }
        else if (spawnZone is SphereCollider sphere)
        {
            Vector2 randomCircle = Random.insideUnitCircle * sphere.radius;
            Vector3 center = sphere.transform.TransformPoint(sphere.center);
            float maxScale = Mathf.Max(sphere.transform.lossyScale.x, sphere.transform.lossyScale.z);

            randomPoint = center + new Vector3(randomCircle.x * maxScale, 0f, randomCircle.y * maxScale);
        }


        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return randomPoint;
    }
}
