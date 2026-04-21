

using System;

[CreateAssetMenu(menuName = "Inventory/ItemIconSet")]
public class ItemIconSetSO : ScriptableObject
{
    public ItemIconEntry[] itemIconEntries;

    public Sprite GetIconForItem(ItemIconKey itemIconKey) {
        foreach (var entry in itemIconEntries) {
            if (entry.itemIconKey == itemIconKey) {
                return entry.icon;
            }
        }
        Debug.LogWarning($"No icon found for ItemIconKey: {itemIconKey}");
        return null; // Return null or a default icon if not found
    }
}

[Serializable]
public class ItemIconEntry {
    public ItemIconKey itemIconKey;
    public Sprite icon;
}