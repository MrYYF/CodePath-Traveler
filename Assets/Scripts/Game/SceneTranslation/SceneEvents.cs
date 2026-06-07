using Framework.Event;
using UnityEngine.SceneManagement;

public readonly struct SceneLoadCompleteEvent : IEvent {
    public readonly Scene LoadedScene;
    public readonly GameMode ModeAfterLoad;

    public SceneLoadCompleteEvent(Scene loadedScene, GameMode modeAfterLoad) {
        LoadedScene = loadedScene;
        ModeAfterLoad = modeAfterLoad;
    }

}
