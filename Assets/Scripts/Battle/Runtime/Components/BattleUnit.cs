

public class BattleUnit : MonoBehaviour {
    [Header("Base Component")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Transform hitPoint;
    [SerializeField] private SpriteRenderer targetCursorRenderer;

    private void Awake() {
        targetCursorRenderer.enabled = false;
        UpdateTargetCursorPosition();
    }

    /// <summary>
    /// 更新焦点光标以及打击点位置
    /// </summary>
    private void UpdateTargetCursorPosition() {
        Vector3 worldCenter = spriteRenderer.bounds.center;
        Vector3 localCenter = transform.InverseTransformPoint(worldCenter);
        Vector3 localPosition = targetCursorRenderer.transform.localPosition;
        localPosition.y = localCenter.y;
        targetCursorRenderer.transform.localPosition = localPosition;
        hitPoint.localPosition = localPosition;
    }

    public void SetTargetSelection(bool visible) {
        if (visible) {
            UpdateTargetCursorPosition();
        }

        targetCursorRenderer.enabled = visible;
    }
}
