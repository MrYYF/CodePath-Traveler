

using DG.Tweening;
using Framework.Event;
using TMPro;

public class BattleNotificationUI : MonoBehaviour,
    IEventReceiver<BattleNotificationEvent>,
    IEventReceiver<SkillNameDisplayEvent> {
    #region 通知条
    [Header("UI 引用")]
    [SerializeField] private GameObject notificationRoot;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private CanvasGroup notificationCanvasGroup;

    [Header("动画参数")]
    [SerializeField, Min(0f)] private float fadeInDuration = 0.15f;
    [SerializeField, Min(0f)] private float displayDuration = 1.5f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.3f;

    [Header("颜色配置")]
    [SerializeField] private Color successColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color failureColor = new Color(0.7f, 0.7f, 0.7f);

    #endregion

    #region 技能名提示
    [SerializeField] private GameObject skillNameRoot;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private CanvasGroup skillNameCanvasGroup;

    [SerializeField, Min(0f)] private float skillNameFadeInDuration = 0.12f;
    [SerializeField, Min(0f)] private float skillNameDisplayDuration = 1.2f;
    [SerializeField, Min(0f)] private float skillNameFadeOutDuration = 0.18f;
    #endregion

    #region 运行状态
    private Tween _notificationTween;
    private Tween _skillNameTween;
    #endregion

    #region 生命周期函数
    private void Awake() {
        notificationRoot.gameObject.SetActive(false);
        skillNameRoot.gameObject.SetActive(false);
        notificationCanvasGroup.alpha = 0;
        skillNameCanvasGroup.alpha = 0;
    }
    private void OnEnable() {
        EventBus.Subscribe<BattleNotificationEvent>(this);
        EventBus.Subscribe<SkillNameDisplayEvent>(this);
    }
    private void OnDisable() {
        EventBus.Unsubscribe<BattleNotificationEvent>(this);
        EventBus.Unsubscribe<SkillNameDisplayEvent>(this);

        _notificationTween?.Kill();
        _skillNameTween?.Kill();

        notificationRoot.gameObject.SetActive(false);
        skillNameRoot.gameObject.SetActive(false);
        notificationCanvasGroup.alpha = 0;
        skillNameCanvasGroup.alpha = 0;
    }
    #endregion

    #region 事件相关
    public void OnEvent(BattleNotificationEvent evt) {
        notificationText.text = evt.Message;
        notificationText.color = evt.IsSuccess ? successColor : failureColor;
        notificationText.alpha = 1f;

        _notificationTween?.Kill();
        _notificationTween = PlayFadeSequence(notificationRoot, notificationCanvasGroup,
            fadeInDuration, displayDuration, fadeOutDuration,
            () => {
                notificationRoot.SetActive(false);
                _notificationTween = null;
            });
    }

    public void OnEvent(SkillNameDisplayEvent evt) {
        skillNameText.text = evt.SkillName;
        skillNameText.alpha = 1f;

        _skillNameTween?.Kill();
        _skillNameTween = PlayFadeSequence(skillNameRoot, skillNameCanvasGroup,
            skillNameFadeInDuration, skillNameDisplayDuration, skillNameFadeOutDuration,
            () => {
                skillNameRoot.SetActive(false);
                _skillNameTween = null;
            });
    }
    #endregion

    /// <summary>
    /// 播放淡入淡出队列
    /// </summary>
    /// <param name="root"></param>
    /// <param name="targetCanvasGroup"></param>
    /// <param name="fadeInDuration"></param>
    /// <param name="displayDuration"></param>
    /// <param name="fadeOutDuration"></param>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    private Tween PlayFadeSequence(GameObject root, CanvasGroup targetCanvasGroup,
        float fadeInDuration, float displayDuration, float fadeOutDuration,
        TweenCallback onFinished) {

        root.SetActive(true);
        targetCanvasGroup.alpha = 0f;


        return DOTween.Sequence()
            .SetUpdate(true)
            .Append(targetCanvasGroup.DOFade(1f, fadeInDuration))
            .AppendInterval(displayDuration)
            .Append(targetCanvasGroup.DOFade(0f, fadeOutDuration))
            .OnComplete(onFinished);
    }

}
