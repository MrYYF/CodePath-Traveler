
/// <summary>
/// 单次战斗指令的执行上下文
/// 
/// 
/// </summary>
public class BattleActionContext {
    public BattleController Controller { get; }

    public BattleEntity Actor { get; }

    public BattleCommandRequest Command { get; }

}
