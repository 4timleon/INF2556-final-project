using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string doorId;
    [SerializeField] private int doorCost = 500;

    [Header("References")]
    [SerializeField] private Collider blockingCollider;
    [SerializeField] private Animator doorAnimator;

    private bool playerInRange;
    private bool isDoorOpen;

    private void Update()
    {
        if (isDoorOpen || !playerInRange) return;

        if (GameControls.GetInputActions().Player.Interact.WasPressedThisFrame())
        {
            TryOpenDoor();
        }
    }

    private void TryOpenDoor()
    {
        if (GameData.Money < doorCost)
        {
            // TODO: UI Feedback "Nicht genug Geld" (später)
            Debug.Log("Not enough money to open the door.");
            return;
        }

        GameData.Money -= doorCost;
        OpenDoor();
    }

    private void OpenDoor()
    {
        isDoorOpen = true;
        if(blockingCollider != null)
            blockingCollider.enabled = false;
        
        if(doorAnimator != null)
            doorAnimator.SetTrigger("Open");
        
        GameEventSystem.OnDoorOpened?.Invoke(doorId);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
