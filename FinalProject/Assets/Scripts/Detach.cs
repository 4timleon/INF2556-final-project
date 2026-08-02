using UnityEngine;
using UnityEngine.InputSystem;

public class Detach : MonoBehaviour {

    [SerializeField] private SkinnedMeshRenderer headMesh;
    [SerializeField] private GameObject detachedHeadPrefab;

    private void Update() {
        DetachHead();
    }

    private void DetachHead() {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            headMesh.enabled = false;

            GameObject newHead = Instantiate(detachedHeadPrefab, headMesh.bounds.center, headMesh.rootBone.rotation);

            Rigidbody rb = newHead.GetComponentInChildren<Rigidbody>();

            rb.AddForce(-transform.forward * 5f, ForceMode.Impulse);
        }
    }
}
