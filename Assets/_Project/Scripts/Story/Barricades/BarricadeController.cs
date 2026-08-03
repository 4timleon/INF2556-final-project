using UnityEngine;

public class BarricadeController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int plankCount = 5;
    [SerializeField] private float repairAnimationDuration = 1f;

    [Header("References")]
    [SerializeField] private GameObject[] plankVisuals;

    private bool[] plankBroken;
    private float cooldownTimer;
    private bool playerInRange;

    private void Awake()
    {
        plankBroken = new bool[plankCount];
    }

    private void Update()
    {
        if (!playerInRange) return;

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        int targetIndex = GetLowestBrokenPlankIndex();
        if (targetIndex == -1) return; // All planks are repaired

        var interact = GameControls.GetInputActions().Player.Interact;

        //Hold or press to repair
        if (interact.IsPressed() || interact.WasPressedThisFrame())
        {
            RepairPlank(targetIndex);
            cooldownTimer = repairAnimationDuration;
        }
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

        if (plankVisuals != null && index < plankVisuals.Length && plankVisuals[index] != null)
            plankVisuals[index].SetActive(true);

        // TODO: hier ggf. Reparatur-Animation/Sound triggern
    }

    public void BreakNextPlank()
    {
        for(int i = plankBroken.Length - 1; i >= 0; i--)
        {
            if (!plankBroken[i])
            {
                plankBroken[i] = true;

                if (plankVisuals != null && i < plankVisuals.Length && plankVisuals[i] != null)
                    plankVisuals[i].SetActive(false);

                return;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}