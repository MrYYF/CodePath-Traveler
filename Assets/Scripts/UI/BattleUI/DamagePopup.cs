using DG.Tweening;
using System.Text;
using TMPro;
using UnityEngine.Pool;

/// <summary>
/// 用于控制战斗数值显示浮动文本控件
/// </summary>
public class DamagePopup : MonoBehaviour {
    [Header("Components")]
    private TextMeshPro textMesh;

    [Header("移位参数")]
    [SerializeField] private float initialVelocityY = 10f;
    [SerializeField] private Vector2 randomHorizontalRange = new Vector2(-3f, 3f);
    [SerializeField] private float drag = 5f;
    [SerializeField] private float gravity = 0f;

    [Header("缩放弹跳参数")]
    [SerializeField] private Ease scaleCurve = Ease.OutBack;
    [SerializeField] private float scaleDuration = 0.3f; //缩放持续时间

    [Header("淡出参数")]
    [SerializeField] private float lifeTime = 1f; //淡出持续时间
    [SerializeField] private float fadeOutStartTime = 0.6f; //淡出开始延迟

    private Vector3 _velocity;
    private float _timer;
    private Color _baseColor;
    private Vector3 _baseScale;
    private ObjectPool<DamagePopup> _pool;
    public void SetPool(ObjectPool<DamagePopup> pool) => _pool = pool;

    #region 生命周期
    private void Awake() {
        textMesh = GetComponent<TextMeshPro>();
        _baseColor = textMesh.color;
        _baseScale = transform.localScale;
    }
    private void Update() {
        float dt = Time.deltaTime <= 0 ? Time.unscaledTime : Time.deltaTime;
        _timer += dt;

        UpdateScale();
        UpdateMotion(dt);
        UpdateFade();

        if(_timer > lifeTime) {
            Release();
        }
    }
    #endregion

    public void Setup(int amount, DamageType type) {
        textMesh.text = ConvertNumberToSpriteString(amount, type);
        _timer = 0f;
        textMesh.color = _baseColor;
        textMesh.alpha = 1f;
        transform.localScale = _baseScale * 0f;

        _velocity = new Vector3(Random.Range(randomHorizontalRange.x, randomHorizontalRange.y),
            initialVelocityY, 0f);
    }

    /// <summary>
    /// 将整数值转换为由 TextMeshPro sprite 标签组成的字符串，便于使用图集中的数字精灵显示伤害/治疗数值。
    /// </summary>
    /// <param name="value">要显示的整数值。方法会使用其绝对值（因此不会显示负号）。</param>
    /// <param name="type">数字所属的 `DamageType`，用于计算图集中数字精灵的起始索引。每种类型假定占用连续的 10 个索引（0-9）。</param>
    /// <returns>返回一个可直接赋值给 `TextMeshPro.text` 的字符串，格式示例：<sprite=12><sprite=13>...，每个数字字符被替换为对应索引的 sprite 标签。</returns>
    private string ConvertNumberToSpriteString(int value, DamageType type) {
        string original = Mathf.Abs(value).ToString();
        StringBuilder builder = new StringBuilder();
        int startIndex = (int)type * 10;
        foreach (var c in original) {
            int digit = c - '0';
            builder.Append($"<sprite={startIndex + digit}>");
        }
        return builder.ToString();
    }

    #region 对象池相关方法
    private void Release() {
        _pool.Release(this);
    }
    #endregion

    #region 浮动数值文字动效
    /// <summary>
    /// 浮动数值文字缩放效果
    /// </summary>
    private void UpdateScale() {
        // 结束缩放效果
        if (scaleDuration <= 0 || _timer >= scaleDuration) {
            transform.localScale = _baseScale;
            return;
        }

        float t = Mathf.Clamp01(_timer / scaleDuration);
        float scaleValue = DOVirtual.EasedValue(0f, 1f, t, scaleCurve);
        transform.localScale = _baseScale * scaleValue;
    }

    /// <summary>
    /// 浮动数值文字移动效果
    /// </summary>
    /// <param name="dt">时间流逝速率</param>
    private void UpdateMotion(float dt) {
        transform.position += _velocity * dt;
        _velocity -= _velocity * dt * drag;

        if (gravity != 0) {
            _velocity += new Vector3(0f, gravity, 0f) * dt;
        }
    }

    /// <summary>
    /// 浮动数值文字淡入效果
    /// </summary>
    private void UpdateFade() {
        if(_timer < fadeOutStartTime || lifeTime <= fadeOutStartTime) {
            return;
        }

        float t = Mathf.Clamp01((_timer - fadeOutStartTime) / (lifeTime - fadeOutStartTime));
        textMesh.alpha = Mathf.Lerp(1f, 0f, t);
    }
    #endregion

}
