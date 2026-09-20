using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class BarricadeController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int plankCount = 5;
    [SerializeField] private float repairAnimationDuration = 1f;
    [SerializeField] private float breakForce = 2.5f;

    [Header("References")]
    [SerializeField] private Animator[] plankAnimators;
    [SerializeField] private Rigidbody[] rigidbodies;

    [Header("NavMesh Link")]
    [SerializeField] private NavMeshLink navMeshLink;

    public bool PlayerIsRepairing { get; private set; } = false;
    public bool PlayerInRange { get; private set; } = false;
    public bool PlayerIsInteracting { get; private set; } = false;
    private bool[] plankBroken;
    private float cooldownTimer;

    private bool interactTextShown = false;

    private Zombie zombie;

    // Startwerte, damit die Planke beim Reparieren zurueckgesetzt werden kann
    private Vector3[] startPositions;
    private Quaternion[] startRotations;

    private void Awake()
    {
        plankBroken = new bool[plankCount];
        startPositions = new Vector3[plankCount];
        startRotations = new Quaternion[plankCount];

        for (int i = 0; i < plankCount; i++)
        {
            if (rigidbodies[i] == null) continue;

            Transform t = rigidbodies[i].transform;
            startPositions[i] = t.localPosition;
            startRotations[i] = t.localRotation;

            rigidbodies[i].isKinematic = true;
        }
    }

    private IEnumerator Start()
    {
        yield return null;

        if (navMeshLink != null)
        {
            navMeshLink.UpdateLink();
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (!PlayerIsInteracting)
        {
            PlayerIsRepairing = false;
            return;
        }

        if (cooldownTimer > 0f) return;

        int targetIndex = GetLowestBrokenPlankIndex();
        if (targetIndex == -1)
        {
            PlayerIsRepairing = false;
            return;
        }

        RepairPlank(targetIndex);
        cooldownTimer = repairAnimationDuration;
    }

    private int GetLowestBrokenPlankIndex()
    {
        for (int i = 0; i < plankBroken.Length; i++)
        {
            if (plankBroken[i]) return i;
        }
        return -1;
    }

    private void RepairPlank(int index)
    {
        plankBroken[index] = false;

        Rigidbody rb = rigidbodies[index];
        if (rb != null)
        {
            // erst Velocities loeschen, solange der Body noch dynamisch ist
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // dann Physik abschalten
            rb.isKinematic = true;

            rb.transform.localPosition = startPositions[index];
            rb.transform.localRotation = startRotations[index];
        }

        if (plankAnimators != null && index < plankAnimators.Length && plankAnimators[index] != null)
        {
            plankAnimators[index].SetTrigger("Repair");
            GameData.Money += 10;
        }

    }

    public void BreakNextPlank()
    {
        for (int i = plankBroken.Length - 1; i >= 0; i--)
        {
            if (!plankBroken[i])
            {
                plankBroken[i] = true;

                if (rigidbodies[i] != null)
                {
                    rigidbodies[i].isKinematic = false;
                    rigidbodies[i].AddForce(transform.forward * breakForce, ForceMode.Impulse);
                }

                if (!interactTextShown && PlayerIsInteracting)
                {
                    GameEventSystem.OnShowRepairText?.Invoke(true);
                    interactTextShown = true;
                }

                return;
            }
        }
    }

    public bool GetAllPlanksDestroyed()
    {
        for (int i = 0; i < plankBroken.Length; i++)
        {
            if (!plankBroken[i]) return false;
        }
        return true;
    }

    private bool GetAnyPlankDestroyed()
    {
        for (int i = 0; i < plankBroken.Length; i++)
        {
            if (plankBroken[i]) return true;
        }
        return false;
    }

    public void OnInteractStarted()
    {
        if (GetAnyPlankDestroyed())
        {
            PlayerIsInteracting = true;
            PlayerIsRepairing = true;
        }
    }

    public void OnInteractEnded()
    {
        PlayerIsInteracting = false;
        PlayerIsRepairing = false;
    }

    public string GetText() => GetAnyPlankDestroyed()
    ? "Hold <sprite name=\"keyboard_e\"> to repair Barricades"
    : $"All Barricades repaired!";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInRange = true;
        }

        zombie = other.GetComponentInParent<Zombie>();
        if (zombie != null)
        {
            // nur wenn der Zombie auf der Aussenseite steht
            Vector3 toZombie = zombie.transform.position - transform.position;
            if (Vector3.Dot(toZombie, transform.forward) > 0f)
            {
                zombie.BarricadeReached(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInRange = false;
        }

        Zombie zombie = other.GetComponentInParent<Zombie>();
        if (zombie != null && !zombie.IsAttackingBarricade)
        {
            zombie.ClearTargetBarricade();
        }
        this.zombie = null;
    }
}