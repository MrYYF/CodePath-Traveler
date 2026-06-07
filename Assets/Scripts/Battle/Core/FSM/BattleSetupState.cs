


/// <summary>
/// 战斗开始状态
/// <remarks>
/// 该状态主要负责：
/// 1. 读取BattleStartPreload中的数据，创建战斗实体并将其添加到BattleController的实体列表中。
/// 2. 绑定 RuntimeData、BattleEntity 和 BattleUnit 之间的关系，确保它们能够正确交互。
/// 3. 播放最小开场入场表现
/// 4. 初始化首轮时间轴，切换到“下一位行动者”状态
/// 
/// 可以理解为：
/// “战斗开始后的总装配状态”
/// </remarks>
/// </summary>
public class BattleSetupState : BattleState {

    private readonly BattleStartPreload _startPreload;
    public BattleSetupState(BattleContorller contorller, BattleStartPreload startPreload) : base(contorller) {
        _startPreload = startPreload;
    }

    public override IEnumerator Execute() {
        Debug.Log("Entering BattleSetupState...");
        yield return null;
    }

}
