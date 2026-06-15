
using System;

/// <summary>
/// 战斗单位组件，附加在战斗场景中的每个单位的GameObject上。
/// </summary>
public class BattleUnit : MonoBehaviour {
    public BattleEntity Entity { get; private set; } // 战斗实体数据

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
        Bounds bounds = spriteRenderer.bounds;
        Vector3 worldLeftCenter = new Vector3(bounds.min.x, bounds.center.y, bounds.center.z);
        Vector3 localLeftCenter = transform.InverseTransformPoint(worldLeftCenter);
        Transform cursorTransform = targetCursorRenderer.transform;
        Vector3 localPos = cursorTransform.localPosition;
        localPos.x = localLeftCenter.x;
        localPos.y = localLeftCenter.y;
        localPos.z = localLeftCenter.z - 0.1f;
        cursorTransform.localPosition = localPos;
    }

    /// <summary>
    /// 单位选择光标是否显示
    /// </summary>
    /// <param name="visible">是否显示</param>
    public void SetTargetSelection(bool visible) {
        if (visible) {
            UpdateTargetCursorPosition();
        }

        targetCursorRenderer.enabled = visible;
    }

    /// <summary>
    /// 绑定战斗实体数据到该组件，设置动画等相关属性。
    /// </summary>
    /// <param name="battleEntity"></param>
    public void Bind(BattleEntity battleEntity) {
        Entity = battleEntity;

        // 绑定动画
        if (battleEntity.Definition.BattleAnimator != null) {
            animator.runtimeAnimatorController = battleEntity.Definition.BattleAnimator;
        }

        UpdateVisuals();
    }

    /// <summary>
    /// 根据战斗实体的当前状态更新动画参数和视觉效果
    /// </summary>
    public void UpdateVisuals() {
        if (!Entity.IsAlive) {
            // 死亡动画
            animator.SetBool("isDead", true);
            return;
        }

        animator.SetBool("isDead", false);
        float maxHP = Entity.TotalStats.MaxHP;
        float hpRatio = Mathf.Clamp01(Entity.CurrentHP / maxHP);

        animator.SetFloat("hp01", hpRatio);
    }

    /// <summary>
    /// 移动到指定坐标位置
    /// </summary>
    /// <param name="targetPos">目标位置</param>
    /// <param name="duration">移动时间</param>
    /// <returns></returns>
    public IEnumerator MoveToPosition(Vector3 targetPos, float duration = 0.5f) {
        animator.SetBool("isMoving", true);

        Vector3 startPos = transform.position;
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        transform.position = targetPos;

        animator.SetBool("isMoving", false);

    }

    public void PlayAttackAnimation() {
        animator.SetTrigger("attack");
    }
}
