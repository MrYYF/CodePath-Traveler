
public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    private Animator animator;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundSnapForce = -1f;
    [SerializeField] private float maxVelocity = -20f;
    [SerializeField] private float speed = 5f;
    private float verticalVelocity;
    private Vector2 movementInput;
    private bool isMoving;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update() {
        Movement();
        SetAnimator();
    }

    private void Movement() {
        var input = InputSystemController.Instance;
        if (input == null) return;

        movementInput = input.GetMovementInput();
        if (characterController.isGrounded && verticalVelocity < 0) {
            verticalVelocity = groundSnapForce;
        } else {
            verticalVelocity += gravity * Time.deltaTime;
            verticalVelocity = Mathf.Min(verticalVelocity, maxVelocity);
        }
        Vector3 velocity = new Vector3(movementInput.x, 0, movementInput.y) * speed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void SetAnimator() {
        if (animator == null) return;

        isMoving = movementInput.magnitude > 0.1f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving) {
            animator.SetFloat("moveX", movementInput.x);
            animator.SetFloat("moveY", movementInput.y);
        }
    }
}
