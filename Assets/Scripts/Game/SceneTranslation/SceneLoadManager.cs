using Unity.Cinemachine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;


/// <summary>
/// 场景加载管理器，负责处理游戏中的场景切换流程，包括淡入淡出效果、资源管理和游戏模式切换等。
/// </summary>
public class SceneLoadManager : Singleton<SceneLoadManager> {
    [Header("场景引用")]
    public AssetReference activeScene; // 当前活动场景的地址引用
    [SerializeField] private AssetReference menuScene;
    public AssetReference MenuScene => menuScene; // 主菜单场景
    [SerializeField] private AssetReference _startupGameplayScene; // 进入游戏时的场景
    public AssetReference StartupGameplayScene => _startupGameplayScene;
    [SerializeField] private bool loadMenuSceneOnStart = true; // 开始时是否加载菜单场景

    [Header("场景转换参数")]
    // 场景加载完成后黑屏持续时间
    [SerializeField, Range(0f, 2f)] private float postLoadBlackScreenDuration = 0.35f;

    // 是否正处于场景加载过程中
    private bool isLoading;
    public bool IsLoading => isLoading;
    // 当前加载的场景句柄，用于卸载和资源管理
    private AsyncOperationHandle<SceneInstance>? _currentSceneHandle;
    


    protected override void Awake() {
        base.Awake();
        // 确定第一个加载的场景
        _startupGameplayScene = activeScene;
        var firstScene = loadMenuSceneOnStart ? menuScene : activeScene;

        // 异步加载场景
        var loadHandle = Addressables.LoadSceneAsync(firstScene, LoadSceneMode.Additive);
        _currentSceneHandle = loadHandle;

        // 设置加载完成回调，确保在场景加载完成后将其设置为活动场景
        loadHandle.Completed += (handle) => {
            if (handle.Status != AsyncOperationStatus.Succeeded) 
                return;
            SceneManager.SetActiveScene(handle.Result.Scene);
            if (loadMenuSceneOnStart) {
                GameModeManager.Instance.RequestChangeGameMode(GameMode.InteractionMenu);
            }
        };
    }

    /// <summary>
    /// 从场景加载请求开始场景切换流程
    /// </summary>
    /// <param name="request">场景加载请求</param>
    public void RequestLoad(SceneLoadRequest request) {
        if (isLoading) {
            return;
        }

        isLoading = true;

        StartCoroutine(LoadFlow(request));
    }

