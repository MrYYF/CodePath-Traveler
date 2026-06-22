using Framework.Event;

public class BattleCinematicService : MonoBehaviour,
    IEventReceiver<KillCinematicRequestedEvent>,
    IEventReceiver<BreakCinematicRequestedEvent> {
    [Header("Scene References")]
    [SerializeField, Tooltip("breakvolume后处理组件")]
    private BattleHitPostFX breakPostFX;
    [SerializeField, Tooltip("battlecamera空父节点")]
    private Transform battleCameraPivot;

    [Header("Cinematic Config")]
    [SerializeField, Tooltip("战斗演出参数SO")]
    private BattleCinematicConfigSO cinematicConfig;

    #region 生命周期
    private void OnEnable() {
        EventBus.Subscribe<KillCinematicRequestedEvent>(this);
        EventBus.Subscribe<BreakCinematicRequestedEvent>(this);
    }

    private void OnDisable() {
        EventBus.Unsubscribe<KillCinematicRequestedEvent>(this);
        EventBus.Unsubscribe<BreakCinematicRequestedEvent>(this);
    }
    #endregion

    #region 事件相关
    public void OnEvent(KillCinematicRequestedEvent evt) {
        throw new System.NotImplementedException();
    }

    public void OnEvent(BreakCinematicRequestedEvent evt) {
        StartCoroutine(PlayBreakCinematic(evt.Target));
    }
    #endregion

    #region 击杀与破盾统一演出流程
    private IEnumerator PlayBreakCinematic(BattleEntity target) {
        if (!cinematicConfig.EnableBreakCinematic) {
            yield break;
        }

        breakPostFX.Play();
        yield return PlayImpactCinematic(target, cinematicConfig.Break);
    }

    /// <summary>
    /// 播放停顿效果
    /// </summary>
    /// <param name="target">目标实体</param>
    /// <param name="settings">演出配置</param>
    /// <returns></returns>
    private IEnumerator PlayImpactCinematic(BattleEntity target, BattleImpactCinematicSettings settings) {
        float previousTimeScale = Time.timeScale;

        // 命中时停
        if (settings.HitStopDuration > 0) {
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(settings.HitStopDuration);
        }

        // 进入慢动作
        yield return PlayTimeScale(previousTimeScale, settings.SloMoScale, settings.SlowMoInDuration);

        // 慢动作持续
        if (settings.HoldDuration > 0) {
            yield return new WaitForSecondsRealtime(settings.HoldDuration);
        }

        // 退出慢动作
        yield return PlayTimeScale(settings.SloMoScale, previousTimeScale, settings.SlowMoOutDuration);

    }

    #endregion

    #region 时间工具
    /// <summary>
    /// 控制时间流速
    /// </summary>
    /// <param name="from">起始流速</param>
    /// <param name="to">终点流速</param>
    /// <param name="duration">过渡时间</param>
    /// <returns></returns>
    private static IEnumerator PlayTimeScale(float from, float to, float duration) {
        if (duration <= 0f) {
            Time.timeScale = to;
            yield break;
        }

        // 用真实时间累加
        float elapse = 0f;
        while (elapse < duration) {
            elapse += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(from, to, elapse / duration);
            yield return null;
        }

        Time.timeScale = to;
    }

    #endregion
}
