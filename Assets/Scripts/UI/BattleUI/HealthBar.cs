using TMPro;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Slider spSlider;
    [SerializeField] private TMP_Text spText;
    [SerializeField] private Slider bpSlider;

    [Header("Focuse")]
    [SerializeField] private RectTransform highlightRoot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite activeBackground;
    [SerializeField] private float activeScale = 1.2f;

    private BattleEntity _targetEntity;
    private Sprite _normalBackGround;
    private Vector3 _baseScale = Vector3.one;


    public void Setup(BattleEntity entity) {
        _targetEntity = entity;
        characterName.text = entity.Definition.Name;

        StatBlock stats = entity.RuntimeData.GetTotalStats();

        hpSlider.maxValue = stats.MaxHP;
        spSlider.maxValue = stats.MaxSP;
        bpSlider.maxValue = 5;

        // 刷新当前值
        RefreshUI();
    }

    private void RefreshUI() {
        CharacterRuntimeData runtimeData = _targetEntity.RuntimeData;
        StatBlock stats = runtimeData.GetTotalStats();
        hpSlider.value = runtimeData.CurrentHP;
        hpText.text = $"{runtimeData.CurrentHP}/{stats.MaxHP}";

        spSlider.value = runtimeData.CurrentSP;
        spText.text = $"{runtimeData.CurrentSP}/{stats.MaxSP}";

        bpSlider.value = runtimeData.CurrentBP;
    }
}
