using UnityEngine;
using UnityEngine.InputSystem;

public class WaveTestDebug : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("Taste 2 gedrückt");
            GameData.Wave++;
            GameEventSystem.OnWaveStarted?.Invoke();
        }
    }
}