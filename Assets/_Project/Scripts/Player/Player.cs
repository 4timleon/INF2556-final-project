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
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private bool cursorVisible = false;

    [Header("Move Parameters")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float gravityMultiplier = 1f;

    [Header("Look Parameters")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float upDownLookRange = 80f;

    public Transform WeaponSocket => weaponSocket;

    private Vector3 currentMovement;
    private float verticalRotation;
    private float currentSpeed => walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultiplier : 1f);

    //private Weapon weapon;

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
        HandleLook();
        HandleFall();
    }

    private void HandleMovement() {
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

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

    private void HandleLook() {
        float mouseXRotation = playerInputHandler.LookInput.x * mouseSensitivity;
        float mouseYRotation = playerInputHandler.LookInput.y * mouseSensitivity;

        //Horizontal Camera Rotation
        transform.Rotate(0, mouseXRotation, 0);

        //Vertical Camera Rotation
        verticalRotation = Mathf.Clamp(verticalRotation - mouseYRotation, -upDownLookRange, upDownLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

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

    public PlayerInputHandler GetPlayerInputHandler() {
        return playerInputHandler;
    }
}
