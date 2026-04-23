


/// <summary>
/// 队伍管理器，负责管理玩家的队伍成员数据，并与探索模式下的跟随系统进行交互
/// </summary>
[RequireComponent(typeof(PartyFieldController))]
public class PartyManager : Singleton<PartyManager>
{
    [Header("Initial Party")]
    [SerializeField] private CharacterDefinitionSO PlayerDefinition;

    [SerializeField] private List<CharacterRuntimeData> partyMembers = new();
    public List<CharacterRuntimeData> PartyMembers => partyMembers;

    private PartyFieldController fieldController;

    protected override void Awake() {
        base.Awake();
        InitParty();
        fieldController = GetComponent<PartyFieldController>();
    }

    private void InitParty() {
        if (partyMembers.Count == 0) {
            partyMembers.Add(new CharacterRuntimeData(PlayerDefinition));
        }
    }

    private void AddMember(CharacterDefinitionSO characterDefinition) {
        partyMembers.Add(new CharacterRuntimeData(characterDefinition));
        RefreshFieldFollowers();
    }

    public void RecruitMember(CharacterDefinitionSO newCharacter) {
        AddMember(newCharacter);

        GameModeManager.Instance.RequestChangeGameMode(GameMode.Explore);
    }

    private void RefreshFieldFollowers() {
        List<CharacterDefinitionSO> defs = new(partyMembers.Count);

        foreach (var member in partyMembers) {
            defs.Add(member.Definition);
        }

        fieldController.UpdateFollowers(defs);
    }
}
