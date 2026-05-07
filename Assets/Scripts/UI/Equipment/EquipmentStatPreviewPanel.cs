

using System;

public class EquipmentStatPreviewPanel : MonoBehaviour
{
    [Serializable]
    public struct StatRowBinding {
        public StatType statType;
        public EquipmentStatCompareRow row;
    }

    [SerializeField] private StatRowBinding[] statRows;

    public void Refresh(StatBlock currentStats, StatBlock previewStats, bool isInPreviewMode = true) {
        for (int i = 0; i < statRows.Length; i++) {
            StatRowBinding binding = statRows[i];
            int current = ReadStat(currentStats, binding.statType);
            int preview = ReadStat(previewStats, binding.statType);

            binding.row.SetRow(current, preview,isInPreviewMode);
        }
    }

    private int ReadStat(StatBlock block, StatType type) {
        switch (type) {
            case StatType.MaxHP: return block.MaxHP;
            case StatType.MaxSP: return block.MaxSP;
            case StatType.PAtk: return block.PAtk;
            case StatType.PDef: return block.PDef;
            case StatType.MAtk: return block.MAtk;
            case StatType.MDef: return block.MDef;
            case StatType.Speed: return block.Speed;
            case StatType.Accuracy: return block.Accuracy;
            case StatType.Evasion: return block.Evasion;
            default: return 0;
        }
    }

}
