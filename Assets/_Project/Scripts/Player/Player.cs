using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {

    //private PlayerRegistry registry;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Transform aimtarget;
    [SerializeField] private Transform weaponSocket;
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
    [SerializeField] private float health = 100;
    [SerializeField] private float preDeathTimer = 60f;

    public float RotationMismatch { get; private set; } = 0f;
    public bool IsRotatingToTarget { get; private set; } = false;

    //References
    private PlayerAnimation playerAnimation;

    public Transform WeaponSocket => weaponSocket;

    private Vector3 currentMovement;
    private float horizontalRotation;
    private float verticalRotation;
    private float currentSpeed => walkSpeed * (GetIsSprinting() ? sprintMultiplier : 1f);

    //private Weapon weapon;

    private bool isInPreDeath = false;
    private bool isDead = false;

    //Camera Variables
    private Vector2 cameraRotation = Vector2.zero;
    private Vector2 playerTargetRotation = Vector2.zero;

    //Rotation Variables
    private bool isRotatingClockwise = false;
    private float rotatingToTargetTimer = 0f;

    private void Awake() {
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void Start() {
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /* public override void OnNetworkSpawn() {
        if (IsOwner) {
            if (mainCamera != null) mainCamera.enabled = true;
            if (audioListener != null) audioListener.enabled = true;

            Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = cursorVisible;
        } else {
            if (mainCamera != null) mainCamera.gameObject.SetActive(false);
            if (audioListener != null) audioListener.enabled = false;
        }
    } */

    private void Update() {
        //if (!IsOwner) return;

        HandleMovement();
        HandleFall();
    }

    private void LateUpdate() {
        HandleLook();
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

        bool isIdling = playerInputHandler.MovementInput == Vector2.zero;

        //Rotate if not idle
        if (!isIdling) {
            RotatePlayerToTarget();
        }

        //If rotation mismatch not within threshold or rotate to Target Timer is true then rotate
        if (Mathf.Abs(RotationMismatch) > rotationThreshold || rotatingToTargetTimer > 0) {
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

    //[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    public void RequestWeaponServerRpc(int weaponIndex) {
        //WeaponManager.Instance.SpawnWeaponForPlayer(weaponIndex, this);
    }

    /* public Weapon GetCurrentWeapon() {
        return weapon;
    }

    public void SetCurrentWeapon(Weapon weapon) {
        this.weapon = weapon;
    } */

    public void TakeDamage(float value) {
        if (isInPreDeath) return;

        health -= value;
        health = Mathf.Max(health, 0);

        Debug.Log("Health: " + health);

        if (health <= 0) {
            isInPreDeath = true;
            PreDeath();
        }
    }

    private void PreDeath() {
        if (isDead) return;

        //If Timer runs out the Player dies
        if (Time.time <= preDeathTimer) {
            isDead = true;
            Die();
        }
    }

    private void Die() {
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

    public PlayerInputHandler GetPlayerInputHandler() {
        return playerInputHandler;
    }
}
