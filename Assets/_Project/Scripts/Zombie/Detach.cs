using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Detach : MonoBehaviour {

    [SerializeField] private SkinnedMeshRenderer mesh;
    [SerializeField] private BodyPartType bodyPart;
    [SerializeField] private GameObject detachedPrefab;
    [SerializeField] private float pushForce = 3f;
    [SerializeField] private Vector3 pushDirection = new Vector3(-1f, 0.2f, 0f);

    private ZombieAnimations zombieAnimations;
    private float damageThreshold;
    private bool isDetached = false;

    //Hit Variables for Debug
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Color originalColor;
    private Material zombieMaterial;
    private readonly Color hitColor = Color.red;

    private void Start() {
        zombieAnimations = GetComponentInParent<ZombieAnimations>();
        skinnedMeshRenderer.GetComponent<SkinnedMeshRenderer>();

        if (skinnedMeshRenderer != null) {
            zombieMaterial = skinnedMeshRenderer.material;
            originalColor = zombieMaterial.color;
        }

    }

    private void Update() {
        //DetachMesh();
    }

    public void Hit() {
        if (isDetached) return;

        switch (bodyPart) {
            case BodyPartType.Head:
                break;
        }

        StopAllCoroutines();
        StartCoroutine(HitFeedback());
    }

    private IEnumerator HitFeedback() {
        zombieMaterial.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        zombieMaterial.color = originalColor;
    }

    public void DetachMesh() {
        if (bodyPart != BodyPartType.LLegCalf) {
            return;
        } else {
            zombieAnimations.SetLostLeg();
        }

        if (damageThreshold >= 40 && isDetached == false) {
            mesh.enabled = false;

            GameObject newMesh = Instantiate(detachedPrefab, mesh.bounds.center, mesh.rootBone.rotation);
            Rigidbody rb = newMesh.GetComponentInChildren<Rigidbody>();

            if (rb != null) {
                rb.AddForce(pushDirection.normalized * pushForce, ForceMode.Impulse);
            }
        }
    }

    private void DetachMesh(Vector3 pushDirection) {
        if (!Keyboard.current.spaceKey.wasPressedThisFrame) {
            return;
        }

        mesh.enabled = false;

        GameObject newMesh = Instantiate(detachedPrefab, mesh.bounds.center, mesh.rootBone.rotation);
        Rigidbody rb = newMesh.GetComponentInChildren<Rigidbody>();

        if (rb != null) {
            rb.AddForce(pushDirection.normalized * pushForce, ForceMode.Impulse);
        }
    }
}
