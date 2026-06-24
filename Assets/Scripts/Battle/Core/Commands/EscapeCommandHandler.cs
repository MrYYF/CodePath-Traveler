public class EscapeCommandHandler : BattleCommandHandleBase {
    /// <summary>
    /// 执行逃跑
    /// 清理当前目标选择
    /// 幸存单位播放撤退演出
    /// 演出结束后回到战斗前场景
    /// 停止当前状态机
    /// </summary>
    /// <returns></returns>
    public override IEnumerator Execute(BattleController controller) {
        // 清理当前锁定目标
        controller.ClearTargetSelection();

        // 存活单位撤离至出生点
        yield return RunEscape(controller);

        // 请求切换游戏场景
        BattleService.Instance.ReturnToPreviousScene();

        // 退出战斗
        controller.StopBattle();
    }

    private IEnumerator RunEscape(BattleController controller) {
        // 逃跑移动时长
        float escapeDuration = controller.Config.EscapeRunDuration;

        // 遍历单位处理存活的对象移动到生成位置
        bool hasMove = false;
        foreach (var entity in controller.AllEntities) {
            // 跳过死亡、非友方单位
            if (!entity.IsAlive || !entity.IsPlayer) {
                continue;
            }

            // 获取生成位置
            Vector3 initPos = controller.FieldManager.GetInitPos();

            // 判断是否已经就位
            if (Vector3.Distance(initPos, entity.Unit.transform.position) <= 0.01f) {
                continue;
            }

            // 将单位移动到生成位置
            hasMove = true;
            controller.StartCoroutine(entity.Unit.MoveToPosition(initPos, escapeDuration));
        }

        // 如果正在执行退场动画则等待动画完成
        if (hasMove) {
            yield return new WaitForSeconds(escapeDuration);
        }

        // 等待退出战斗场景延迟
        float exitDelay = controller.Config.EscapeExitDelay;
        if (exitDelay > 0f) {
            yield return new WaitForSeconds(exitDelay);
        }
    }
}
