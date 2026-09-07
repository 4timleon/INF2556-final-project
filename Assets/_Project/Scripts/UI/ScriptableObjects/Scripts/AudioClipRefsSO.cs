using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipRefsSO", menuName = "Scriptable Objects/AudioClipRefsSO")]
public class AudioClipRefsSO : ScriptableObject
{

    [Header("Zombie Chase Sound")]
    public AudioClip[] zombieChaseSounds;
    public float zombieChaseVolume;

    [Space]
    [Header("Zombie Attack Sound")]
    public AudioClip[] zombieAttackSounds;
    public float zombieAttackVolume;

    [Space]
    [Header("Zombie Hit Sound")]
    public AudioClip[] zombieHitSounds;
    public float zombieHitVolume;

    [Space]
    [Header("Heartbeat Sound")]
    public AudioClip heartbeatSound;
    public float heartbeatVolume;

    [Space]
    [Header("Player Hit Sound")]
    public AudioClip[] playerHitSounds;
    public float playerHitVolume;

    [Space]
    [Header("Card Sounds")]
    public AudioClip cardSlide;
    public float cardSlideVolume;
}
