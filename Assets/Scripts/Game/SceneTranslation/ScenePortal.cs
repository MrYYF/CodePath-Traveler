

using Unity.VisualScripting;
using UnityEngine.AddressableAssets;

public class ScenePortal : MonoBehaviour {
    [Header("Target Scene")]
    [SerializeField] private AssetReference targetScene;
    [SerializeField] private string targetSpawnPointId;

    [Header("Trigger Mode")]
    [SerializeField] private bool requireConfirmKey = true;
    [SerializeField, Min(0)] private float triggerCooldown = 1f;

    [Header("Transition")]
    [SerializeField] private FadeStyle fadeStyle = FadeStyle.WipeMask;

    // 玩家是否处于触发区域内
    private bool _playerInside;
    // 下一次允许触发的时间戳
    private float _nextAllowedTriggerTime;

    private void Update() {
        if (!_playerInside || !requireConfirmKey)
            return;

        InputSystemController input = InputSystemController.Instance;
        if (input.GetPlayerConfirmPressed()) {
            RequestTeleport();
        }
    }

    private void OnTriggerEnter(Collider other) {
        _playerInside = true;
        if (!requireConfirmKey)
            RequestTeleport();
    }
    private void OnTriggerExit(Collider other) {
        _playerInside = false;
    }

    private void RequestTeleport() {
        // 传送冷却
        if (Time.time < _nextAllowedTriggerTime) 
            return;
        

        SceneLoadManager sceneLoadManager = SceneLoadManager.Instance;

        // 正在传送
        if(sceneLoadManager.IsLoading)
            return;

        if (targetScene.IsUnityNull())
            return;

        Debug.Log(targetScene);

        // 构建场景切换请求并请求切换
        sceneLoadManager.RequestLoad(new SceneLoadRequest(
            targetScene,
            fadeStyle,
            GameMode.Explore,
            targetSpawnPointId
            ));

        // 请求发出后，关闭本次触发，计算下一次冷却时间
        _playerInside = false;
        _nextAllowedTriggerTime = Time.time + triggerCooldown;
    }

    

}
