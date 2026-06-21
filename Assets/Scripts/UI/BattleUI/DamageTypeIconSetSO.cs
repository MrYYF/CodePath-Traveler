
using System;

[CreateAssetMenu(menuName = "Battle/DamageTypeIconSet")]
public class DamageTypeIconSetSO : ScriptableObject {
    [SerializeField] private DamageTypeIconEntry[] entries;

    private readonly Dictionary<DamageType, Sprite> _iconCache = new();

    public Sprite GetIcon(DamageType type) {
        return _iconCache[type];
    }

    private void OnValidate() {
        _iconCache.Clear();
        foreach (var entry in entries) {
            _iconCache[entry.damageType] = entry.icon;

        }
    }
}

[Serializable]
public struct DamageTypeIconEntry {
    #region 图标条目结构
    public DamageType damageType;
    public Sprite icon;
    #endregion
}
