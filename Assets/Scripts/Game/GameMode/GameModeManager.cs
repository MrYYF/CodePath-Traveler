

public class GameModeManager : Singleton<GameModeManager> {
    public GameMode CurrentGameMode;
    [SerializeField] private GameMode defaultGamemode = GameMode.Explore;

    #region 生命周期
    protected override void Awake() {
        base.Awake();
        CurrentGameMode = defaultGamemode;
        Application.targetFrameRate = 60;
    }

    private void Start() {
        ApplyMode(CurrentGameMode);
    }
    #endregion

    private void ApplyMode(GameMode newMode) {
        CurrentGameMode = newMode;
        EventBus.Publish(new GameModeChangedEvent(CurrentGameMode));
    }

    #region 外部调用入口
    public void RequestChangeGameMode(GameMode newMode) {
        if (Instance != this) return;
        if (!CanSwitchMode(newMode)) return;

        ApplyMode(newMode);
    }

    public bool CanSwitchMode(GameMode newMode) {
        if (CurrentGameMode != GameMode.Battle || newMode != GameMode.Explore)
            return true;

        return false;
    }
    #endregion
}
