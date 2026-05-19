
using UnityEngine.AddressableAssets;

/// <summary>
/// 挑战指令，负责处理与NPC的交互，显示挑战面板，并在玩家确认后触发挑战事件
/// </summary>
public class ChallengeAction : ActionBase {
    [Header("Challenge Action")]
    public AssetReference battleSceneReference;
    public List<CharacterDefinitionSO> enemyTeamMembers;
    [Tooltip("敌人阵型，决定敌人站位和战斗策略")]
    public EnemyLayoutFomation enemyLayoutFomation = EnemyLayoutFomation.Line;
    public int LastDifficulty { get; private set; }

    public CharacterDefinitionSO CurrentCharacter { get; private set; }

    private void Awake() {
        CurrentCharacter = GetComponent<CharacterIdentity>().CharacterDefinitionSO;
        enemyTeamMembers ??= new();
        if (!enemyTeamMembers.Contains(CurrentCharacter)) {
            enemyTeamMembers.Insert(0, CurrentCharacter);
        }
    }

    public override void TriggerAction(AllyDefinitionSO inteactor) {
        LastDifficulty = EvaluateDifficulty();
        EventBus.Publish(new PanelRequestEvent(this));
    }

    /// <summary>
    /// 计算玩家队伍的战斗力
    /// </summary>
    /// <returns>战斗力评估值</returns>
    private int EvaluatePlayerTeamPower() {
        List<CharacterRuntimeData> partyMembers = PartyManager.Instance.PartyMembers;
        int totalPower = 0;
        foreach (CharacterRuntimeData member in partyMembers) {
            totalPower += CharacterRuntimeData.EvaluatePowerFromStats(member.GetTotalStats());
        }

        return totalPower;
    }

    /// <summary>
    /// 计算敌人队伍的战斗力
    /// </summary>
    /// <returns>战斗力评估值</returns>
    private int EvaluateEnemyTeamPower() {
        int totalPower = 0;
        foreach (CharacterDefinitionSO enemy in enemyTeamMembers) {
            // 这里简单地用敌人的基础属性来评估战斗力，实际可以根据敌人等级、特殊能力等因素进行更复杂的评估
            totalPower += CharacterRuntimeData.EvaluatePowerFromStats(enemy.BaseStats);
        }
        return totalPower;
    }

    private int EvaluateDifficulty() {
        int playerPower = EvaluatePlayerTeamPower();
        int enemyPower = EvaluateEnemyTeamPower();
        float ratio = enemyPower / (float)playerPower;
        if(ratio < 0.5f) return 0;
        if(ratio < 0.7f) return 1;
        if(ratio < 0.9f) return 2;
        if(ratio < 1.1f) return 3;
        if(ratio < 1.3f) return 4;
        if (ratio < 1.5f) return 5;
        if (ratio < 1.7f) return 6;
        if (ratio < 1.9f) return 7;
        if (ratio < 2.1f) return 8;
        if (ratio < 2.3f) return 9;
        return 10;

    }
}
