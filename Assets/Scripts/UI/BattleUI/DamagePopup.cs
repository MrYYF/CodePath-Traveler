using System.Text;
using TMPro;

/// <summary>
/// 用于控制战斗数值显示浮动文本控件
/// </summary>
public class DamagePopup : MonoBehaviour {
    [SerializeField] private TextMeshPro valueText;
    [SerializeField] private float initialVelocityY = 0.5f; // 漂浮速度
    [SerializeField] private Vector2 randomHorizontalRange = new Vector2(-2f, 2f); //文字出现的随机区域
    [SerializeField] private float lifeTime; // 显示时间
    [SerializeField] private float fadeOutStartTime; // 淡出时间

    private float _timer;
    private Vector3 _velocity;

    private void Update() {
        _timer += Time.deltaTime;
        if (_timer >= fadeOutStartTime) {
            float alpha = Mathf.Lerp(1f, 0f, (_timer - fadeOutStartTime) / (lifeTime - fadeOutStartTime));
            valueText.alpha = alpha;
        }
        transform.position += _velocity * Time.deltaTime;
        if (_timer >= lifeTime) {
            Destroy(gameObject);
        }
    }

    public void Setup(int amount, DamageType type) {
        valueText.text = ConvertNumberToSpriteString(amount, type);
        valueText.alpha = 0f;
        _timer = 0f;
        _velocity = new Vector3(Random.Range(randomHorizontalRange.x, randomHorizontalRange.y), initialVelocityY, 0f);

    }

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
}
