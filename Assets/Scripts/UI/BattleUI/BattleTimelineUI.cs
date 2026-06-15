
using DG.Tweening;
using TMPro;
using UnityEngine.Pool;
using UnityEngine.UI;

/// <summary>
/// 控制战斗CTB时间轴UI层的类
/// </summary>
public class BattleTimelineUI : MonoBehaviour {
    [SerializeField] private TimelineIcon timelineIconPrefab;
    [Header("Containers")]
    [SerializeField] private RectTransform currentRoundContainer;
    [SerializeField] private RectTransform nextRoundContainer;
    [Header("当前行动单位")]
    [SerializeField] private Image activeUnitPortrait; // 头像
    [SerializeField] private TMP_Text activeUnitName; // 名字
    [Header("Animation")]
    [SerializeField, Tooltip("图标淡入和滑动的基础时常")] private float animDuration = 0.5f;
    [SerializeField, Tooltip("滑动时的缓动曲线")] private Ease moveEase = Ease.OutQuart;
    [SerializeField, Tooltip("队列头像依次出现的间隔")] private float spawnStagger = 0.03f;
    [SerializeField, Tooltip("入场前x轴位置偏移")] private float spawnOffsetX = 40f;

    private IObjectPool<TimelineIcon> _pool;
    // 已经激活的图标id映射
    private readonly Dictionary<string, TimelineIcon> _activeIconMaps = new();

    private void Awake() {
        // 初始化对象池
        _pool = new ObjectPool<TimelineIcon>(
            createFunc: () => Instantiate(timelineIconPrefab, transform),
            actionOnGet: icon => {
                icon.gameObject.SetActive(true);
                icon.transform.SetAsLastSibling(); // 确保新获取的图标在最后面
            },
            actionOnRelease: icon => {
                icon.gameObject.SetActive(false);
                icon.transform.SetParent(transform);

                // 回收时重置视觉偏移，避免服用后还停留在上次动画位置

            },
            actionOnDestroy: icon => Destroy(icon.gameObject),
            defaultCapacity: 10,
            maxSize: 16
        );
    }

    #region 时间轴刷新主流程
    /// <summary>
    /// 更新时间轴的UI
    /// </summary>
    /// <param name="predictions">时间轴预测节点</param>
    public void UpdateTimeline(List<BattleTimelinePredictionNode> predictions) {
        // 记录这次预测包含的id
        HashSet<string> keptIDs = new HashSet<string>();

        // 核心循环
        for (int i = 0; i < predictions.Count; i++) {
            BattleTimelinePredictionNode node = predictions[i];
            keptIDs.Add(node.UniqueID);

            RectTransform targetParent = (node.Round == 0) ? currentRoundContainer : nextRoundContainer;

            TimelineIcon icon;
            // 如果图标已存在则直接更新父级
            if (_activeIconMaps.TryGetValue(node.UniqueID, out icon)) {
                icon.transform.SetParent(targetParent, false);
                icon.transform.SetAsLastSibling();
                icon.Setup(node.Entity);
            }
            else {
                icon = GetIcon(targetParent);
                _activeIconMaps[node.UniqueID] = icon;
                icon.Setup(node.Entity);

                if (isActiveAndEnabled) {
                    float delay = i * spawnStagger;
                    StartCoroutine(AnimateSpawn(icon, targetParent, delay));
                }
            }
        }

        // 清理旧图标
        List<string> toRemoveIDs = new List<string>();
        foreach (var pair in _activeIconMaps) {
            // 保留的id中没有则添加到删除列表
            if (!keptIDs.Contains(pair.Key)) {
                toRemoveIDs.Add(pair.Key);
                TimelineIcon icon = pair.Value;
                if (isActiveAndEnabled) {
                    StartCoroutine(AnimateDespawn(icon));
                }
                else {
                    _pool.Release(icon);
                }
            }
        }

        foreach (var id in toRemoveIDs) {
            _activeIconMaps.Remove(id);
        }

        // 强制刷新一下UI布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(currentRoundContainer);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nextRoundContainer);
    }

    /// <summary>
    /// 从对象池中取用时间轴图标并将其置于指定父物体下
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    private TimelineIcon GetIcon(Transform parent) {
        TimelineIcon icon = _pool.Get();
        icon.transform.SetParent(parent);
        icon.transform.SetAsLastSibling();
        return icon;
    }
    #endregion

    #region 当前行动者焦点辅助
    public void SetActiveEntity(BattleEntity entity) {
        UpdateActiveUnitFrame(entity);
        PlayActiveUnitFrameAnimation();
    }

    /// <summary>
    /// 根据传入的entity更新当前行动者UI
    /// </summary>
    /// <param name="entity">当前行动者</param>
    private void UpdateActiveUnitFrame(BattleEntity entity) {
        ClearActiveUnitFrame();
        activeUnitPortrait.sprite = entity.Definition.Portrait;
        activeUnitPortrait.enabled = true;
        activeUnitName.text = entity.Definition.Name;
    }
    /// <summary>
    /// 清除当前行动者UI
    /// </summary>
    private void ClearActiveUnitFrame() {
        activeUnitPortrait.enabled = false;
        activeUnitName.text = string.Empty;
    }
    #endregion

    #region 时间轴动画协程
    /// <summary>
    /// 图标入场动画
    /// </summary>
    /// <param name="icon">人物图标</param>
    /// <param name="container">父级容器</param>
    /// <param name="delay">延迟</param>
    /// <returns></returns>
    private IEnumerator AnimateSpawn(TimelineIcon icon, RectTransform container, float delay) {
        if (delay > 0) {
            yield return new WaitForSeconds(delay);
        }
        icon.PlayEntranceAnimation(animDuration, spawnOffsetX, moveEase);

        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }

    /// <summary>
    /// 图标退场动画
    /// </summary>
    /// <param name="icon">图标</param>
    /// <returns></returns>
    private IEnumerator AnimateDespawn(TimelineIcon icon) {
        float duration = animDuration * 0.6f;
        icon.PlayExitAnimation(duration, -spawnOffsetX, moveEase);
        yield return new WaitForSeconds(duration);
        _pool.Release(icon);
    }

    /// <summary>
    /// 当前行动者框的动画
    /// </summary>
    private void PlayActiveUnitFrameAnimation() {
        activeUnitPortrait.transform.DOKill();
        activeUnitPortrait.transform.localScale = Vector3.one * 0.5f;
        activeUnitPortrait.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }
    #endregion
}
