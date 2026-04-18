


public class FieldFollower : MonoBehaviour {
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;

    [Header("Animator Params")]
    [SerializeField] private string isMovingParam = "isMoving";
    [SerializeField] private string moveXParam = "moveX";
    [SerializeField] private string moveYParam = "moveY";

    [Header("最小位移阈值")]
    [SerializeField] private float movementThreshold = 0.001f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundSnapForce = -1f;
    [SerializeField] private float maxVelocity = -20f;

    private float verticalVelocity;



    #region 对外接口
    public void SetupFollower(CharacterDefinitionSO definition) {
        animator.runtimeAnimatorController = definition.FieldAnimator;
    }

    public void MoveTo(Vector3 targetPosition, float speed) {
        // 获得与目标的差值
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0; // 只考虑水平面上的移动
        // 计算水平移动的步长
        Vector3 horizontalStep = Vector3.ClampMagnitude(toTarget, Mathf.Max(0f, speed) * Time.deltaTime);


        // 计算垂直方向上的速度
        if (characterController.isGrounded && verticalVelocity < 0) {
            verticalVelocity = groundSnapForce;
        }
        else {
            verticalVelocity += gravity * Time.deltaTime;
            verticalVelocity = Mathf.Min(verticalVelocity, maxVelocity);
        }

        Vector3 movement = horizontalStep;
        movement.y = verticalVelocity * Time.deltaTime;

        characterController.Move(movement);
        UpdateAnimation(horizontalStep);
    }
    #endregion

    private void UpdateAnimation(Vector3 step) {
        bool isMoving = step.magnitude > movementThreshold * movementThreshold;

        animator.SetBool(isMovingParam, isMoving);

        if (isMoving) {
            animator.SetFloat(moveXParam, step.x);
            animator.SetFloat(moveYParam, step.y);
        }
    }
}
