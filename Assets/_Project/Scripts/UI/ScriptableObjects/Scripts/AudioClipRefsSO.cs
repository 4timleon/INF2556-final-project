using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipRefsSO", menuName = "Scriptable Objects/AudioClipRefsSO")]
public class AudioClipRefsSO : ScriptableObject
{

    [Header("Zombie Chase Sounds")]
    public AudioClip[] zombieChaseSounds;
    public float zombieChaseVolume;

    [Space]
    [Header("Zombie Attack Sounds")]
    public AudioClip[] zombieAttackSounds;
    public float zombieAttackVolume;

    [Space]
    [Header("Zombie Hit Sounds")]
    public AudioClip[] zombieHitSounds;
    public float zombieHitVolume;

    [Space]
    [Header("Heartbeat Sound")]
    public AudioClip heartbeatSound;
    public float heartbeatVolume;

    [Space]
    [Header("Player Hit Sounds")]
    public AudioClip[] playerHitSounds;
    public float playerHitVolume;

    [Space]
    [Header("Player Hurt Sounds")]
    public AudioClip[] playerHurtSounds;
    public float playerHurtVolume;

    [Space]
    [Header("Card Sound")]
    public AudioClip cardSlide;
    public float cardSlideVolume;

    [Space]
    [Header("Revive Sound")]
    public AudioClip revivedSound;
    public float revivedSoundVolume;

    [Space]
    [Header("Reload Sounds")]
    public AudioClip pistolReloadEjectSound;
    public AudioClip pistolReloadHomeSound;
    public AudioClip assaultReloadEjectSound;
    public AudioClip assaultReloadHomeSound;
    public AudioClip smgReloadEjectSound;
    public AudioClip smgReloadHomeSound;
    //public AudioClip shotgunReloadEjectSound;
    public AudioClip shotgunReloadHomeSound;
    public AudioClip sniperReloadEjectSound;
    public AudioClip sniperReloadHomeSound;

    public float pistolReloadSoundVolume;
    public float assaultReloadSoundVolume;
    public float smgReloadSoundVolume;
    public float shotgunReloadSoundVolume;
    public float sniperReloadSoundVolume;

    public AudioClip pocketSearchSound;
    public float pocketSearchSoundVolume;
}
