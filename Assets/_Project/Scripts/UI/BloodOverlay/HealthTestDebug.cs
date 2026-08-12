using UnityEngine;
using UnityEngine.InputSystem;

public class HealthTestDebug : MonoBehaviour
{
    private void Start()
    {
        GameData.PlayerHealth = GameData.PlayerMaxHealth; // Start bei vollem Leben
        GameEventSystem.OnPlayerHealthChanged?.Invoke(GameData.PlayerHealth, GameData.PlayerMaxHealth);
    }

    private void Update()
    {
        if (Keyboard.current.minusKey.wasPressedThisFrame)
        {
            GameData.PlayerHealth = Mathf.Max(0, GameData.PlayerHealth - 10);
            GameEventSystem.OnPlayerHealthChanged?.Invoke(GameData.PlayerHealth, GameData.PlayerMaxHealth);
            Debug.Log($"Health: {GameData.PlayerHealth}"); // <- testweise
        }

        if (Keyboard.current.equalsKey.wasPressedThisFrame)
        {
            GameData.PlayerHealth = Mathf.Min(GameData.PlayerMaxHealth, GameData.PlayerHealth + 10);
            GameEventSystem.OnPlayerHealthChanged?.Invoke(GameData.PlayerHealth, GameData.PlayerMaxHealth);
        }
    }
}