using UnityEngine.AddressableAssets;

public class SceneLoadManager : Singleton<SceneLoadManager> {
    [SerializeField] private AssetReference menuScene;
    //[SerializeField] private bool loadMenuSceneOnStart = true;

    public AssetReference activeScene;

    private bool isLoading;

    public void RequestLoad(SceneLoadRequest request) {
        if (isLoading) {
            return;
        }

        isLoading = true;

        StartCoroutine(LoadFlow(request));
    }

    private IEnumerator LoadFlow(SceneLoadRequest request) {
        yield return null;
    }
}
