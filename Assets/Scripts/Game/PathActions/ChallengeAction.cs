
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
        if(!enemyTeamMembers.Contains(CurrentCharacter)) {
            enemyTeamMembers.Insert(0, CurrentCharacter);
        }
    }

    public override void TriggerAction(AllyDefinitionSO inteactor) {
        EventBus.Publish(new PanelRequestEvent(this));
    }
}
