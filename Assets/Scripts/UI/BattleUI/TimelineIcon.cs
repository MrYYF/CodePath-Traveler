using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 时间轴
/// </summary>
public class TimelineIcon : MonoBehaviour {
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Sprite allyFrame;
    [SerializeField] private Sprite enemyFrame;

    private CanvasGroup _canvasGroup;
    private Vector3 _visualInitPos;

    private void Awake() {
        _canvasGroup = GetComponent<CanvasGroup>();
        _visualInitPos = visualRoot.localPosition;
        _canvasGroup.alpha = 1f;
    }

    public void Setup(BattleEntity entity) {
        portraitImage.sprite = entity.Definition.Portrait;
        portraitImage.SetNativeSize();
        borderImage.sprite = entity.IsPlayer ? allyFrame : enemyFrame;
    }

    #region 动画相关
    /// <summary>
    /// 播放入场动画
    /// </summary>
    /// <param name="duration">持续时间</param>
    /// <param name="offsetX">起始偏移量</param>
    /// <param name="ease">淡入淡出方式</param>
    public void PlayEntranceAnimation(float duration, float offsetX, Ease ease) {
        StopVisualTweens();
        _canvasGroup.alpha = 1f;
        visualRoot.localPosition = _visualInitPos + new Vector3(offsetX, 0, 0);
        visualRoot.DOLocalMoveX(_visualInitPos.x, duration).SetEase(ease);
    }

    /// <summary>
    /// 播放退场动画
    /// </summary>
    /// <param name="duration">持续时间</param>
    /// <param name="offsetX">起始偏移量</param>
    /// <param name="ease">淡入淡出方式</param>
    public void PlayExitAnimation(float duration, float offsetX, Ease ease) {
        StopVisualTweens();
        visualRoot.DOLocalMove(_visualInitPos + new Vector3(offsetX, 0, 0), duration).SetEase(ease);
        _canvasGroup.DOFade(0f, duration).SetEase(ease);
    }

    private void StopVisualTweens() {
        visualRoot.DOKill();
        _canvasGroup.DOKill();
    }
    #endregion
}
