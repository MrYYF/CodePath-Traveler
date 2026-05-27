

using UnityEngine.AddressableAssets;

/// <summary>
/// 场景加载请求，包含场景加载所需的各种参数。
/// </summary>
public struct SceneLoadRequest {
    public readonly AssetReference Scene;
    public readonly FadeStyle FadeStyle;
    public readonly GameMode ModeAfterLoad;
    public readonly string SpawnPointId;
    public readonly float FadeOutDurationOverride;
    public readonly float FadeInDurationOverride;

    public SceneLoadRequest(AssetReference scene, FadeStyle fadeStyle, GameMode modeAfterLoad,
        string spawnPointId = null,
        float fadeOutDurationOverride = -1,
        float fadeInDurationOverride = -1) {
        this.Scene = scene;
        this.FadeStyle = fadeStyle;
        this.ModeAfterLoad = modeAfterLoad;
        this.SpawnPointId = spawnPointId;
        this.FadeOutDurationOverride = fadeOutDurationOverride;
        this.FadeInDurationOverride = fadeInDurationOverride;
    }

}
