
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
    public virtual bool CanExecute(AllyDefinitionSO inteactor) {
        return true;
    }
    public virtual void TriggerAction(AllyDefinitionSO inteactor) {
        // 需要二级面板确认的操作在这里触发
        Execute(inteactor);
    }
    public virtual void Execute(object context = null) {
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

