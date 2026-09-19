using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GeneratorController : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private int cost = 500;

    private bool hasBeenPaid;

    private void TryToggle()
    {
        if (!hasBeenPaid)
        {
            if (GameData.Money < cost)
            {
                return;
            }

            GameData.Money -= cost;
            hasBeenPaid = true;
        }

        SetPower(!GameData.IsPowerOn);
    }

    private void SetPower(bool state)
    {
        GameData.IsPowerOn = state;

        GameEventSystem.OnPowerChanged?.Invoke(state);
    }

    public void OnInteractStarted() => TryToggle();

    public string GetText() => hasBeenPaid
    ? $"Press <sprite name=\"keyboard_e\"> to turn Power {ToggleString()}"
    : $"Press <sprite name=\"keyboard_e\"> to unlock Generator | {cost}$";

    string ToggleString() => GameData.IsPowerOn ? "off" : "on";
}