    /// <summary>
    /// 加载场景加载请求流程协程
    /// </summary>
    /// <param name="request">场景加载请求</param>
    private IEnumerator LoadFlow(SceneLoadRequest request) {
        try {
            // 切换游戏模式，禁用玩家输入
            GameModeManager.Instance.RequestChangeGameMode(GameMode.InteractionMenu);

            // 配置淡入淡出效果
            FadeController.Instance.SetStyle(request.FadeStyle);
            FadeController.Instance.SetNextFadeDurations(
                request.FadeOutDurationOverride,
                request.FadeInDurationOverride);

            // 开始淡出
            bool fadeOutCompleted = false;
            FadeController.Instance.FadeOut(() => fadeOutCompleted = true);
            yield return new WaitUntil(() => fadeOutCompleted);

            // 卸载当前场景，释放资源
            if (_currentSceneHandle.HasValue && _currentSceneHandle.Value.IsValid()) {
                yield return Addressables.UnloadSceneAsync(_currentSceneHandle.Value, true);
                _currentSceneHandle = null;
            }

            // 加载新场景
            var loadHandle = Addressables.LoadSceneAsync(request.Scene, LoadSceneMode.Additive, true);
            yield return loadHandle;
            bool loadSuccessed = loadHandle.Status == AsyncOperationStatus.Succeeded;
            // 加载失败时记录错误并退出流程
            if (!loadSuccessed) {
                Debug.LogError($"Failed to load scene: {request.Scene}");
                yield break;
            }
            _currentSceneHandle = loadHandle;
            activeScene = request.Scene;
            SceneManager.SetActiveScene(loadHandle.Result.Scene);
            // 定位出生点，广播场景加载完成事件
            EventBus.Publish(new SceneLoadCompleteEvent(loadHandle.Result.Scene, request.ModeAfterLoad));

            // 如果是回到探索场景
            bool restoreExploreModeBeforeFadeIn = loadSuccessed && request.ModeAfterLoad == GameMode.Explore;
            if (restoreExploreModeBeforeFadeIn) {
                GameModeManager.Instance.RequestChangeGameMode(GameMode.Explore);
                yield return null;

                ApplySpawnPointAfterLoad(request, loadHandle.Result.Scene);
            }

            // 场景加载完成后保持黑屏一段时间，确保资源完全就绪
            yield return new WaitForSeconds(postLoadBlackScreenDuration);

            // 开始淡入
            bool fadeInCompleted = false;
            FadeController.Instance.FadeIn(() => fadeInCompleted = true);
            yield return new WaitUntil(() => fadeInCompleted);

            // 如果之前没有提前切换回探索模式，则在淡入完成后切换到目标游戏模式
            if (loadSuccessed && !restoreExploreModeBeforeFadeIn) {
                GameModeManager.Instance.RequestChangeGameMode(request.ModeAfterLoad);
            }
        }
        finally {
            isLoading = false;
        }
        yield return null;
    }

    #region 传送
    private void ApplySpawnPointAfterLoad(SceneLoadRequest request, Scene loadedScene) {
        SceneSpawnPoint spawnPoint = FindSpawnPoint(loadedScene, request.SpawnPointId);
        PlayerController player = FindObjectOfType<PlayerController>();

        // 获取玩家位置并计算偏移
        Vector3 delta = spawnPoint.transform.position - player.transform.position;

        // 移动玩家以及队列
        PartyManager.Instance.TeleportPartyTo(spawnPoint.transform.position, spawnPoint.transform.rotation);

        // 切换相机位置
        SnapMainCamera(delta);
    }

    /// <summary>
    /// 寻找目标场景中符合需求的传送点
    /// </summary>
    /// <param name="scene">目标场景</param>
    /// <param name="targetSpawnId">目标传送点id</param>
    /// <returns>传送点信息</returns>
    private SceneSpawnPoint FindSpawnPoint(Scene scene, string targetSpawnId) {
        SceneSpawnPoint fallback = null;
        SceneSpawnPoint first = null;

        GameObject[] rootsObj = scene.GetRootGameObjects();
        foreach (var root in rootsObj) {
            SceneSpawnPoint sceneSpawnPoint = root.GetComponent<SceneSpawnPoint>();
            if (sceneSpawnPoint == null) continue;

            if (sceneSpawnPoint.SpawnId == targetSpawnId)
                return sceneSpawnPoint;

            // 未找到匹配的id则记录第一个传送点位
            if (first == null)
                first = sceneSpawnPoint;

            // 存储带有默认标记的传送点位
            if (fallback == null && sceneSpawnPoint.IsDefaultFallBack)
                fallback = sceneSpawnPoint;
        }

        return fallback ?? first;
    }

    private void SnapMainCamera(Vector3 delta) {
        Camera mainCamera = Camera.main;

        // 移动主相机
        mainCamera.transform.position += delta;

        // 遍历调整虚拟相机
        foreach (var vcam in FindObjectsOfType<CinemachineCamera>()) {
            // 将上一帧状态标记为无效，防止慢慢过渡
            vcam.PreviousStateIsValid = false;
            // 通知位移量，对齐跟随目标
            vcam.OnTargetObjectWarped(vcam.Follow, delta);
        }

        // 重启一下brain，清除可能的残留
        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        brain.enabled = false;
        brain.enabled = true;
    }
    #endregion
}
