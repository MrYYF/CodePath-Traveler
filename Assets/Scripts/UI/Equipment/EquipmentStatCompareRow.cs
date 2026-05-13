using TMPro;

public class EquipmentStatCompareRow : MonoBehaviour {
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private TMP_Text modifyValueText;

    [Header("Color Settings")]
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;


    public void SetRow(int currentValue, int previewValue, bool isInPreviewMode) {
        currentValueText.text = currentValue.ToString();

        if(!isInPreviewMode || previewValue == currentValue) {
            modifyValueText.text = "";
            return;
        }

        modifyValueText.text = " > " + previewValue.ToString();

        if(previewValue > currentValue) {
            modifyValueText.color = positiveColor;
        } else if (previewValue < currentValue) {
            modifyValueText.color = negativeColor;
        } else {
            modifyValueText.color = Color.white; // No change
        }
    }
}
