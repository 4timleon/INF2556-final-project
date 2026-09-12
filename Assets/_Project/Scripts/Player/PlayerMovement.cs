using System;
using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private const string MOUSE_SENS_KEY = "MouseSensitivity";

    [Title("Inputs:", 14)]
    [SerializeField] private PlayerInputs playerInputs;
    [Space]
    [Title("Movement:", 14)]
    [SerializeField] private float maxWalkSpeed = 10f;
    [SerializeField] private float maxSprintSpeed = 20f;
    [SerializeField] private float maxAimWalkSpeed = 5f;
    [Space]
    [SerializeField] private float groundAcceleration = 100f;
    [SerializeField] private float groundFriction = 50f;
    [SerializeField] private float airAcceleration = 20f;
    [Space]
    [SerializeField] private float leanMultiplier = 1f;
    [Space]
    [Title("Jumping:", 14)]
    [SerializeField] private float jumpHeight = 4f;
    [SerializeField] private float gravity = 20f;
    [Space]
    [Title("Camera:", 14)]
    [SerializeField] private Transform cameraRigTransform;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float sensitivity = 1f;
    [Space]
    [Title("Arms:", 14)]
    [SerializeField] private Transform armsTransform;
    [SerializeField] private float armPitchSwayAmount = 0.5f;
    [SerializeField] private float armPitchReturnTime = 0.2f;
    [SerializeField] private float maxArmPitchSway = 5f;
    [Space]
    [SerializeField] private float armYawSwayAmount = 0.5f;
    [SerializeField] private float armYawReturnTime = 0.2f;
    [SerializeField] private float maxArmYawSway = 10f;
    [Space]
    [Title("Animation:", 14)]
    [SerializeField] private Animator animator;
    [SerializeField] private string movementBlend;
    [Space]
    [Title("Recoil:", 14)]
    [SerializeField] private float recoilReturnSpeed = 15f;

    private CharacterController characterController;
    private WeaponController weaponController;
    private Player player;
    private Vector3 planarVelocity;

    private float currentArmPitch = 0.0f;
    private float armPitchVelocity = 0.0f;
    private float currentArmYaw = 0.0f;
    private float armYawVelocity = 0.0f;

    private int isInStanceHash;
    private int isSprintingHash;
    private int isWalkingHash;

    private Vector3 velocity = Vector3.zero;
    private bool isSprinting;

    private bool isAttacking;
    private bool isSwitching;
    private bool isAiming;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private float recoilPitch = 0f;
    private float recoilYaw = 0f;

    //Pause
    private bool isPaused = false;

    private void Start()
    {
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        sensitivity = PlayerPrefs.GetFloat(MOUSE_SENS_KEY, 0.1f);

        isInStanceHash = Animator.StringToHash("isInStance");
        isSprintingHash = Animator.StringToHash("isSprinting");
        isWalkingHash = Animator.StringToHash("isWalking");
    }

    private void OnEnable()
    {
        ControlsSettings.OnMouseSensitivityChanged += SetMouseSensitivity;
        GameEventSystem.OnGamePause += GameEventSystem_OnGamePause;

        GameEventSystem.OnPlayerAttackStarted += Event_AttackStarted;
        GameEventSystem.OnPlayerAttackEnded += Event_AttackEnded;

        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster += Event_WeaponHolster;

        GameEventSystem.OnPlayerSprintStarted += Event_SprintStarted;
        GameEventSystem.OnPlayerSprintEnded += Event_SprintCanceled;

        GameEventSystem.OnPlayerJump += Event_Jump;

        GameEventSystem.OnPlayerWeaponShoot += Event_WeaponRecoil;
        GameEventSystem.OnPlayerInSight += Event_PlayerInSight;
    }

    private void OnDisable()
    {
        ControlsSettings.OnMouseSensitivityChanged -= SetMouseSensitivity;
        GameEventSystem.OnGamePause -= GameEventSystem_OnGamePause;

        GameEventSystem.OnPlayerAttackStarted -= Event_AttackStarted;
        GameEventSystem.OnPlayerAttackEnded -= Event_AttackEnded;

        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
        GameEventSystem.OnPlayerWeaponHolster -= Event_WeaponHolster;

        GameEventSystem.OnPlayerSprintStarted -= Event_SprintStarted;
        GameEventSystem.OnPlayerSprintEnded -= Event_SprintCanceled;

        GameEventSystem.OnPlayerJump -= Event_Jump;

        GameEventSystem.OnPlayerWeaponShoot -= Event_WeaponRecoil;
        GameEventSystem.OnPlayerInSight -= Event_PlayerInSight;
    }

    private void Event_AttackStarted() => isAttacking = true;
    private void Event_AttackEnded() => isAttacking = false;
    private void Event_WeaponHolster() => isSwitching = true;
    private void Event_SprintStarted() => isSprinting = CanSprint();
    private void Event_SprintCanceled() => isSprinting = false;
    private void Event_PlayerInSight(bool obj) => isAiming = obj;

    private void Event_WeaponDraw(WeaponController weaponController)
    {
        this.weaponController = weaponController;
        isSwitching = false;
    }

    private void Event_Jump()
    {
        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
        }
    }

    private void Event_WeaponRecoil(float pitchKick, float yawKick)
    {
        recoilPitch += pitchKick;
        recoilYaw += yawKick;
    }

    private void GameEventSystem_OnGamePause(bool state)
    {
        isPaused = state;
    }

    private void Update()
    {
        if (isPaused || player.IsDead) return;

        BodyMovement();
        LeanMovement();
        AnimationMovement();
    }

    private void LateUpdate()
    {
        if (isPaused || player.IsDead) return;

        CameraMovement();
    }

    private void BodyMovement()
    {
        Vector2 input = playerInputs.MoveInput;
        Vector3 targetDirection = new Vector3(input.x, 0, input.y);

        targetDirection = transform.TransformDirection(targetDirection).normalized;

        if (!CanSprint() && isSprinting) isSprinting = false;

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
        planarVelocity.x = characterController.velocity.x;
        planarVelocity.z = characterController.velocity.z;

        float currentRealSpeed = planarVelocity.magnitude;
        float animationSpeed = currentRealSpeed / maxSprintSpeed;

        if (!isSprinting)
        {
            animationSpeed = Mathf.Min(animationSpeed, 0.5f);
        }

        animator.SetFloat(movementBlend, animationSpeed, 0.1f, Time.deltaTime);
    }

    private void LeanMovement()
    {
        if (playerInputs.MoveInput.x == 0)
        {
            if (Mathf.Round(cameraRigTransform.localRotation.eulerAngles.z) == 0)
            {
                return;
            }
        }

        float input = playerInputs.MoveInput.x;
        float lean = input * (leanMultiplier * (-1f));

        LeanTween.cancel(cameraRigTransform.gameObject);
        LeanTween.rotateLocal(cameraRigTransform.gameObject, new Vector3(0, 0, lean), .15f);
    }

    private void CameraMovement()
    {
        float mouseX = playerInputs.LookInput.x * (sensitivity / 10);
        float mouseY = playerInputs.LookInput.y * (sensitivity / 10);

        yaw += mouseX;
        pitch -= mouseY;

        yaw += recoilYaw;
        pitch -= recoilPitch;

        recoilPitch = Mathf.Lerp(recoilPitch, 0f, recoilReturnSpeed * Time.deltaTime);
        recoilYaw = Mathf.Lerp(recoilYaw, 0f, recoilReturnSpeed * Time.deltaTime);

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

    private bool CanSprint() => !weaponController.IsReloading && !isSwitching && !isAttacking;
    private float GetSpeed()
    {
        if (isSprinting)
        {
            return maxSprintSpeed;
        }
        else
        {
            return isAiming ? maxAimWalkSpeed : maxWalkSpeed;
        }
    }

    public void SetMouseSensitivity(float value)
    {
        sensitivity = value;
    }

}