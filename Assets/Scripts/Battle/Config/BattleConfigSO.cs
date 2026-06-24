
/// <summary>
/// 战斗配置有关类
/// 包括动画时间、攻击间隔等变量
/// </summary>
[CreateAssetMenu(menuName = "Battle/BattleConfig")]
public class BattleConfigSO : ScriptableObject {
    [Header("Flow Timings")]
    public float StartBattleDelay = 0.5f; // 战斗开始后正式进入流程的缓冲时间
    public float TurnStartDelay = 0.3f; // 回合开始前的等待时间
    public float TurnEndDelay = 0.3f; // 回合结束后的停顿时间
    public float VictoryResultDelay = 0.2f; // 胜利面板出现延迟
    public float AIThinkDuration = 1f; // AI行动思考时长

    [Header("Attack Timing")]
    public float GroupTargetHitInterval = 0.05f; //群体目标受击间隔
    public float MultiHitInterval = 0.08f; //多段攻击受击间隔

    [Header("Animation Timing")]
    public float AttackWindupTime = 0.4f; // 攻击前摇时长
    public float AttackRecoveryTime = 0.8f; // 攻击后摇时长
    public float DefendPoseDuration = 0.5f; // 防御姿态保持时间
    public float EscapeRunDuration = 1.2f; // 角色逃跑动作时长
    public float EscapeExitDelay = 0.35f; // 退出战斗场景延迟

}
