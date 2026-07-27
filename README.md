<div align="center">
    <h1>CodePath Traveler</h1>
    <p>一款基于unity引擎独立设计与开发一款复刻《歧路旅人》风格的HD-2D回合制RPG核心框架</p>
</div>

<img src=".\docs\img\标题界面.png" alt="标题界面" style="zoom:50%;" />



## 项目介绍

本项目旨在复刻经典JRPG《歧路旅人》的核心玩法，并在此基础上探索模块化Gameplay架构、数据驱动设计以及可扩展的战斗系统实现。

项目以模块化架构为核心，完整实现了地图探索、NPC交互、角色成长、装备商店、随机遇敌及CTB回合制战斗等经典JRPG玩法闭环，致力于通过复刻经典来验证和提升工程化开发能力。

工作期间持续利用业余时间学习 Unity，并独立开发 HD-2D JRPG 项目，逐步完成地图探索、CTB 战斗、NPC 系统等核心玩法，决定将职业方向转向游戏开发。



## 功能展示

### NPC交互

<img src=".\docs\img\NPC交互菜单.png" alt="NPC交互菜单界面" style="zoom:50%;" />

实现了包含打听、招募、偷窃、挑战等多个交互指令



### 二级指令面板

<img src=".\docs\img\打听功能.png" alt="打听功能" style="zoom:50%;" />





### 商店

<img src=".\docs\img\商店面板.png" alt="商店面板" style="zoom:50%;" />

物品描述还可用富文本进行编写，实现物品描述的详细定义



### 物品栏&装备

<img src=".\docs\img\库存面板.png" alt="库存面板" style="zoom:50%;" />

<img src=".\docs\img\装备面板.png" alt="装备面板" style="zoom:50%;" />



### CTB战斗

<img src=".\docs\img\Boss阶段转换.png" alt="Boss阶段转换" style="zoom:60%;" />

采用CTB时间轴的方式实现回合制战斗

添加护盾、弱点、元素/状态/属性、BP点数等多种战斗机制



### 战斗结算

<img src=".\docs\img\战斗胜利结算界面.png" alt="战斗胜利结算界面" style="zoom:60%;" />

战斗结束人物获取经验值，根据成长等级增加属性

战利品按照物品权重配置随机掉落



## 项目细节

### ScriptableObject 数据驱动

<img src=".\docs\process\ScriptableObject .png" alt="ScriptableObject " style="zoom: 80%;" />

游戏中存在大量配置数据，ScriptableObject可以通过不修改代码的方式修改游戏配置，同时便于游戏策划后续的维护与调整。



#### 为什么选择 ScriptableObject？

ScriptableObject 可以：

- 将配置数据与逻辑分离
- 支持 Inspector 可视化编辑
- 避免大量 Json 解析
- 天然支持资源管理

非常适合数据配置



#### 具体实现

``` c#
[CreateAssetMenu(menuName = "Battle/Skill")]
public class SkillDataSO : ScriptableObject {}
```



### EventBus事件总线管理

![EventBus](.\docs\process\EventBus.png)

使用事件总线的方式可以降低系统之间的直接依赖，避免模块之间互相耦合。同时实现模块之间只需通过监听事件而非直接引用的方式来进行数据与信息的传递。



#### 具体实现

利用泛型定义事件接口与事件接收接口：

``` c#
public interface IEvent {}
```

``` c#
public interface IEventReceiver<TEvent> where TEvent : IEvent {
    void OnEvent(TEvent evt);
}
```

事件总线EventBus：

``` c#
/// <summary>
/// 事件总线，负责事件的订阅、发布、取消订阅
/// </summary>
public static class EventBus {
    // 事件订阅字典
    private static readonly Dictionary<Type, List<object>> EventDic = new();
	// 订阅事件
    public static void Subscribe<TEvent>(IEventReceiver<TEvent> receiver) where TEvent : IEvent {}
	// 取消订阅事件
    public static void Unsubscribe<TEvent>(IEventReceiver<TEvent> receiver) where TEvent : IEvent {}
	// 发布事件
    public static void Publish<TEvent>(TEvent evt) where TEvent : IEvent {}
}
```

事件定义：

