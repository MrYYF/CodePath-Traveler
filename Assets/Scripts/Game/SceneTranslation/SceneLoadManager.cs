using UnityEngine.AddressableAssets;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    [SerializeField] private AssetReference menuScene;
    [SerializeField] private bool loadMenuSceneOnStart = true;

    public AssetReference activeScene;

}
