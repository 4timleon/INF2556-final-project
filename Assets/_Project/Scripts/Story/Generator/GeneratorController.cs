using UnityEngine;

public class GeneratorController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int generatorCost = 500;

    [Header("References")]
    [SerializeField] private Animator generatorAnimator;

    private bool playerInRange;
    private bool isActivated;

    private void Update()
    {
        if (isActivated || !playerInRange) return;

        if (GameControls.GetInputActions().Player.Interact.WasPressedThisFrame())
        {
            TryActivate();
        }
    }

    // Einziger Einstiegspunkt für die Aktivierung.
    // Spaeter (Multiplayer): hier waere die Stelle fuer eine
    // "nur Server darf das wirklich ausfuehren" Pruefung (z.B. RPC an den Host).
    
    private void TryActivate()
    {
        if (GameData.Money < generatorCost)
        {
            // TODO: UI Feedback "Nicht genug Geld"
            return;
        }

        GameData.Money -= generatorCost;
        Activate();
    }

    private void Activate()
    {
        isActivated = true;

        GameData.IsPowerOn = true;

        if (generatorAnimator != null)
            generatorAnimator.SetTrigger("Activate");

        GameEventSystem.OnPowerTurnedOn?.Invoke();
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