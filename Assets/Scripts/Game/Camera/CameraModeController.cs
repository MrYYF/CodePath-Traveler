using Framework.Event;

public class CameraModeController : MonoBehaviour,
    IEventReceiver<GameModeChangedEvent>,
    IEventReceiver<BattleEndedEvent> {

    [Header("Cameras")]
    [SerializeField] private GameObject followCamera;
    [SerializeField] private GameObject battleCamera;
    [SerializeField] private GameObject battleResultCameraRoot;

    #region 生命周期
    private void OnEnable() {
        EventBus.Subscribe<GameModeChangedEvent>(this);
        EventBus.Subscribe<BattleEndedEvent>(this);
    }

    private void OnDisable() {
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
        EventBus.Unsubscribe<BattleEndedEvent>(this);
    }
    #endregion

    #region 事件相关
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

    public void OnEvent(BattleEndedEvent evt) {
        SetCameraView(CameraView.BattleResult);
        if (!evt.IsWin) {
            //SetCameraView(CameraView.Explore);
            EventBus.Publish(new BattleLoseViewEnterEvent());
            return;
        }

        EventBus.Publish(new BattleResultViewEnterEvent(evt.ExpReward, evt.MoneyReward, evt.dropRewards));
    }
    #endregion

    private void SetCameraView(CameraView view) {

        followCamera.SetActive(view == CameraView.Explore);
        battleCamera.SetActive(view == CameraView.Battle);
        battleResultCameraRoot.SetActive(view == CameraView.BattleResult);
    }


}
