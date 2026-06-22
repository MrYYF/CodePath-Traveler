using System;

/// <summary>
/// 战斗电影化演出配置
/// </summary>
[CreateAssetMenu(menuName = "Battle/Cinematic Config")]
public class BattleCinematicConfigSO : ScriptableObject {
    [Header("Cinematic Toggle")]
    public bool EnableKillCinematic = true;
    public bool EnableBreakCinematic = true;
    public float KillDissolveStagger = 0.08f; //多击杀时特效间隔

    [Header("Kill Impact")]
    public BattleImpactCinematicSettings Kill = BattleImpactCinematicSettings.CreateLegacyDefault();

    [Header("Break Impact")]
    public BattleImpactCinematicSettings Break = BattleImpactCinematicSettings.CreateLegacyDefault();


}

/// <summary>
/// 单次冲击演出参数集合
/// </summary>
[Serializable]
public class BattleImpactCinematicSettings {
    [Header("Time Scale")]
    public float HitStopDuration = 0.5f; // 命中时停时长
    [Range(0.05f, 1f)] public float SloMoScale = 0.15f; // 慢动作时间缩放倍率
    public float SlowMoInDuration = 0.06f; // 进入慢动作过渡时长
    public float SlowMoOutDuration = 0.18f; // 退出慢动作过渡时长
    public float HoldDuration = 0.12f; // 没有镜头特写时慢动作额外停留时长
    [Header("Camera")]
    public float CameraTurnDuration = 0.08f; // 镜头转向目标时长
    public float CameraHoldDuration = 0.05f; // 镜头到位后停留时长
    public float CameraReturnDuration = 0.12f; // 镜头回到默认朝向时长
    public Vector3 CameraEulerOffset = Vector3.zero; // 镜头旋转偏移
    public Vector3 CameraPositionOffset = Vector3.zero; // 镜头位置偏移

    public static BattleImpactCinematicSettings CreateLegacyDefault() {
        return new BattleImpactCinematicSettings();
    }
}
