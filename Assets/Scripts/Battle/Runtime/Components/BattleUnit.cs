
using System;

/// <summary>
/// 战斗单位组件，附加在战斗场景中的每个单位的GameObject上。
/// </summary>
public class BattleUnit : MonoBehaviour {
    public BattleEntity Entity { get; private set; } // 战斗实体数据

    [Header("Base Component")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer targetCursorRenderer;
    [Header("眩晕特效")]
    [SerializeField] private GameObject breakStunFx;
    [SerializeField] private float breakStunYOffset = 0.4f;

    private void Awake() {
        targetCursorRenderer.enabled = false;
        UpdateTargetCursorPosition();
    }

    /// <summary>
    /// 更新焦点光标以及打击点位置
    /// </summary>
    private void UpdateTargetCursorPosition() {
        if (spriteRenderer.sprite == null) {
            return;
        }
        Vector3 worldCenter = spriteRenderer.bounds.center;
        Vector3 localCenter = transform.InverseTransformPoint(worldCenter);
        Vector3 localPosition = targetCursorRenderer.transform.localPosition;
        localPosition.y = localCenter.y;
        localPosition.z = localCenter.z - 0.1f;
        targetCursorRenderer.transform.localPosition = localPosition;
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
            SetBreakStunVisual(false);
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

    public void PlayUseItemAnimation() {
        animator.SetTrigger("use");
    }

    /// <summary>
    /// 获取战斗数值飘动文本显示位置
    /// </summary>
    /// <returns>位置</returns>
    public Vector3 GetPopupAnchorPosition() => targetCursorRenderer.transform.position;

    /// <summary>
    /// 设置眩晕效果是否显示
    /// </summary>
    /// <param name="visible"></param>
    public void SetBreakStunVisual(bool visible) {
        if (visible) {
            UpdateBreakStunPosition();
        }
        breakStunFx.SetActive(visible);
    }

    /// <summary>
    /// 更新破盾眩晕效果的位置
    /// </summary>
    private void UpdateBreakStunPosition() {
        // sprite世界空间最高点坐标
        Vector3 worldTop = spriteRenderer.bounds.max;
        // 本地坐标
        Vector3 localTop = transform.InverseTransformPoint(worldTop);
        Transform breakStunTransform = breakStunFx.transform;
        Vector3 localPos = breakStunTransform.localPosition;
        localPos.y = localTop.y + breakStunYOffset;
        breakStunTransform.localPosition = localPos;
    }
}
