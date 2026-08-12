using UnityEngine;

public class PlayerAnimation : MonoBehaviour {

    [SerializeField] private Animator animator;
    [SerializeField] private float locomotionBlendSpeed = 10f;

    private int inputXHash = Animator.StringToHash("inputX");
    private int inputYHash = Animator.StringToHash("inputY");
    private int sprintMagnitudeHash = Animator.StringToHash("sprintMagnitude");
    private int rotationMismatchHash = Animator.StringToHash("rotationMismatch");

    private Player player;
    private PlayerInputHandler playerInputHandler;

    private Vector3 currentBlendInput = Vector3.zero;

    private void Start() {
        player = GetComponent<Player>();
        playerInputHandler = player.GetPlayerInputHandler();
    }

    private void Update() {
        UpdateMovementState();
    }

    private void UpdateMovementState() {
        // Get raw input and apply sprint multiplier
        float inputX = playerInputHandler.MovementInput.x;
        float inputY = playerInputHandler.MovementInput.y;

        if (player.GetIsSprinting()) {
            inputX *= 1.5f;
            inputY *= 1.5f;
        }

        // Get camera's forward and right directions
        (Vector3 cameraRight, Vector3 cameraForward) = player.GetCameraDirection();

        // Build world direction relative to camera
        Vector3 worldDir = cameraRight * inputX + cameraForward * inputY;

        // Transform into player's local space
        float localInputX = Vector3.Dot(worldDir, transform.right);
        float localInputY = Vector3.Dot(worldDir, transform.forward);

        currentBlendInput = Vector3.Lerp(currentBlendInput, new Vector2(localInputX, localInputY), locomotionBlendSpeed * Time.deltaTime);

        animator.SetFloat(inputXHash, currentBlendInput.x);
        animator.SetFloat(inputYHash, currentBlendInput.y);
        animator.SetFloat(sprintMagnitudeHash, currentBlendInput.magnitude);

        if (player.IsIdling) {
            animator.SetFloat(rotationMismatchHash, player.RotationMismatch);
        } else {
            animator.SetFloat(rotationMismatchHash, 0f);
        }
    }

}
