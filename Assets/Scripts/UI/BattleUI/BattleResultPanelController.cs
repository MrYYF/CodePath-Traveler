using Framework.Event;
using TMPro;
using UnityEngine.UI;

public class BattleResultPanelController : MonoBehaviour,
    IEventReceiver<BattleResultViewEnterEvent> {
    #region 结算面板配置
    [Header("Panel Root")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private RectTransform infoHUDRoot;
    [SerializeField] private RectTransform lootItemRoot;

    [Header("Result Text")]
    [SerializeField] private TMP_Text expRewardText;
    [SerializeField] private TMP_Text moneyRewardText;
    [SerializeField] private TMP_Text moneyCurrentText;

    [Header("Prefab")]
    [SerializeField] private InfoHUD infoHUDPrefab;
    [SerializeField] private LootItem lootItemPrefab;

    [Header("Aciton")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private bool hideOnConfirm = true;

    [Header("Exp Animation")]
    [SerializeField] private float expTweenStagger = 0.08f;
    #endregion

    #region 运行时缓存
    private Coroutine _fadeRoutine;
    #endregion

    #region 生命周期
    private void OnEnable() {
        EventBus.Subscribe<BattleResultViewEnterEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<BattleResultViewEnterEvent>(this);
    }
    #endregion

    #region 事件相关
    public void OnEvent(BattleResultViewEnterEvent evt) {
        panelRoot.SetActive(true);
        // 添加战利品到库存
        ApplyInventoryRewards(evt);
        // 刷新文本
        RefreshRewardText(evt);
        // 添加队伍HUD信息
        RebuildPartyInfoHud(evt);
        // 物品掉落
        RebuildLootItems(evt);
        // 打开结算面板
        StopFadeRoutine();
        _fadeRoutine = StartCoroutine(FadeInRoutine());
    }
    #endregion

    /// <summary>
    /// 将结算时的金币和掉落物添加进库存
    /// </summary>
    /// <param name="result">事件参数</param>
    private void ApplyInventoryRewards(BattleResultViewEnterEvent result) {
        // 添加金币
        InventoryManager inventory = InventoryManager.Instance;
        inventory.AddCurrency(result.MoneyReward);

        // 添加物品
        List<BattleDropReward> dropRewards = result.dropRewards;
        foreach (var drop in dropRewards) {
            inventory.AddItem(drop.ItemDefinition, drop.Quantity);
        }
    }

    /// <summary>
    /// 刷新基础奖励文本
    /// </summary>
    /// <param name="result">事件参数</param>
    private void RefreshRewardText(BattleResultViewEnterEvent result) {
        expRewardText.text = "+" + result.ExpReward;
        moneyRewardText.text = "+" + result.MoneyReward;
        moneyCurrentText.text = InventoryManager.Instance.Currency.ToString();
    }

    /// <summary>
    /// 重建队伍HUD
    /// </summary>
    /// <param name="result"></param>
    private void RebuildPartyInfoHud(BattleResultViewEnterEvent result) {
        ClearChildren(infoHUDRoot);
        List<CharacterRuntimeData> partyMembers = PartyManager.Instance.PartyMembers;

        // 统计存活角色数量，并将死亡角色血量置为1
        int aliveCount = 0;
        foreach (var member in partyMembers) {
            if (member.CurrentHP > 0) {
                aliveCount++;
            }
            else {
                member.CurrentHP = 1;
            }
        }

        // 计算均分经验以及余数
        int baseExp = aliveCount > 0 ? result.ExpReward / aliveCount : 0;
        int remainder = aliveCount > 0 ? result.ExpReward % aliveCount : 0;
        int aliveIndex = 0;

        foreach (var member in partyMembers) {
            InfoHUD hud = Instantiate(infoHUDPrefab, infoHUDRoot);
            int startLevel = member.Level;
            int startExp = member.CurrentExp;
            int gainExp = member.CurrentHP > 1 ? baseExp + (aliveIndex++ < remainder ? 1 : 0) : 0;
            int showTargetExp = member.GetExpRequiredToNextLevel();
            float startProgress = member.GetExpProgress01();
            hud.SetInfo(member.Definition.Name, startLevel, startExp, showTargetExp, startProgress, member.Definition.Portrait);

            //TODO:动画
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(infoHUDRoot);
    }

    /// <summary>
    /// 重构战利品掉落列表
    /// </summary>
    /// <param name="result">事件参数</param>
    private void RebuildLootItems(BattleResultViewEnterEvent result) {
        ClearChildren(lootItemRoot);
        foreach (var drop in result.dropRewards) {
            LootItem item = Instantiate(lootItemPrefab, lootItemRoot);
            item.SetLootItem(new InventoryItem(drop.ItemDefinition, drop.Quantity));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(lootItemRoot);
    }

    #region 工具函数
    private IEnumerator FadeInRoutine() {
        canvasGroup.alpha = 0f;
        float t = 0f;
        while (t < fadeInDuration) {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        _fadeRoutine = null;
        confirmButton.Select();
    }

    /// <summary>
    /// 清除父物体下的所有子物体
    /// </summary>
    /// <param name="root">父物体根节点</param>
    private void ClearChildren(RectTransform root) {
        for (int i = 0; i < root.childCount; i++) {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 停止转场协程
    /// </summary>
    private void StopFadeRoutine() {
        if (_fadeRoutine == null) {
            return;
        }
        StopCoroutine(_fadeRoutine);
        _fadeRoutine = null;
    }

    /// <summary>
    /// 立即隐藏结算面板
    /// </summary>
    private void HideImmediate() {
        StopFadeRoutine();
        canvasGroup.alpha = 0;
        ClearChildren(infoHUDRoot);
        ClearChildren(lootItemRoot);
        panelRoot.SetActive(false);
    }
    #endregion
}
