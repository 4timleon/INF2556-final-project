using UnityEngine;

public class BarricadeController : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private int plankCount = 5;
    [SerializeField] private float repairAnimationDuration = 1f;
    [SerializeField] private float breakForce = 2.5f;

    [Header("References")]
    [SerializeField] private Animator[] plankAnimators;
    [SerializeField] private Rigidbody[] rigidbodies;

    private bool[] plankBroken;
    private float cooldownTimer;
    private bool playerInRange;

    // Startwerte, damit die Planke beim Reparieren zurueckgesetzt werden kann
    private Vector3[] startPositions;
    private Quaternion[] startRotations;

    private void Awake() {
        plankBroken = new bool[plankCount];
        startPositions = new Vector3[plankCount];
        startRotations = new Quaternion[plankCount];

        for (int i = 0; i < plankCount; i++) {
            if (rigidbodies[i] == null) continue;

            Transform t = rigidbodies[i].transform;
            startPositions[i] = t.localPosition;
            startRotations[i] = t.localRotation;

            rigidbodies[i].isKinematic = true;
        }
    }

    private void Update() {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (!playerInRange || cooldownTimer > 0f) return;

        int targetIndex = GetLowestBrokenPlankIndex();
        if (targetIndex == -1) return;

        var interact = GameControls.GetInputActions().Player.Interact;

        if (interact.IsPressed() || interact.WasPressedThisFrame()) {
            RepairPlank(targetIndex);
            cooldownTimer = repairAnimationDuration;
        }
    }

    private int GetLowestBrokenPlankIndex() {
        for (int i = 0; i < plankBroken.Length; i++) {
            if (plankBroken[i]) return i;
        }
        return -1;
    }

    private void RepairPlank(int index) {
        plankBroken[index] = false;

        Rigidbody rb = rigidbodies[index];
        if (rb != null) {
            // erst Velocities loeschen, solange der Body noch dynamisch ist
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // dann Physik abschalten
            rb.isKinematic = true;

            rb.transform.localPosition = startPositions[index];
            rb.transform.localRotation = startRotations[index];
        }

        if (plankAnimators != null && index < plankAnimators.Length && plankAnimators[index] != null)
            plankAnimators[index].SetTrigger("Repair");
    }

    public void BreakNextPlank() {
        for (int i = plankBroken.Length - 1; i >= 0; i--) {
            if (!plankBroken[i]) {
                plankBroken[i] = true;

                if (rigidbodies[i] != null) {
                    rigidbodies[i].isKinematic = false;
                    rigidbodies[i].AddForce(transform.forward * breakForce, ForceMode.Impulse);
                }

                return;
            }
        }
    }

    public bool GetAllPlanksDestroyed() {
        for (int i = 0; i < plankBroken.Length; i++) {
            if (!plankBroken[i]) return false;
        }
        return true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player"))
            playerInRange = true;

        Zombie zombie = other.GetComponentInParent<Zombie>();
        if (zombie != null) {
            // nur wenn der Zombie auf der Aussenseite steht
            Vector3 toZombie = zombie.transform.position - transform.position;
            if (Vector3.Dot(toZombie, transform.forward) > 0f) {
                zombie.BarricadeReached(this);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player"))
            playerInRange = false;

        Zombie zombie = other.GetComponentInParent<Zombie>();
        if (zombie != null && !zombie.IsAttackingBarricade) {
            zombie.ClearTargetBarricade();
        }
    }
}