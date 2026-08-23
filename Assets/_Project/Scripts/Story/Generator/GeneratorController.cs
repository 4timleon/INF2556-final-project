using UnityEngine;

public class GeneratorController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int generatorCost = 500;

    [Header("References")]
    [SerializeField] private Animator generatorAnimator;

    private bool playerInRange;
    private bool hasBeenPaid;   // einmal bezahlt, danach frei schaltbar

    private void Update()
    {
        if (!playerInRange) return;

        if (GameControls.GetInputActions().Player.Interact.WasPressedThisFrame())
        {
            TryToggle();
        }
    }

    private void TryToggle()
    {
        // Beim ersten Mal muss bezahlt werden
        if (!hasBeenPaid)
        {
            if (GameData.Money < generatorCost)
            {
                // TODO: UI Feedback "Nicht genug Geld"
                return;
            }

            GameData.Money -= generatorCost;
            hasBeenPaid = true;
        }

        SetPower(!GameData.IsPowerOn);
    }

    private void SetPower(bool state)
    {
        GameData.IsPowerOn = state;

        if (generatorAnimator != null)
            generatorAnimator.SetBool("IsOn", state);

        GameEventSystem.OnPowerChanged?.Invoke(state);
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