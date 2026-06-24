public class SceneSpawnPoint : MonoBehaviour
{
    [Header("Spawn Config")]
    [SerializeField] private string spawnId;
    [SerializeField] private bool isDefaultFallBack;

    public string SpawnId => spawnId;
    public bool IsDefaultFallBack => isDefaultFallBack;
}
