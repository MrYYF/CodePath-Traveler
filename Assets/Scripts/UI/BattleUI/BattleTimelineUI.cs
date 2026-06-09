
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 控制战斗CTB时间轴UI层的类
/// </summary>
public class BattleTimelineUI : MonoBehaviour
{
    [SerializeField] private TimelineIcon timelineIconPrefab;
    [Header("Containers")]
    [SerializeField] private RectTransform currentRoundContainer;
    [SerializeField] private RectTransform nextRoundContainer;
    [Header("Active Unit Display")]
    [SerializeField] private Image activeUnitPortrait;
    [SerializeField] private TMP_Text activeUnitName;




}
