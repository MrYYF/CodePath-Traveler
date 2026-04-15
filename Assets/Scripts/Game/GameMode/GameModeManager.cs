

public class GameModeManager : Singleton<GameModeManager> {
    public GameMode currentGameMode;
    [SerializeField] private GameMode defaultGamemode = GameMode.Explore;
        
    protected override void Awake() {
        base.Awake();
        currentGameMode = defaultGamemode;
    }

    private void Start() {
        EventBus.Publish(new GameModeChangedEvent(currentGameMode));
    }

}
