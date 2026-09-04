using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour {

    private const string MOUSE_SENS_KEY = "MouseSensitivity";

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Transform aimtarget;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private VolumeProfile globalVolumeProfile;
    [SerializeField] private VolumeProfile monochromeVolumeProfile;
    [SerializeField] private bool cursorVisible = false;

    [Header("Move Parameters")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float gravityMultiplier = 1f;

    [Header("Look Parameters")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float upDownLookRange = 80f;

    [Header("Rotation")]
    [SerializeField] private float playerModelRotationSpeed = 10f;
    [SerializeField] private float rotateToTargetTime = 0.25f;
    [SerializeField] private float rotationThreshold = 30;

    [Header("Health")]
    [SerializeField] private float currentHealth = 100;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float preDeathTimer = 10f;
    [SerializeField] private bool revive = true; //Test: Set to false
    [SerializeField] private float healDelay = 5f;
    [SerializeField] private float healTillFull = 3f;
    [SerializeField] private float invincibilityFrameDuration = 1f;

    [Header("Sound")]
    [SerializeField] private AudioSource lowHpAudioSource;
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioSource gameOverSoundSource;

    public bool IsIdling { get; private set; } = false;
    public float RotationMismatch { get; private set; } = 0f;
    public bool IsRotatingToTarget { get; private set; } = false;

    private Vector3 currentMovement;
    private float currentSpeed => walkSpeed * (GetIsSprinting() ? sprintMultiplier : 1f);

    public bool IsInPreDeath { get; private set; } = false;
    public bool IsDead { get; private set; } = false;

    //Camera Variables
    private Vector2 cameraRotation = Vector2.zero;
    private Vector2 playerTargetRotation = Vector2.zero;

    //Rotation Variables
    private bool isRotatingClockwise = false;
    private float rotatingToTargetTimer = 0f;

    //Pause
    private bool isPaused = false;

    //Health
    private Coroutine healCoroutine;
    private float nextDamageTime = 0f;
    private float deathCounter;

    //Event
    public static event Action<float> OnPreDeathCounter;

    private void Start() {
        mouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENS_KEY, 0.1f);

        deathCounter = 0f;

        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void OnEnable() {
        PauseMenuController.OnGamePaused += PauseMenuController_OnGamePaused;
        ControlsSettings.OnMouseSensitivityChanged += SetMouseSensitivity;
    }

    private void OnDisable() {
        PauseMenuController.OnGamePaused -= PauseMenuController_OnGamePaused;
        ControlsSettings.OnMouseSensitivityChanged -= SetMouseSensitivity;
    }

    private void Update() {
        if (!IsDead) {
            HandleMovement();
        }

        HandleFall();
    }

    private void LateUpdate() {
        if (!isPaused && !IsDead) {
            HandleLook();
        }
    }

    private void HandleMovement() {
        (Vector3 cameraRight, Vector3 cameraForward) = GetCameraDirection();

        Vector3 inputDirection = cameraRight * playerInputHandler.MovementInput.x +
                                    cameraForward * playerInputHandler.MovementInput.y;

        currentMovement.x = inputDirection.normalized.x * currentSpeed;
        currentMovement.z = inputDirection.normalized.z * currentSpeed;

        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void HandleFall() {
        if (characterController.isGrounded) {
            currentMovement.y = -0.5f;
        } else {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    #region Rotation
    private void HandleLook() {
        cameraRotation.x += playerInputHandler.LookInput.x * mouseSensitivity;
        cameraRotation.y = Mathf.Clamp(cameraRotation.y - playerInputHandler.LookInput.y * mouseSensitivity, -upDownLookRange, upDownLookRange);

        playerTargetRotation.x += transform.eulerAngles.x + mouseSensitivity * playerInputHandler.LookInput.x;

        IsIdling = playerInputHandler.MovementInput == Vector2.zero;

        //Rotate if not idle
        if (!IsIdling) {
            RotatePlayerToTarget();
        } else if (Mathf.Abs(RotationMismatch) > rotationThreshold || rotatingToTargetTimer > 0) {//If rotation mismatch not within threshold or rotate to Target Timer is true then rotate
            UpdateIdleRotation(rotationThreshold);
        }

        mainCamera.transform.rotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0f);

        //Get angle between camera and player
        Vector3 cameraForwardProjectedXZ = new Vector3(mainCamera.transform.forward.x, 0f, mainCamera.transform.forward.z).normalized;
        Vector3 crossProd = Vector3.Cross(transform.forward, cameraForwardProjectedXZ);
        float sign = Mathf.Sign(Vector3.Dot(crossProd, transform.up));
        RotationMismatch = sign * Vector3.Angle(transform.forward, cameraForwardProjectedXZ);
    }

    private void UpdateIdleRotation(float rotationThreshold) {
        //Start new rotation direction
        if (Mathf.Abs(RotationMismatch) > rotationThreshold) {
            rotatingToTargetTimer = rotateToTargetTime;
            isRotatingClockwise = RotationMismatch > rotationThreshold;
        }
        rotatingToTargetTimer -= Time.deltaTime;

        if (isRotatingClockwise && RotationMismatch > 0f ||
            !isRotatingClockwise && RotationMismatch < 0f) {
            RotatePlayerToTarget();
        }
    }

    private void RotatePlayerToTarget() {
        Quaternion targetRotationX = Quaternion.Euler(0f, playerTargetRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
    }
    #endregion

    public void TakeDamage(float value) {
        if (IsInPreDeath || IsDead) return;

        // Check if still in invincibility frames
        if (Time.time < nextDamageTime) return;

        currentHealth -= value;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Set next damage time
        nextDamageTime = Time.time + invincibilityFrameDuration;

        GameEventSystem.OnPlayerHealthChanged(currentHealth, maxHealth);

        if (hitAudioSource.isPlaying) hitAudioSource.Stop();
        SoundManager.Instance?.PlayHitSound(hitAudioSource);

        //Debug.Log("Health: " + currentHealth);

        if (healCoroutine != null) {
            StopCoroutine(healCoroutine);
            healCoroutine = null;
        }
        healCoroutine = StartCoroutine(HealAfter());

        if (currentHealth < 50f)
            SoundManager.Instance?.PlayLowHPSound(lowHpAudioSource);

        if (currentHealth <= 0) {
            if (revive) {
                IsInPreDeath = true;
                globalVolume.profile = monochromeVolumeProfile;
                StartCoroutine(PreDeath());
            } else {
                IsDead = true;
                Die();
            }
        }
    }

    public IEnumerator HealAfter() {
        yield return new WaitForSecondsRealtime(healDelay);

        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, currentHealth, maxHealth, healTillFull)
            .setOnUpdate((float value) => {
                currentHealth = value;

                GameEventSystem.OnPlayerHealthChanged(currentHealth, maxHealth);

                if (currentHealth >= 50 && lowHpAudioSource.isPlaying) {
                    lowHpAudioSource.Stop();
                }
            })
            .setIgnoreTimeScale(true);
    }

    private IEnumerator PreDeath() {
        deathCounter++;

        GameEventSystem.OnPlayerPreDeath?.Invoke(true);

        //Sends the time in PreDeath once
        OnPreDeathCounter?.Invoke(preDeathTimer);

        LeanTween.value(gameObject, Time.timeScale, 0.3f, 1f)
            .setOnUpdate((float value) => {
                Time.timeScale = value;
            })
            .setIgnoreTimeScale(true);

        //If Timer runs out the Player revives
        yield return new WaitForSecondsRealtime(preDeathTimer);

        //Stand Up
        globalVolume.profile = globalVolumeProfile;
        revive = false;
        IsInPreDeath = false;
        Time.timeScale = 1f;

        //Remove Revive Perk
    }

    private void Die() {
        deathCounter++;

        if (healCoroutine != null && IsDead) {
            StopCoroutine(healCoroutine);
            healCoroutine = null;
        }

        if (lowHpAudioSource.isPlaying) lowHpAudioSource.Stop();
        gameOverSoundSource.Play();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0.3f;

        GameEventSystem.OnPlayerDeath?.Invoke(deathCounter);
    }

    public (Vector3, Vector3) GetCameraDirection() {
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        return (cameraRight, cameraForward);
    }

    public bool GetIsSprinting() {
        if (!playerInputHandler.SprintTriggered) {
            return false;
        }

        (Vector3 cameraRight, Vector3 cameraForward) = GetCameraDirection();
        Vector3 inputDirection = cameraRight * playerInputHandler.MovementInput.x +
                                 cameraForward * playerInputHandler.MovementInput.y;

        if (inputDirection.sqrMagnitude <= 0.0001f) {
            return false;
        }

        return Vector3.Dot(inputDirection.normalized, cameraForward) > 0f;
    }

    private void PauseMenuController_OnGamePaused(bool state) {
        isPaused = state;

        //Pause the heartbeat Sound
        if (lowHpAudioSource.isPlaying && state) {
            lowHpAudioSource.Pause();
        } else if (currentHealth < 50f && !state) {
            lowHpAudioSource.Play();
        }
    }

    public void SetMouseSensitivity(float value) {
        mouseSensitivity = value;
    }

    public PlayerInputHandler GetPlayerInputHandler() {
        return playerInputHandler;
    }
}