``` c#
public readonly struct GameModeChangedEvent : IEvent
{
    public readonly GameMode NewGameMode;

    public GameModeChangedEvent(GameMode newGameMode)
    {
        this.NewGameMode = newGameMode;
    }
}
```

订阅&取消订阅：

``` c#
private void OnEnable() {
    EventBus.Subscribe<GameModeChangedEvent>(this);
}
private void OnDisable() {
    EventBus.Unsubscribe<GameModeChangedEvent>(this);
}
```

调用：

``` c#
EventBus.Publish(new GameModeChangedEvent(CurrentGameMode));
```



### FSM有限状态机

![FSM](.\docs\process\FSM.png)

通过State Pattern将每一个状态单独设置为一个类，负责Enter、Update、Exit等不同阶段的状态处理。

这样可以防止随着技能或状态的增加导致的越来越难以维护的问题。



#### 具体实现

状态基类：

``` c#
public abstract class BattleState {
	// 进入
    public virtual IEnumerator Enter() {
        yield break;
    }

    // 执行
    public abstract IEnumerator Execute();

    // 退出
    public virtual IEnumerator Exit() {
        yield break;
    }
}
```

继承实现状态类的功能：

``` c#
public class TargetSelectionState : BattleState {}
```

状态机循环：

``` C#
/// <summary>
/// 标准状态机的战斗循环，持续执行当前状态的Enter、Execute、Exit方法，直到战斗结束或状态发生变化。
/// </summary>
/// <returns></returns>
private IEnumerator BattleLoopRoutine() {
    while (_battleRunning && _currentState != null) {
        // 存储当前状态的快照，以防在执行过程中状态发生变化
        BattleState stateSnapshot = _currentState;

        yield return StartCoroutine(stateSnapshot.Enter());

        // 在执行过程中，如果状态发生变化，立即跳出当前状态的执行，进入新的状态
        if (stateSnapshot != _currentState) {
            yield return StartCoroutine(stateSnapshot.Exit());
            continue;
        }

        yield return StartCoroutine(stateSnapshot.Execute());

        yield return StartCoroutine(stateSnapshot.Exit());

    }

    _battleLoopRoutine = null;
}
```



### ActionBase

![ActionBase](.\docs\process\ActionBase.png)

通过ActionBase来定义不同的交互命令，所有对象只需要挂载不同的Action即可实现对应的交互命令，便于后续命令增多后的维护与拓展。

#### 具体实现

``` c#
/// <summary>
/// 地图指令的基类，用于存储指令信息、触发职业条件等
/// 决定被挂载对象有哪些交互指令
/// </summary>
public abstract class ActionBase : MonoBehaviour
{
    // 判断指令是否可以被执行，默认只判断职业条件，特殊指令可以重写这个方法添加额外的条件
    public virtual bool CanExecute(AllyDefinitionSO inteactor) {
        return true;
    }

    // 需要二级面板确认的操作在这里触发
    public virtual void TriggerAction(AllyDefinitionSO inteactor) {}

    // 直接执行指令
    public virtual void Execute() {}
}
```



### CTB行动顺序调度算法

![CTB](.\docs\process\CTB.png)

根据单位的速度构建单位行动顺序条，同时引入回合机制，角色下回合的行动顺序会受到当前状态影响实时改变

#### 具体实现

``` c#
private List<BattleEntity> GenerateSortedOrder(List<BattleEntity> allEntities) {
    List<BattleEntity> result = new List<BattleEntity>();

    for (int i = 0; i < allEntities.Count; i++) {
        if (allEntities[i].IsAlive) {
            result.Add(allEntities[i]);
        }
    }
    result.Sort((a, b) => {
        int speedCompare = b.GetCurrentSpeed().CompareTo(a.GetCurrentSpeed());
        if (speedCompare != 0) {
            return speedCompare;
        }

        if (a.IsPlayer) return -1;
        if (b.IsPlayer) return 1;
        return string.CompareOrdinal(a.ID, b.ID);
    });

    string orderLog = "CTB顺序：";
    for (int i = 0; i < result.Count; i++) {
        BattleEntity entity = result[i];
        orderLog += $"{i + 1}.{entity.Definition.Name}({entity.GetCurrentSpeed()})";
    }
    return result;
}
```

