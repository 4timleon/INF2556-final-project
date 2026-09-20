using System.Collections;
using UnityEngine;

public class StartGameSoundEffect : MonoBehaviour {

    public static StartGameSoundEffect Instance { get; private set; }

    [SerializeField] private AudioSource startGameSource;
    [SerializeField] private AudioListener startGameListener;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound() {
        SetAudioListener(true);
        startGameSource.Play();

        StartCoroutine(CheckSound());
    }

    private IEnumerator CheckSound() {
        while (startGameSource.isPlaying) {
            yield return null;
        }

        SetAudioListener(false);
    }

    public void SetAudioListener(bool state) {
        startGameListener.enabled = state;
    }
}
