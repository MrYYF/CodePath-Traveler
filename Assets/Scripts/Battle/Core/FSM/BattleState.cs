/// <summary>
/// 战斗状态基类，定义了战斗状态的基本结构和生命周期方法（Enter、Execute、Exit）。
/// </summary>
public abstract class BattleState {

    protected readonly BattleController _controller;

    protected BattleState(BattleController controller) {
        _controller = controller;
    }

    public virtual IEnumerator Enter() {
        yield break;
    }

    public abstract IEnumerator Execute();

    public virtual IEnumerator Exit() {
        yield break;
    }
}
