using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string doorId;      // eindeutige ID, z.B. "Door_A1"
    [SerializeField] private int cost = 500;

    [Header("References")]
    [SerializeField] private Collider blockingCollider; // der Mesh Collider vom Tuerblatt
    [SerializeField] private Animator doorAnimator;

    private bool playerInRange;
    private bool isOpen;

    private void Update()
    {
        if (isOpen || !playerInRange) return;

        if (GameControls.GetInputActions().Player.Interact.WasPressedThisFrame())
        {
            TryOpenDoor();
        }
    }

    private void TryOpenDoor()
    {
        if (GameData.Money < cost)
        {
            // TODO: UI Feedback "Nicht genug Geld"
            return;
        }

        GameData.Money -= cost;
        OpenDoor();
    }

    private void OpenDoor()
    {
        isOpen = true;

        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");

        if (blockingCollider != null)
            blockingCollider.enabled = false;

        GameEventSystem.OnDoorOpened?.Invoke(doorId);
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