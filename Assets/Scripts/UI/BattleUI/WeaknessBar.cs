using Framework.Event;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 弱点条UI层组件
/// </summary>
public class WeaknessBar : MonoBehaviour,
    IEventReceiver<EntityShieldChangedEvent>,
    IEventReceiver<EntityWeaknessChangedEvent>,
    IEventReceiver<EntityRecoverFromBreakEvent> {
    #region 弱点条配置与缓存
    [Header("Shield")]
    [SerializeField] private TMP_Text shieldText;

    [Header("Weakness")]
    [SerializeField] private RectTransform weakRoot;
    [SerializeField] private GameObject weakIconPrefab;

    private readonly List<GameObject> _spawnedIcons = new List<GameObject>();
    private BattleEntity _targetEntity;

    private DamageTypeIconSetSO _iconSet;
    #endregion

    #region 生命周期
    private void OnEnable() {
        EventBus.Subscribe<EntityShieldChangedEvent>(this);
        EventBus.Subscribe<EntityWeaknessChangedEvent>(this);
        EventBus.Subscribe<EntityRecoverFromBreakEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<EntityShieldChangedEvent>(this);
        EventBus.Unsubscribe<EntityWeaknessChangedEvent>(this);
        EventBus.Unsubscribe<EntityRecoverFromBreakEvent>(this);
    }

    #endregion

    public void Setup(BattleEntity targetEntity, DamageTypeIconSetSO iconSet) {
        _targetEntity = targetEntity;
        _iconSet = iconSet;

        // 更新护盾
        RefreshShield();

        // 更新弱点
        RebuildWeaknessIcons();
    }

    /// <summary>
    /// 刷新护盾
    /// </summary>
    private void RefreshShield() {
        shieldText.text = _targetEntity.CurrentShield.ToString();
    }

    /// <summary>
    /// 重构弱点图标
    /// </summary>
    private void RebuildWeaknessIcons() {
        List<DamageType> weaknesses = _targetEntity.GetWeaknesses();
        if (weaknesses.Count <= 0) {
            return;
        }

        foreach (var weakness in weaknesses) {
            Sprite icon = _iconSet.GetIcon(weakness);
            GameObject instance = Instantiate(weakIconPrefab, weakRoot);
            instance.SetActive(true);
            _spawnedIcons.Add(instance);

            Image iconImage = instance.transform.Find("WeaknessIcon").GetComponent<Image>();
            iconImage.sprite = icon;
        }
    }

    public void SetVisible(bool visible) {
        if(gameObject.activeSelf == visible) {
            return;
        }
        gameObject.SetActive(visible);
        if (visible) {
            RefreshShield();
            RebuildWeaknessIcons();
        }
    }

    /// <summary>
    /// 设置屏幕坐标
    /// </summary>
    /// <param name="position"></param>
    public void SetScreenPosition(Vector2 position) {
        ((RectTransform)transform).anchoredPosition = position;
    }


    #region 事件监听
    public void OnEvent(EntityWeaknessChangedEvent evt) {
        if(evt.Target != _targetEntity) {
            return;
        }
        RebuildWeaknessIcons();
    }

    public void OnEvent(EntityShieldChangedEvent evt) {
        if(evt.Target != _targetEntity) {
            return;
        }
        RefreshShield();
    }

    public void OnEvent(EntityRecoverFromBreakEvent evt) {
        if (evt.Target != _targetEntity) {
            return;
        }
        RefreshShield();
    }
    #endregion
}
