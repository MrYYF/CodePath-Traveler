

/// <summary>
/// 招募指令，负责处理与NPC的交互，显示招募面板，并在玩家确认后将NPC添加到队伍中
/// </summary>
public class RecruitAction : ActionBase
{
    public CharacterDefinitionSO CurrentCharacter { get; private set; }

    private void Awake() {
        CurrentCharacter = GetComponent<CharacterIdentity>().CharacterDefinitionSO;
    }

    public override void TriggerAction(AllyDefinitionSO inteactor) {
        EventBus.Publish(new PanelRequestEvent(this));
    }

    public override void Execute(object context = null) {
        PartyManager.Inastance.RecruitMember(CurrentCharacter);
        HideSceneNPC();
    }

    private void HideSceneNPC() {
        gameObject.SetActive(false);
    }
}
