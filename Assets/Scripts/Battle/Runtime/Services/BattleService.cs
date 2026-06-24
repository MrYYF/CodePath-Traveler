using Framework.Event;
using UnityEngine.AddressableAssets;


/// <summary>
/// 战斗服务，负责管理战斗的开始和结束流程，包括场景切换、战斗数据准备等。
/// </summary>
public class BattleService : Singleton<BattleService>,
    IEventReceiver<BattleResultConfirmedEvent> {
    // 战斗结束后返回的场景地址引用
    private AssetReference _returnSceneAfterBattle;
    // 战斗开始预加载数据，包含战斗开始前需要准备的角色数据和敌人布阵信息
    private BattleStartPreload _pendingPreload;
    // 是否有待处理的战斗预加载数据
    public bool HasPendingPreload => _pendingPreload != null;

    #region 生命周期
    private void OnEnable() {
        EventBus.Subscribe<BattleResultConfirmedEvent>(this);
    }
    private void OnDisable() {
        EventBus.Subscribe<BattleResultConfirmedEvent>(this);
    }
    #endregion

    /// <summary>
    /// 消费战斗开始预加载数据
    /// </summary>
    /// <returns></returns>
    public BattleStartPreload ConsumeStartPreload() {
        var preload = _pendingPreload;
        _pendingPreload = null;
        return preload;
    }

    /// <summary>
    /// 根据提供的 ChallengeAction 构建敌方角色列表，并使用当前队伍、指定战斗场景和敌方阵型启动战斗。
    /// </summary>
    /// <remarks>从 PartyManager 获取当前队伍成员；enemyTeamMembers 中的 null 条目会被忽略。</remarks>
    /// <param name="challengeAction">包含敌方成员定义、战斗场景引用和敌方阵型的配置对象。</param>
    public void StartBattleFromAction(ChallengeAction challengeAction) {
        List<CharacterRuntimeData> allies = PartyManager.Instance.PartyMembers;
        List<CharacterRuntimeData> enemy = new List<CharacterRuntimeData>();
        foreach (var enemyDef in challengeAction.enemyTeamMembers) {
            if (enemyDef == null) continue;
            enemy.Add(new CharacterRuntimeData(enemyDef));
        }

        StartBattle(allies,
            enemy,
            challengeAction.battleSceneReference,
            challengeAction.enemyLayoutFomation);
    }

    /// <summary>
    /// 进入战斗场景。该方法会缓存当前场景以便战斗结束后返回，重置战斗状态，并创建战斗预加载数据，然后请求加载战斗场景。
    /// </summary>
    /// <param name="allies">友方角色列表</param>
    /// <param name="enemy">敌方角色列表</param>
    /// <param name="battleScene">战斗场景引用</param>
    /// <param name="enemyLayoutFomation">敌方阵型</param>
    public void StartBattle(List<CharacterRuntimeData> allies,
        List<CharacterRuntimeData> enemy,
        AssetReference battleScene,
        EnemyLayoutFomation enemyLayoutFomation) {

        SceneLoadManager sceneLoadManager = SceneLoadManager.Instance;

        // 缓存当前场景，以便战斗结束后返回
        _returnSceneAfterBattle = sceneLoadManager.activeScene;

        // 重置战斗状态
        NormalizeBattleSessionState(allies);

        // 创建战斗预加载数据
        _pendingPreload = new BattleStartPreload(
            new List<CharacterRuntimeData>(allies),
            new List<CharacterRuntimeData>(enemy),
            enemyLayoutFomation);

        // 加载战斗场景
        sceneLoadManager.RequestLoad(new SceneLoadRequest(battleScene, FadeStyle.WipeMask, GameMode.Battle));
    }

    /// <summary>
    /// 初始化战斗会话状态
    /// </summary>
    /// <param name="members">友方角色列表</param>
    private void NormalizeBattleSessionState(List<CharacterRuntimeData> members) {
        foreach (var member in members) {
            member.ResetBattleBP();
            if (member.CurrentHP <= 0) {
                member.CurrentHP = 1;
            }
        }
    }


    #region 事件相关
    public void OnEvent(BattleResultConfirmedEvent evt) {
        ReturnToPreviousScene();
    }
    #endregion

    /// <summary>
    /// 战斗离场/逃离 回到之前的场景
    /// </summary>
    public void ReturnToPreviousScene() {
        // 请求返回上一个场景
        SceneLoadManager.Instance.RequestLoad(
            new SceneLoadRequest(
                _returnSceneAfterBattle,
                FadeStyle.WipeMask,
                GameMode.Explore));

        // 清除本次战斗缓存
        _pendingPreload = null;
        _returnSceneAfterBattle = null;

        // 重置战斗会话状态
        NormalizeBattleSessionState(PartyManager.Instance.PartyMembers);
    }
}
