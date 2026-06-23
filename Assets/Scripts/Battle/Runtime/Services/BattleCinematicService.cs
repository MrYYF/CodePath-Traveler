using DG.Tweening;
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

    public float KillDissolveStagger => cinematicConfig.KillDissolveStagger;

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
        StartCoroutine(PlayKillCinematic(evt.Target));
    }

    public void OnEvent(BreakCinematicRequestedEvent evt) {
        StartCoroutine(PlayBreakCinematic());
    }
    #endregion

    #region 击杀与破盾统一演出流程
    private IEnumerator PlayBreakCinematic() {
        breakPostFX.Play();

        if (!cinematicConfig.EnableBreakCinematic) {
            yield break;
        }

        yield return PlayImpactCinematic(cinematicConfig.Break);
    }

    private IEnumerator PlayKillCinematic(BattleEntity target) {
        target.Unit.PlayEnemyDissolve();
        if (!cinematicConfig.EnableKillCinematic) {
            yield break;
        }

        yield return PlayImpactCinematic(cinematicConfig.Kill);

    }

    /// <summary>
    /// 播放停顿效果
    /// </summary>
    /// <param name="settings">演出配置</param>
    /// <returns></returns>
    private IEnumerator PlayImpactCinematic(BattleImpactCinematicSettings settings) {
        // 相机偏移
        Tween cameraTween = PlayCamera(settings);

        // 记录当前时间倍率
        float previousTimeScale = Time.timeScale;

        // 命中时停
        if (settings.HitStopDuration > 0) {
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(settings.HitStopDuration);
        }

        // 进入慢动作
        yield return PlayTimeScale(previousTimeScale, settings.SloMoScale, settings.SlowMoInDuration);

        // 等待镜头动画完成
        yield return cameraTween.WaitForCompletion();

        //// 慢动作持续
        //if (settings.HoldDuration > 0) {
        //    yield return new WaitForSecondsRealtime(settings.HoldDuration);
        //}

        // 退出慢动作
        yield return PlayTimeScale(settings.SloMoScale, previousTimeScale, settings.SlowMoOutDuration);

    }

    #endregion

    #region 相机演出

    /// <summary>
    /// 播放相机动画
    /// </summary>
    /// <param name="settings">配置</param>
    /// <returns></returns>
    private Tween PlayCamera(BattleImpactCinematicSettings settings) {
        // 记录基础位置
        Vector3 basePos = battleCameraPivot.position;
        Quaternion baseRot = battleCameraPivot.rotation;

        // 计算最终位置
        Vector3 toPos = basePos + (baseRot * settings.CameraPositionOffset);
        Quaternion toRot = baseRot * Quaternion.Euler(settings.CameraEulerOffset);

        // 停止上一个补间动画
        battleCameraPivot.DOKill();

        // 开始相机移动 SetUpdate(true)无视timescale用真实时间播放动画
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        AddCameraMove(sequence, toPos, toRot, settings.CameraTurnDuration);

        // 移动后停留
        if (settings.CameraHoldDuration > 0) {
            sequence.AppendInterval(settings.CameraHoldDuration);
        }

        // 回到原位置
        AddCameraMove(sequence, basePos, baseRot, settings.CameraReturnDuration);

        return sequence;
    }

    /// <summary>
    /// 在动画队列中添加相机移动
    /// </summary>
    /// <param name="sequence">队列</param>
    /// <param name="toPos">目标位置</param>
    /// <param name="toRot">目标旋转</param>
    /// <param name="duration">持续时间</param>
    private void AddCameraMove(Sequence sequence, Vector3 toPos, Quaternion toRot, float duration) {
        if (duration <= 0) {
            sequence.AppendCallback(() => battleCameraPivot.SetPositionAndRotation(toPos, toRot));
        }

        sequence.Append(battleCameraPivot.DOMove(toPos, duration).SetEase(Ease.Linear));
        // Join与上一个同时播放
        sequence.Join(battleCameraPivot.DORotateQuaternion(toRot, duration).SetEase(Ease.Linear));
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
