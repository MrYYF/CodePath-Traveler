


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
    }

    public void RecruitMember(CharacterDefinitionSO newCharacter) {
        AddMember(newCharacter);

        GameModeManager.Inastance.RequestChangeGameMode(GameMode.Explore);
    }
}
