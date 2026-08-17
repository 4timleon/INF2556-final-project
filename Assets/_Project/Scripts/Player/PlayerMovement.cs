using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    private CharacterController characterController;

    [Header("Movement:")]
    public float maxWalkSpeed = 10f;
    public float maxSprintSpeed = 20f;

    public float groundAcceleration = 100f;
    public float groundFriction = 50f;
    public float airAcceleration = 20f;

    public float leanMultiplier = 1f;

    [Header("Jumping:")]
    public float jumpHeight = 4f;
    public float gravity = 20f;

    [Header("Camera:")]
    public Transform cameraRigTransform;
    public Transform cameraTransform;
    public float sensitivity = 1f;

    [Header("Arms:")]
    public Transform armsTransform;
    public float armPitchSwayAmount = 0.5f;
    public float armPitchReturnTime = 0.2f;
    public float maxArmPitchSway = 5f;
    public float armYawSwayAmount = 0.5f;
    public float armYawReturnTime = 0.2f;
    public float maxArmYawSway = 10f;

    private float currentArmPitch = 0.0f;
    private float armPitchVelocity = 0.0f;
    private float currentArmYaw = 0.0f;
    private float armYawVelocity = 0.0f;

    [Header("Animation:")]
    public Animator animator;
    public string movementBlend;

    private int isInStanceHash;
    private int isSprintingHash;
    private int isWalkingHash;

    private Vector3 velocity = Vector3.zero;
    private bool isSprinting;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private void Start()
    {
        inputActions = GameControls.GetInputActions();

        characterController = GetComponent<CharacterController>();

        inputActions.Player.Sprint.started += OnSprintStarted;
        inputActions.Player.Sprint.canceled += OnSprintCanceled;

        inputActions.Player.Jump.started += OnJump;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isInStanceHash = Animator.StringToHash("isInStance");
        isSprintingHash = Animator.StringToHash("isSprinting");
        isWalkingHash = Animator.StringToHash("isWalking");
    }

    private void OnDisable()
    {
        inputActions.Player.Sprint.started -= OnSprintStarted;
        inputActions.Player.Sprint.canceled -= OnSprintCanceled;
        inputActions.Player.Jump.started -= OnJump;
    }

    private void OnSprintStarted(UnityEngine.InputSystem.InputAction.CallbackContext context) => isSprinting = true;

    private void OnSprintCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context) => isSprinting = false;

    private void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
        }
    }

    private void Update()
    {
        BodyMovement();
        LeanMovement();
        CameraMovement();
        AnimationMovement();
    }

    private void BodyMovement()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 targetDirection = new Vector3(input.x, 0, input.y);

        targetDirection = transform.TransformDirection(targetDirection).normalized;

        float currentMaxSpeed = GetSpeed();

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        if (characterController.isGrounded)
        {
            if (input.magnitude > 0.1f)
            {
                horizontalVelocity = Vector3.MoveTowards(
                    horizontalVelocity,
                    targetDirection * currentMaxSpeed,
                    groundAcceleration * Time.deltaTime);
            }
            else
            {
                horizontalVelocity = Vector3.MoveTowards(
                    horizontalVelocity,
                    Vector3.zero,
                    groundFriction * Time.deltaTime);
            }

            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }
        else
        {
            horizontalVelocity += targetDirection * airAcceleration * Time.deltaTime;
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, currentMaxSpeed);
        }

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        if (!characterController.isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    private void AnimationMovement()
    {
        Vector3 planarVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);

        float currentRealSpeed = planarVelocity.magnitude;

        float animationSpeed = currentRealSpeed / maxSprintSpeed;

        animator.SetFloat(movementBlend, animationSpeed, 0.1f, Time.deltaTime);
    }

    private void LeanMovement()
    {
        if (inputActions.Player.Move.ReadValue<Vector2>().x == 0)
        {
            if (Mathf.Round(cameraRigTransform.localRotation.eulerAngles.z) == 0)
            {
                return;
            }
        }

        float input = inputActions.Player.Move.ReadValue<Vector2>().x;
        float lean = input * (leanMultiplier * (-1f));

        LeanTween.cancelAll(cameraRigTransform.gameObject);
        LeanTween.rotateLocal(cameraRigTransform.gameObject, new Vector3(0, 0, lean), .15f);
    }

    private void CameraMovement()
    {
        float mouseX = inputActions.Player.Look.ReadValue<Vector2>().x * (sensitivity / 10);
        float mouseY = inputActions.Player.Look.ReadValue<Vector2>().y * (sensitivity / 10);

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.localRotation = Quaternion.AngleAxis(yaw, Vector3.up);
        cameraTransform.localRotation = Quaternion.AngleAxis(pitch, Vector3.right);

        currentArmPitch -= mouseY * armPitchSwayAmount;
        currentArmYaw -= mouseX * armYawSwayAmount;

        currentArmPitch = Mathf.Clamp(currentArmPitch, -maxArmPitchSway, maxArmPitchSway);
        currentArmYaw = Mathf.Clamp(currentArmYaw, -maxArmYawSway, maxArmYawSway);

        currentArmPitch = Mathf.SmoothDampAngle(currentArmPitch, 0f, ref armPitchVelocity, armPitchReturnTime);
        currentArmYaw = Mathf.SmoothDampAngle(currentArmYaw, 0f, ref armYawVelocity, armYawReturnTime);

        float finalArmPitch = pitch + currentArmPitch;

        Quaternion yawRotation = Quaternion.AngleAxis(currentArmYaw, Vector3.up);
        Quaternion pitchRotation = Quaternion.AngleAxis(finalArmPitch, Vector3.right);

        armsTransform.localRotation = yawRotation * pitchRotation;
    }

    private float GetSpeed() => isSprinting ? maxSprintSpeed : maxWalkSpeed;
    public bool IsWalking() => inputActions.Player.Move.ReadValue<Vector2>() != Vector2.zero;
    public bool IsSprinting() => isSprinting;
    public bool IsStanding() => !IsWalking() && !IsSprinting();
}