using UnityEngine.UI;

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

    private void Awake() {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Setup(BattleEntity entity) {
        _canvasGroup.alpha = 1f;
        portraitImage.sprite = entity.Definition.Portrait;
        borderImage.sprite = entity.IsPlayer ? allyFrame : enemyFrame;
    }
}
