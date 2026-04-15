

public class GameModeManager : Singleton<GameModeManager> {
    public GameMode CurrentGameMode;
    [SerializeField] private GameMode defaultGamemode = GameMode.Explore;

    protected override void Awake() {
        base.Awake();
        CurrentGameMode = defaultGamemode;
    }

    private void Start() {
        ApplyMode(CurrentGameMode);
    }

    private void ApplyMode(GameMode newMode) {
        CurrentGameMode = newMode;
        EventBus.Publish(new GameModeChangedEvent(CurrentGameMode));
    }

    #region 外部调用入口
    public void RequestChangeGameMode(GameMode newMode) {
        if (Inastance != this) return;
        if (!CanSwitchMode(newMode)) return;

        ApplyMode(newMode);
    }

    public bool CanSwitchMode(GameMode newMode) {
        if (CurrentGameMode == GameMode.Battle)
            return false;

        return true;
    }
    #endregion
}
