using Framework.Event;

public class CameraModeController : MonoBehaviour, IEventReceiver<GameModeChangedEvent> {

    [Header("Cameras")]
    [SerializeField] private GameObject followCamera;
    [SerializeField] private GameObject battleCamera;

    private void OnEnable() {
        EventBus.Subscribe<GameModeChangedEvent>(this);
    }

    private void OnDisable() {
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
    }

    public void OnEvent(GameModeChangedEvent evt) {
        switch (evt.NewGameMode) {
            case GameMode.Explore:
                SetCameraView(CameraView.Explore);
                break;
            case GameMode.Battle:
                SetCameraView(CameraView.Battle);
                break;
            
        }
    }

    private void SetCameraView(CameraView view) {
        bool followCameraActive = false;
        bool battleCameraActive = false;

        switch (view) {
            case CameraView.Explore:
                followCameraActive = true;
                break;
            case CameraView.Battle:
                battleCameraActive = true;
                break;
            case CameraView.BattleResult:
                // Implement battle result camera logic if needed
                break;
        }

        followCamera.SetActive(followCameraActive);
        battleCamera.SetActive(battleCameraActive);
    }
}
