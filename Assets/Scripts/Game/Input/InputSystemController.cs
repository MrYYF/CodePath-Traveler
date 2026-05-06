using UnityEngine.InputSystem;
using Framework.Event;

public class InputSystemController : Singleton<InputSystemController>,IEventReceiver<GameModeChangedEvent>
{
    private CharacterInputActions _inputActions;
    public CharacterInputActions InputActions => _inputActions;
    private bool _isInitialized = false;
    private ActiveInputActionMap _currentActionMap = ActiveInputActionMap.Player;

    #region 生命周期
    protected override void Awake() {
        base.Awake();
        if(!_isInitialized) {
            _inputActions ??= new CharacterInputActions();
            _isInitialized = true;
        }
    }

    private void OnEnable() {
        EventBus.Subscribe<GameModeChangedEvent>(this);
    }

    private void OnDisable() {
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
    }

    private void OnDestroy() {
        _inputActions.Dispose();
    }
    #endregion

    public Vector2 GetMovementInput() {
        _inputActions.Player.Enable(); // 确保玩家输入ActionMap已启用
        if (!_isInitialized ||_currentActionMap != ActiveInputActionMap.Player) {
            return Vector2.zero; // 如果当前不是玩家输入模式，返回零向量
        }
        return _inputActions.Player.Move.ReadValue<Vector2>();
    }

    public bool GetPlayerConfirmPressed() {
        if (!_isInitialized || _currentActionMap != ActiveInputActionMap.Player) {
            return false; // 如果当前不是玩家输入模式，返回false
        }
        return _inputActions.Player.Confirm.WasPressedThisFrame();
    }

    public bool GetUICancelPressed() {
        if (!_isInitialized || _currentActionMap != ActiveInputActionMap.UI) {
            return false; // 如果当前不是玩家输入模式，返回false
        }
        return _inputActions.UI.Cancel.WasPressedThisFrame();
    }

    public bool GetMenuPressed() {
        if (!_isInitialized) {
            return false; // 如果当前不是玩家输入模式，返回false
        }
        return _inputActions.UI.Menu.WasPressedThisFrame() || _inputActions.Player.Menu.WasPressedThisFrame();
    }

    #region 事件系统
    // 监听游戏模式切换事件，根据当前游戏模式切换输入ActionMap
    public void OnEvent(GameModeChangedEvent evt) {
        Debug.Log($"GameMode changed to {evt.NewGameMode.ToString()}");
        _currentActionMap = GetActionMapForGameMode(evt.NewGameMode);
        _inputActions.Disable(); // 先禁用所有输入
        switch (_currentActionMap) {
            case ActiveInputActionMap.Player:
                _inputActions.Player.Enable();
                break;
            case ActiveInputActionMap.UI:
                _inputActions.UI.Enable();
                break;
            default:
                break;
        }
    }

    private ActiveInputActionMap GetActionMapForGameMode(GameMode gameMode) {
        switch (gameMode) {
            case GameMode.Explore:
                return ActiveInputActionMap.Player;
            case GameMode.InteractionMenu:
            case GameMode.Battle:
            case GameMode.Pause:
            default:
                return ActiveInputActionMap.UI;

        }
    }
    #endregion
}
