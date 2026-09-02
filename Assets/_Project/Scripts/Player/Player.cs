using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour {

    //private PlayerRegistry registry;

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

    public bool IsIdling { get; private set; } = false;
    public float RotationMismatch { get; private set; } = 0f;
    public bool IsRotatingToTarget { get; private set; } = false;

    private Vector3 currentMovement;
    private float currentSpeed => walkSpeed * (GetIsSprinting() ? sprintMultiplier : 1f);

    //private Weapon weapon;

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

    public static event Action<float> OnPreDeathCounter;

    private void Start() {
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void OnEnable() {
        PauseMenuController.OnGamePaused += PauseMenuController_OnGamePaused;
    }

    private void OnDisable() {
        PauseMenuController.OnGamePaused -= PauseMenuController_OnGamePaused;
    }

    private void Update() {
        HandleMovement();
        HandleFall();
    }

    private void LateUpdate() {
        if (!isPaused) {
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

        if (hitAudioSource.isPlaying) {
            hitAudioSource.Stop();
        }
        SoundManager.Instance?.PlayHitSound(hitAudioSource);

        //Debug.Log("Health: " + currentHealth);

        if (healCoroutine != null) {
            StopCoroutine(healCoroutine);
            healCoroutine = null;
        }
        healCoroutine = StartCoroutine(HealAfter());

        if (currentHealth < 50f) {
            SoundManager.Instance?.PlayLowHPSound(lowHpAudioSource);
        }

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
        yield return new WaitForSeconds(healDelay);

        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, currentHealth, maxHealth, healTillFull)
            .setOnUpdate((float value) => {
                currentHealth = value;

                GameEventSystem.OnPlayerHealthChanged(currentHealth, maxHealth);

                if (currentHealth >= 50 && lowHpAudioSource.isPlaying) {
                    lowHpAudioSource.Stop();
                }
            });
    }

    private IEnumerator PreDeath() {
        GameEventSystem.OnPlayerPreDeath?.Invoke(true);

        //Sends the time in PreDeath once
        OnPreDeathCounter?.Invoke(preDeathTimer);

        //If Timer runs out the Player revives
        yield return new WaitForSeconds(preDeathTimer);

        //Stand Up
        globalVolume.profile = globalVolumeProfile;
        revive = false;
        IsInPreDeath = false;

        //Remove Revive Perk
    }

    private void Die() {
        if (healCoroutine != null && IsDead) {
            StopCoroutine(healCoroutine);
            healCoroutine = null;
        }

        GameEventSystem.OnPlayerDeath?.Invoke();
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

        if (lowHpAudioSource.isPlaying && state) {
            lowHpAudioSource.Pause();
        } else if (currentHealth < 50f && !state) {
            lowHpAudioSource.Play();
        }
    }

    public PlayerInputHandler GetPlayerInputHandler() {
        return playerInputHandler;
    }
}
