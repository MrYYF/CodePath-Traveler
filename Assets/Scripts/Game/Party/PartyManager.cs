


using Framework.Event;

/// <summary>
/// 队伍管理器，负责管理玩家的队伍成员数据，并与探索模式下的跟随系统进行交互
/// </summary>
[RequireComponent(typeof(PartyFieldController))]
public class PartyManager : Singleton<PartyManager>,
    IEventReceiver<GameModeChangedEvent> {
    [Header("Initial Party")]
    [SerializeField] private CharacterDefinitionSO PlayerDefinition;

    [SerializeField] private List<CharacterRuntimeData> partyMembers = new();
    public List<CharacterRuntimeData> PartyMembers => partyMembers;

    private PartyFieldController fieldController;

    private bool fieldActorsHidden = false; // 用于跟踪探索模式下的跟随者是否被隐藏

    protected override void Awake() {
        base.Awake();
        fieldController = GetComponent<PartyFieldController>();
        InitParty();
    }

    private void Start() {
        ApplyPartyInitialEquipment();
    }
    private void OnEnable() {
        EventBus.Subscribe<GameModeChangedEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
    }

    /// <summary>
    /// 初始化队伍成员数据，如果没有预设的玩家角色定义，则创建一个默认的玩家角色实例，并将其添加到队伍成员列表中
    /// </summary>
    private void InitParty() {
        if (partyMembers.Count == 0) {
            partyMembers.Add(new CharacterRuntimeData(PlayerDefinition));
        }
    }

    /// <summary>
    /// 添加新成员到队伍中，并刷新探索模式下的跟随者列表以反映新的队伍成员
    /// </summary>
    /// <param name="characterDefinition">要添加的新成员的角色定义</param>
    private void AddMember(CharacterDefinitionSO characterDefinition) {
        partyMembers.Add(new CharacterRuntimeData(characterDefinition));
        RefreshFieldFollowers();
    }

    /// <summary>
    /// 招募新成员到队伍中，并切换回探索模式以更新跟随者列表
    /// </summary>
    /// <param name="newCharacter">要招募的新成员的角色定义</param>
    public void RecruitMember(CharacterDefinitionSO newCharacter) {
        FadeController.Instance.SetStyle(FadeStyle.PanelFade);
        FadeController.Instance.FadeOut(() => {
            AddMember(newCharacter);
            FadeController.Instance.FadeIn(() => GameModeManager.Instance.RequestChangeGameMode(GameMode.Explore));
        });
    }

    /// <summary>
    /// 刷新探索模式下的跟随者列表，使其与当前队伍成员保持一致
    /// </summary>
    private void RefreshFieldFollowers() {
        List<CharacterDefinitionSO> defs = new(partyMembers.Count);

        foreach (var member in partyMembers) {
            defs.Add(member.Definition);
        }

        fieldController.UpdateFollowers(defs);
    }

    /// <summary>
    /// 应用队伍成员的初始装备
    /// </summary>
    private void ApplyPartyInitialEquipment() {
        foreach (var member in partyMembers) {
            member.ApplyInitialEquipment();
        }
    }

    #region 事件监听
    public void OnEvent(GameModeChangedEvent evt) {
        if (evt.NewGameMode == GameMode.Battle) {
            if (fieldActorsHidden) {
                Debug.Log("跟随者已经被隐藏，无需再次隐藏");
                return;
            }
            Debug.Log("隐藏跟随者以及玩家角色");
            fieldController.SetPlayerActive(false);
            fieldController.ClearFollower();
            fieldActorsHidden = true;
            return;
        }

        if (evt.NewGameMode == GameMode.Explore) {
            if (!fieldActorsHidden) {
                return;
            }

            fieldController.SetPlayerActive(true);
            RefreshFieldFollowers();
            fieldActorsHidden = false;
            return;
        }

    }
    #endregion
}
