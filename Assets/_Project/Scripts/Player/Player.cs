using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private VolumeProfile globalVolumeProfile;
    [SerializeField] private VolumeProfile monochromeVolumeProfile;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float preDeathTimer = 10f;
    [SerializeField] private bool revive = false;
    [SerializeField] private float healDelay = 5f;
    [SerializeField] private float healTillFull = 3f;
    [SerializeField] private float invincibilityFrameDuration = 1f;

    [Header("Sound")]
    [SerializeField] private AudioSource lowHpAudioSource;
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioSource hurtAudioSource;
    [SerializeField] private AudioSource gameOverSoundSource;
    [SerializeField] private AudioSource reviveSoundSource;

    [Header("Weapons")]
    [SerializeField] private WeaponBuilder weaponBuilder;
    [SerializeField] private PlayerArmory playerArmory;

    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsInPreDeath { get; private set; } = false;
    public bool IsDead { get; private set; } = false;

    //Health
    private Coroutine healCoroutine;
    private float nextDamageTime = 0f;
    private float deathCounter;

    //Event
    public static event Action<float> OnPreDeathCounter;

    private void Start()
    {
        MaxHealth = maxHealth;
        Health = MaxHealth;

        revive = false;

        deathCounter = 0f;

        Invoke("GiveFirstWeapon", 1f);

        GameEventSystem.OnPerkAquired?.Invoke(PerkType.Revive);
    }

    private void OnEnable()
    {
        GameEventSystem.OnGamePause += GameEventSystem_OnGamePause;
    }

    private void OnDisable()
    {
        GameEventSystem.OnGamePause -= GameEventSystem_OnGamePause;
    }

    // testing -> weaponBuilder.BuildWeapon(true) -> 'true' für -> start with Pistol.
    private void GiveFirstWeapon()
    {
        playerArmory.AddWeapon(weaponBuilder.BuildWeapon(true));
    }

    public void ToggleReviveStatus(bool toggle) => revive = toggle;

    public void IncreaseMaxHealth(float amount)
    {
        MaxHealth = amount;
        Health = amount;
    }

    public void ResetMaxHealth()
    {
        MaxHealth = maxHealth;
        Health = MaxHealth;
    }

    public void TakeDamage(float value)
    {
        if (IsInPreDeath || IsDead) return;

        // Check if still in invincibility frames
        if (Time.time < nextDamageTime) return;

        Health -= value;
        Health = Mathf.Max(Health, 0);

        // Set next damage time
        nextDamageTime = Time.time + invincibilityFrameDuration;

        GameEventSystem.OnPlayerHealthChanged(Health, maxHealth);

        if (hitAudioSource.isPlaying) hitAudioSource.Stop();
        SoundManager.Instance?.PlayHitSound(hitAudioSource);

        if (hurtAudioSource.isPlaying) hurtAudioSource.Stop();
        SoundManager.Instance?.PlayHurtSound(hurtAudioSource);

        //Debug.Log("Health: " + currentHealth);

        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
            LeanTween.cancel(gameObject);
            healCoroutine = null;
        }
        healCoroutine = StartCoroutine(HealAfter());

        if (Health < 50f)
            SoundManager.Instance?.PlayLowHPSound(lowHpAudioSource);

        if (Health <= 0)
        {
            if (revive)
            {
                IsInPreDeath = true;
                globalVolume.profile = monochromeVolumeProfile;
                StartCoroutine(PreDeath());
            }
            else
            {
                IsDead = true;
                Die();
            }
        }
    }

    public IEnumerator HealAfter()
    {
        yield return new WaitForSeconds(healDelay);

        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, Health, maxHealth, healTillFull)
            .setOnUpdate((float value) =>
            {
                Health = value;

                GameEventSystem.OnPlayerHealthChanged?.Invoke(Health, maxHealth);

                if (Health >= 50 && lowHpAudioSource.isPlaying)
                {
                    lowHpAudioSource.Stop();
                }
            });
    }

    private IEnumerator PreDeath()
    {
        deathCounter++;

        GameEventSystem.OnPlayerPreDeath?.Invoke(true);

        //Sends the time in PreDeath once
        OnPreDeathCounter?.Invoke(preDeathTimer);

        LeanTween.value(gameObject, Time.timeScale, 0.3f, 2f)
            .setOnUpdate((float value) =>
            {
                Time.timeScale = value;
            })
            .setIgnoreTimeScale(true);

        //If Timer runs out the Player revives
        yield return new WaitForSeconds(preDeathTimer);

        //Stand Up
        Health = MaxHealth;
        GameEventSystem.OnPlayerHealthChanged?.Invoke(Health, maxHealth);
        lowHpAudioSource.Stop();
        globalVolume.profile = globalVolumeProfile;
        revive = false;
        IsInPreDeath = false;
        Time.timeScale = 1f;
        SoundManager.Instance?.PlayPlayerRevivedSound(reviveSoundSource);
    }

    private void Die()
    {
        deathCounter++;

        if (healCoroutine != null && IsDead)
        {
            StopCoroutine(healCoroutine);
            LeanTween.cancel(gameObject);
            healCoroutine = null;
        }

        if (lowHpAudioSource.isPlaying) lowHpAudioSource.Stop();
        gameOverSoundSource.Play();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0.3f;

        GameEventSystem.OnPlayerDeath?.Invoke(deathCounter);
    }

    private void GameEventSystem_OnGamePause(bool state)
    {
        //Pause the heartbeat Sound
        if (lowHpAudioSource.isPlaying && state)
        {
            lowHpAudioSource.Pause();
        }
        else if (Health < 50f && !state)
        {
            lowHpAudioSource.Play();
        }
    }

}
