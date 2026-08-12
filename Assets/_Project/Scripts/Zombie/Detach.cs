using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Detach : MonoBehaviour {

    [SerializeField] private SkinnedMeshRenderer mesh;
    [SerializeField] private BodyPartType bodyPart;
    [SerializeField] private GameObject detachedPrefab;
    [SerializeField] private float pushForce = 3f;
    [SerializeField] private Vector3 pushDirection = new Vector3(-1f, 0.2f, 0f);

    public float DamageTaken { get; private set; }
    public bool IsDetached { get; private set; } = false;
    public BodyPartType BodyPartType => bodyPart;

    //Hit Variables for Debug
    private Color originalColor;
    private Material zombieMaterial;
    private readonly Color hitColor = Color.red;

    private DetachController detachController;

    private void Start() {
        detachController = GetComponentInParent<DetachController>();//Remove Later after shooting implemented

        if (mesh != null) {
            zombieMaterial = mesh.material;
            originalColor = zombieMaterial.color;
        }
    }

    public void TakeBodyPartDamage(float value) {
        DamageTaken += value;

        StopAllCoroutines();
        StartCoroutine(HitFeedback());
    }

    private IEnumerator HitFeedback() {
        zombieMaterial.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        zombieMaterial.color = originalColor;
    }

    public void DetachMesh() {
        mesh.enabled = false;

        GameObject newMesh = Instantiate(detachedPrefab, mesh.bounds.center, mesh.rootBone.rotation);
        Rigidbody rb = newMesh.GetComponentInChildren<Rigidbody>();

        if (rb != null) {
            rb.AddForce(pushDirection.normalized * pushForce, ForceMode.Impulse);
        }

        IsDetached = true;

        //Remove Later after shooting implemented
        detachController.RemoveFromList(this);
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
