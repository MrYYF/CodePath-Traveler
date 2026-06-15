
/// <summary>
/// 战斗命令执行基类
/// </summary>
public abstract class BattleCommandHandleBase {
    protected BattleController Controller { get; set; }

    protected BattleEntity Actor => Controller.CurrentEntity;

    protected BattleCommandRequest Command => Controller.CurrentCommandRequest;

    #region 入口
    public virtual IEnumerator Execute(BattleController controller) {
        Controller = controller;

        if (!PreparePhase()) {
            yield break;
        }

        yield return AnimationPhase();
        yield return ExecutionPhase();
        yield return ResolvePhase();
    }

    #endregion

    #region 四阶段扩展点
    /// <summary>
    /// 参数校验、资源扣除、目标解析等前置准备阶段
    /// </summary>
    /// <returns></returns>
    protected virtual bool PreparePhase() => true;

    /// <summary>
    /// 动作演出阶段
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator AnimationPhase() { yield break; }

    /// <summary>
    /// 核心效果结算
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator ExecutionPhase() { yield break; }

    /// <summary>
    /// 收尾阶段，等待恢复，清理临时状态
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator ResolvePhase() { yield break; }

    #endregion
}
