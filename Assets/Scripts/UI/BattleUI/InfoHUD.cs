using TMPro;
using UnityEngine.UI;

public class InfoHUD : MonoBehaviour
{
    #region 信息栏组件
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private Slider expSlider;

    #endregion

    public void SetInfo(string displayName,int level,int currentExp,int targetExp,float expProgress01,Sprite protrait) {
        int shownTargetExp = targetExp > 0 ? targetExp : 1;
        if(protrait != null) {
            characterImage.sprite = protrait;
        }
        nameText.text = displayName;
        levelText.text = "lv." + level.ToString();
        expText.text = $"{currentExp}/{shownTargetExp}";
        expSlider.minValue = 0;
        expSlider.maxValue = 1;
        expSlider.value = Mathf.Clamp01(expProgress01);
    }
}
