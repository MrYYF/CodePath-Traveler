


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
        base.Execute(context);

    }
}
