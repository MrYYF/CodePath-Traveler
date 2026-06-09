

using TMPro;
using UnityEngine.UI;

/// <summary>
/// 技能按钮的UI控件
/// </summary>
public class SkillButton : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillCost;

    public void Setup(SkillDataSO skillData) {
        bool showIcon = skillData.elementType != ElementType.None;
        skillIcon.gameObject.SetActive(showIcon);
        skillIcon.sprite = skillData.icon;
        skillName.text = skillData.name;
        skillCost.text = $"SP {skillData.spCost}";
    }

}
