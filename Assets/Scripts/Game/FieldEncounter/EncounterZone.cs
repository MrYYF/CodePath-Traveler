using System;
using UnityEngine.AddressableAssets;

/// <summary>
/// 遇敌配置
/// </summary>
[Serializable]
public struct EncounterGroup {
    [Tooltip("敌方阵容组合")]
    public List<CharacterDefinitionSO> Enemies;

    [Tooltip("阵型")]
    public EnemyLayoutFomation Fomation;

    [Tooltip("出现权重"), Min(1)] public int Weight;
}

public class EncounterZone : MonoBehaviour
{
    [Header("Battle Scene")]
    [Tooltip("战斗场景")] public AssetReference battleSceneReference;

    [Header("Encounter Settings")]
    [Tooltip("遇敌最小移动距离")] public float minEncounterDistance = 15f;
    [Tooltip("遇敌最大移动距离")] public float maxEncounterDistance = 30f;

    [Header("Enemy Pools")]
    [Tooltip("遇敌池")] public List<EncounterGroup> encounterGroups = new List<EncounterGroup>();

    /// <summary>
    /// 根据权重随机抽取一组敌人
    /// </summary>
    /// <returns>遇敌配置信息</returns>
    public EncounterGroup GetRandomEncounter() {
        // 累加所有权重
        int totalWeight = 0;
        foreach (var group in encounterGroups) {
            totalWeight += group.Weight;
        }

        // 生成随机值
        int randomvalue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;
        foreach (var group in encounterGroups) {
            currentWeight += group.Weight;
            if (randomvalue < currentWeight) {
                return group;
            }
        }

        return encounterGroups[^1];
    }
}


