using System;

/// <summary>
/// 地图指令的基类，用于存储指令信息、触发职业条件等
/// 决定被挂载对象有哪些交互指令
/// </summary>
public abstract class ActionBase : MonoBehaviour
{
    public Job matchJob = Job.Any;

    public ActionCommandInfo CommandInfo;

    public virtual bool CanShow(AllyDefinitionSO inteactor) {
        return isJobMatch(inteactor);
    }

    /// <summary>
    /// 判断指令是否可以被执行，默认只判断职业条件，特殊指令可以重写这个方法添加额外的条件
    /// </summary>
    /// <param name="inteactor">执行指令的角色信息</param>
    /// <returns>指令是否可以被执行</returns>
    public virtual bool CanExecute(AllyDefinitionSO inteactor) {
        return true;
    }

    /// <summary>
    /// 需要二级面板确认的操作在这里触发
    /// </summary>
    /// <param name="inteactor">执行指令的角色信息</param>
    public virtual void TriggerAction(AllyDefinitionSO inteactor) {
        Execute();
    }

    /// <summary>
    /// 直接执行指令
    /// </summary>
    public virtual void Execute() {
        // Default implementation does nothing
    }

    private bool isJobMatch(AllyDefinitionSO inteactor) {
        return matchJob == Job.Any || inteactor.Job == CommandInfo.RequiredJob;
    }
}

[Serializable]
public struct ActionCommandInfo {
    public string CommandName;
    public string Description;
    public Sprite Icon;
    public Job RequiredJob;
    public int Order;
}

