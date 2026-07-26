using UnityEngine.UI;

public class MenuSceneController : MonoBehaviour {
    [Header("Start Menu")]
    [SerializeField] private GameObject gameStartMenuPanel;
    [SerializeField] private float panelFadeInDuration = 0.5f;

    [Header("Buttons")]
    [SerializeField] private Button newGameButton;

    [Header("Transition Fade")]
    [SerializeField] private FadeStyle startGameFadeStyle = FadeStyle.PanelFade;
    [SerializeField, Range(-1f, 3f)] private float startGameFadeOutOverride;
    [SerializeField, Range(-1f, 3f)] private float startGameFadeInOverride;

    private CanvasGroup _startMenuCanvasGroup;
    private Coroutine _panelFadeRoutine;
    private bool _startMenuShown;
    private bool _startRequested;
    private void Awake() {
        _startMenuCanvasGroup = gameStartMenuPanel.GetComponent<CanvasGroup>();
        HideStartMenu();
    }
    private IEnumerator Start() {
        yield return null;
        ShowStartMenu();
    }

    private void OnEnable() {
        newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        _startRequested = false;
    }
    private void OnDisable() {
        newGameButton.onClick.RemoveListener(OnNewGameButtonClicked);
        StopPanelFadeRoutine();
    }

    public void OnNewGameButtonClicked() {
        if (_startRequested)
            return;

        SceneLoadManager sceneLoadManager = SceneLoadManager.Instance;
        sceneLoadManager.RequestLoad(new SceneLoadRequest(
            sceneLoadManager.StartupGameplayScene,
            startGameFadeStyle,
            GameMode.Explore,
            null,
            startGameFadeOutOverride,
            startGameFadeInOverride
            ));

    }

    /// <summary>
    /// 开启主菜单
    /// </summary>
    private void ShowStartMenu() {
        if (_startMenuShown) return;
        _startMenuShown = true;

        gameStartMenuPanel.SetActive(true);
        newGameButton.Select();
        StopPanelFadeRoutine();
        _panelFadeRoutine = StartCoroutine(FadeStartMenu());
    }

    /// <summary>
    /// 渐变开始菜单
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeStartMenu() {
        _startMenuCanvasGroup.alpha = 0;
        _startMenuCanvasGroup.interactable = false;

        // 渐变显示
        float elpase = 0;
        while (elpase < panelFadeInDuration) {
            elpase += Time.deltaTime;
            _startMenuCanvasGroup.alpha = Mathf.Clamp01(elpase / panelFadeInDuration);
            yield return null;
        }

        _startMenuCanvasGroup.alpha = 1;
        _startMenuCanvasGroup.interactable = true;
        _panelFadeRoutine = null;
    }

    private void HideStartMenu() {
        gameStartMenuPanel.SetActive(false);
        _startMenuCanvasGroup.alpha = 0;
        _startMenuCanvasGroup.interactable = false;
    }

    private void StopPanelFadeRoutine() {
        if (_panelFadeRoutine == null) {
            return;
        }

        StopCoroutine(_panelFadeRoutine);
        _panelFadeRoutine = null;
    }
}
