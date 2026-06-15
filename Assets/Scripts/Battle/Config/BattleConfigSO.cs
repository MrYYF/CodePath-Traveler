
/// <summary>
/// 战斗配置有关类
/// 包括动画时间、攻击间隔等变量
/// </summary>
[CreateAssetMenu(menuName = "Battle/BattleConfig")]
public class BattleConfigSO : ScriptableObject {
    [Header("Attack Timing")]
    public float GroupTargetHitInterval = 0.05f; //群体目标受击间隔
    public float MultiHitInterval = 0.08f; //多段攻击受击间隔

    [Header("Animation Timing")]
    public float AttackWindupTime = 0.4f; // 攻击前摇时长
    public float AttackRecoveryTime = 0.8f; // 攻击后摇时长
    public float DefendPoseDuration = 0.5f; // 防御姿态保持时间

}
