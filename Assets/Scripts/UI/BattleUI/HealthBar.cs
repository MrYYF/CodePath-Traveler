using Framework.Event;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 状态栏UI层，负责控制状态栏中展示数据
/// </summary>
public class HealthBar : MonoBehaviour,
    IEventReceiver<ActiveEntityChangedEvent>,
    IEventReceiver<EntityStatChangedEvent> {
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

    // 当前healthbarUI组件所绑定的实体
    private BattleEntity _targetEntity;
    private Sprite _normalBackGround;
    private Vector3 _baseScale = Vector3.one;
    private bool _isActive;

    #region 生命周期
    private void Awake() {
        _baseScale = highlightRoot.localScale;
        _normalBackGround = backgroundImage.sprite;
    }
    private void OnEnable() {
        EventBus.Subscribe<ActiveEntityChangedEvent>(this);
        EventBus.Subscribe<EntityStatChangedEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<ActiveEntityChangedEvent>(this);
        EventBus.Unsubscribe<EntityStatChangedEvent>(this);
    }
    #endregion

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

    /// <summary>
    /// 刷新UI显示
    /// </summary>
    private void RefreshUI() {
        CharacterRuntimeData runtimeData = _targetEntity.RuntimeData;
        StatBlock stats = runtimeData.GetTotalStats();
        hpSlider.value = runtimeData.CurrentHP;
        hpText.text = $"{runtimeData.CurrentHP}/{stats.MaxHP}";

        spSlider.value = runtimeData.CurrentSP;
        spText.text = $"{runtimeData.CurrentSP}/{stats.MaxSP}";

        bpSlider.value = runtimeData.CurrentBP;
    }

    #region 事件监听
    public void OnEvent(ActiveEntityChangedEvent evt) {
        if (_targetEntity == null) {
            SetActiveVisual(false);
            return;
        }

        // 当当前行动的实体与UI所绑定的实体对应时则高亮显示
        SetActiveVisual(_targetEntity == evt.Entity);
    }

    private void SetActiveVisual(bool active) {
        if (_isActive == active) {
            return;
        }

        _isActive = active;
        backgroundImage.sprite = active ? activeBackground : _normalBackGround;
        backgroundImage.SetNativeSize();
        highlightRoot.localScale = active ? _baseScale * activeScale : _baseScale;

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public void OnEvent(EntityStatChangedEvent evt) {
        if (evt.Entity != _targetEntity) {
            return;
        }
        RefreshUI();
    }

    #endregion
}
