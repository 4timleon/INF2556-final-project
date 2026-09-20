using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GeneratorController : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private int cost = 500;

    [Header("Sound")]
    [SerializeField] private AudioSource generatorOnAudioSource;
    [SerializeField] private AudioSource generatorLoopAudioSource;

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

    private void OnEnable()
    {
        GameEventSystem.OnGamePause += OnGamePaused;
    }

    private void OnDisable()
    {
        GameEventSystem.OnGamePause -= OnGamePaused;
    }

    private void SetPower(bool state)
    {
        GameData.IsPowerOn = state;

        GameEventSystem.OnPowerChanged?.Invoke(state);

        if (state)
        {
            StartCoroutine(PlaySound());
        }
        else
        {
            generatorOnAudioSource.Stop();
            generatorLoopAudioSource.Stop();
        }

    }

    public void OnInteractStarted() => TryToggle();

    public string GetText() => hasBeenPaid
    ? $"Press <sprite name=\"keyboard_e\"> to turn Power {ToggleString()}"
    : $"Press <sprite name=\"keyboard_e\"> to unlock Generator | {cost}$";

    string ToggleString() => GameData.IsPowerOn ? "off" : "on";

    private IEnumerator PlaySound()
    {
        SoundManager.Instance?.PlayGeneratorOnSound(generatorOnAudioSource);

        yield return null;
        yield return new WaitUntil(() => !generatorOnAudioSource.isPlaying);

        SoundManager.Instance?.PlayGeneratorLoopSound(generatorLoopAudioSource);
    }

    private void OnGamePaused(bool state)
    {
        if (state)
            generatorLoopAudioSource.Pause();
        else
            generatorLoopAudioSource.Play();
    }
}