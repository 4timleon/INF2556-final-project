using System;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.Pool;
using WeaponNamespace;

public class SoundManager : MonoBehaviour
{

    //private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private float volume = 1f;

    //Sound Object Pool
    private IObjectPool<SoundEmitter> soundEmitterPool;
    public readonly List<SoundEmitter> activeSoundEmitter = new();
    public readonly Queue<SoundEmitter> FrequentSoundEmitters = new();

    [SerializeField] private SoundEmitter soundEmitterPrefab;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private int maxSoundInstances = 30;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        //volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
    }

    private void Start()
    {
        InitializePool();
    }

    private void OnEnable()
    {
        GameEventSystem.OnPlayerDeath += GameEventSystem_OnPlayerDeath;
    }

    private void OnDisable()
    {
        GameEventSystem.OnPlayerDeath -= GameEventSystem_OnPlayerDeath;
    }

    private void GameEventSystem_OnPlayerDeath(float deathCounter)
    {
        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, volume, 0f, 5f)
            .setOnUpdate((float value) =>
            {
                volume = value;
            })
            .setIgnoreTimeScale(true);
    }

    #region Sound Pool
    public SoundBuilder CreateSound() => new SoundBuilder(this);

    public bool CanPlaySound(SoundData data)
    {
        if (!data.frequentSound) return true;

        if (FrequentSoundEmitters.Count >= maxSoundInstances && FrequentSoundEmitters.TryDequeue(out var soundEmitter))
        {
            try
            {
                soundEmitter.Stop();
                return true;
            }
            catch
            {
                Debug.Log("SoundEmitter is already released");
            }
            return false;
        }

        return true;
    }

    public SoundEmitter Get()
    {
        return soundEmitterPool.Get();
    }

    public void ReturnToPool(SoundEmitter soundEmitter)
    {
        soundEmitterPool.Release(soundEmitter);
    }

    private SoundEmitter CreateSoundEmitter()
    {
        var soundEmitter = Instantiate(soundEmitterPrefab);
        soundEmitter.gameObject.SetActive(false);
        return soundEmitter;
    }

    private void OnTakeFromPool(SoundEmitter soundEmitter)
    {
        soundEmitter.gameObject.SetActive(true);
        activeSoundEmitter.Add(soundEmitter);
    }

    private void OnReturnedToPool(SoundEmitter soundEmitter)
    {
        soundEmitter.gameObject.SetActive(false);
        activeSoundEmitter.Remove(soundEmitter);
    }

    private void OnDestroyPoolObject(SoundEmitter soundEmitter)
    {
        Destroy(soundEmitter.gameObject);
    }

    private void InitializePool()
    {
        soundEmitterPool = new ObjectPool<SoundEmitter>(
            CreateSoundEmitter,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            collectionCheck,
            defaultCapacity,
            maxPoolSize);
    }
    #endregion

    #region Player Sounds
    public void PlayHitSound(AudioSource audioSource)
    {
        PlaySound(audioSource, audioClipRefsSO.playerHitSounds, audioClipRefsSO.playerHitVolume);
    }

    public void PlayHurtSound(AudioSource audioSource)
    {
        PlaySound(audioSource, audioClipRefsSO.playerHurtSounds, audioClipRefsSO.playerHurtVolume);
    }

    public void PlayLowHPSound(AudioSource audioSource)
    {
        PlaySound(audioSource, audioClipRefsSO.heartbeatSound, audioClipRefsSO.heartbeatVolume);
    }

    public void PlayPlayerRevivedSound(AudioSource audioSource)
    {
        PlaySound(audioSource, audioClipRefsSO.revivedSound, audioClipRefsSO.revivedSoundVolume);
    }

    public void PlayReloadEjectSound(AudioSource audioSource, Vector3 position, WeaponType weaponType)
    {
        AudioClip clip = null;
        float volume = 0f;
        switch (weaponType)
        {
            case WeaponType.Pistol:
                clip = audioClipRefsSO.pistolReloadEjectSound;
                volume = audioClipRefsSO.pistolReloadSoundVolume;
                break;
            case WeaponType.AssaultRifle:
                clip = audioClipRefsSO.assaultReloadEjectSound;
                volume = audioClipRefsSO.assaultReloadSoundVolume;
                break;
            case WeaponType.SubmachineGun:
                clip = audioClipRefsSO.smgReloadEjectSound;
                volume = audioClipRefsSO.smgReloadSoundVolume;
                break;
            case WeaponType.Shotgun:
                //clip = audioClipRefsSO.shotgunReloadEjectSound;
                //volume = audioClipRefsSO.shotgunReloadSoundVolume;
                break;
            case WeaponType.SniperRifle:
                clip = audioClipRefsSO.sniperReloadEjectSound;
                volume = audioClipRefsSO.sniperReloadSoundVolume;
                break;
        }

        PlaySound(audioSource, clip, position, volume);
    }

    public void PlayReloadPocketSound(AudioSource audioSource, Vector3 position)
    {
        PlaySound(audioSource, audioClipRefsSO.pocketSearchSound, position, audioClipRefsSO.pocketSearchSoundVolume);
    }

    public void PlayReloadHomeSound(AudioSource audioSource, Vector3 position, WeaponType weaponType)
    {
        AudioClip clip = null;
        float volume = 0f;
        switch (weaponType)
        {
            case WeaponType.Pistol:
                clip = audioClipRefsSO.pistolReloadHomeSound;
                volume = audioClipRefsSO.pistolReloadSoundVolume;
                break;
            case WeaponType.AssaultRifle:
                clip = audioClipRefsSO.assaultReloadHomeSound;
                volume = audioClipRefsSO.assaultReloadSoundVolume;
                break;
            case WeaponType.SubmachineGun:
                clip = audioClipRefsSO.smgReloadHomeSound;
                volume = audioClipRefsSO.smgReloadSoundVolume;
                break;
            case WeaponType.Shotgun:
                clip = audioClipRefsSO.shotgunReloadHomeSound;
                volume = audioClipRefsSO.shotgunReloadSoundVolume;
                break;
            case WeaponType.SniperRifle:
                clip = audioClipRefsSO.sniperReloadHomeSound;
                volume = audioClipRefsSO.sniperReloadSoundVolume;
                break;
        }

        PlaySound(audioSource, clip, position, volume);
    }
    #endregion

    #region Zombie Sounds
    public void PlayZombieHitSound(AudioSource audioSource)
    {
        PlaySound(audioSource, audioClipRefsSO.zombieHitSounds, audioClipRefsSO.zombieHitVolume);
    }

    public void PlayZombieAttackSound(AudioSource audioSource)
    {
        PlaySound(audioSource, audioClipRefsSO.zombieAttackSounds, audioClipRefsSO.zombieAttackVolume);
    }

    public void PlayZombieChaseSound(AudioSource audioSource)
    {
        PlaySound(audioSource, audioClipRefsSO.zombieChaseSounds, audioClipRefsSO.zombieChaseVolume);
    }
    #endregion

    #region UI Sounds
    public void PlayCardSlideSound(AudioSource audioSource)
    {
        PlaySound(audioSource, audioClipRefsSO.cardSlide, audioClipRefsSO.cardSlideVolume);
    }
    #endregion
    //Play a random clip in the audio Clip Array
    private void PlaySound(AudioSource audioSource, AudioClip[] audioClipArray, float volume = 1f)
    {
        PlaySound(audioSource, audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)], volume);
    }

    //Play one Clip once
    private void PlaySound(AudioSource audioSource, AudioClip audioClip, float volumeMultiplier = 1f)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volumeMultiplier * volume;
        audioSource.Play();
    }

    //Play one Clip once at a certain Location
    private void PlaySound(AudioSource audioSource, AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        audioSource.transform.position = position;
        audioSource.clip = audioClip;
        audioSource.volume = volumeMultiplier * volume;
        audioSource.Play();
    }
}
