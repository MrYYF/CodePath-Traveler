/// <summary>
/// 敌方死亡销毁与消散特效
/// </summary>
public class BattleUnitDissolveFX : MonoBehaviour {
    [Header("死亡特效")]
    [SerializeField, Tooltip("死亡特效预制体")]
    private ParticleSystem deathVfxPrefab;
    [SerializeField, Tooltip("死亡特效的世界坐标偏移")]
    private Vector3 deathVfxOffset = Vector3.zero;
    [SerializeField, Tooltip("死亡特效对象多久后清除")]
    private float deathVfxDestroyDelay = 3f;

    private BattleUnit _battleUnit;
    private bool _deathVfxPlayed;

    private void Awake() {
        _battleUnit = GetComponent<BattleUnit>();
    }

    /// <summary>
    /// 延迟后播放死亡特效
    /// </summary>
    /// <param name="delay">消散延迟</param>
    public void PlayDelayVfx(float delay = 0f) {
        if (_deathVfxPlayed) {
            return;
        }

        _deathVfxPlayed = true;

        if (delay < 0f) {
            HideBodyThenSpawnVfx();
        }

        StartCoroutine(CoPlayedDeathVfx(delay));
    }

    /// <summary>
    /// 播放死亡特效
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator CoPlayedDeathVfx(float delay) {
        // 延迟阶段
        float elapsed = 0f;
        while (elapsed < delay) {
            float deltaTime = Time.timeScale <= 0 ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += deltaTime;
            yield return null;
        }

        // 生成死亡特效
        HideBodyThenSpawnVfx();
    }

    #region 特效生成
    /// <summary>
    /// 隐藏单位并生成死亡特效
    /// </summary>
    private void HideBodyThenSpawnVfx() {
        // 隐藏本体战斗单位
        _battleUnit.SetBodyVisible(false);

        // 生成特效
        Vector3 pos = transform.position + deathVfxOffset;
        ParticleSystem vfxInstantiate = Instantiate(deathVfxPrefab, pos, Quaternion.identity);
        Destroy(vfxInstantiate, deathVfxDestroyDelay);
    }


    #endregion
}
