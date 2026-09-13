using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour {

    private const string AUDIOMIXER_MASTERVOLUME = "masterVolume";
    private const string AUDIOMIXER_MUSICVOLUME = "musicVolume";
    private const string AUDIOMIXER_SOUNDFXVOLUME = "soundFXVolume";


    [SerializeField] private AudioMixer audioMixer;

    public void SetMasterVolume(float level) {
        float clampedLevel = Mathf.Clamp(level, 0.0001f, 1f);
        audioMixer.SetFloat(AUDIOMIXER_MASTERVOLUME, Mathf.Log10(clampedLevel) * 20f);
    }

    public void SetMusicVolume(float level) {
        float clampedLevel = Mathf.Clamp(level, 0.0001f, 1f);
        audioMixer.SetFloat(AUDIOMIXER_MUSICVOLUME, Mathf.Log10(clampedLevel) * 20f);
    }

    public void SetSoundFXVolume(float level) {
        float clampedLevel = Mathf.Clamp(level, 0.0001f, 1f);
        audioMixer.SetFloat(AUDIOMIXER_SOUNDFXVOLUME, Mathf.Log10(clampedLevel) * 20f);
    }
}
