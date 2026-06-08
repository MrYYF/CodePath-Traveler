

/// <summary>
/// 探索模式下的队伍跟随系统，负责管理跟随者的生成、位置更新和动画状态
/// </summary>
public class PartyFieldController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform followersParent;
    [SerializeField] private GameObject fieldFollowerPrefab;
    [SerializeField] private Transform playerTrans;

    [Header("Settings")]
    [SerializeField] private float followDistance = 1.2f; // 相邻两人之间的间距
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float zOffset = 0.01f;
    [SerializeField] private float sampleMinDistance = 0.05f;

    private List<Vector3> trail = new(); //移动轨迹
    private List<FieldFollower> fieldFollowers = new(); //跟随者


    private void LateUpdate() {
        UpdateLeaderTrail();

        for (int i = 0; i < fieldFollowers.Count; i++) {
            var follower = fieldFollowers[i];
            float targetDistance = followDistance * (i + 1);
            Vector3 targetPos = GetPointAtDistance(targetDistance);

            follower.MoveTo(ApplyFollowerOffset(targetPos, i), followSpeed);
        }
    }

    /// <summary>
    /// 更新跟随者列表
    /// </summary>
    /// <param name="partyMembers"></param>
    public void UpdateFollowers(List<CharacterDefinitionSO> partyMembers) {
        int followerCount = partyMembers.Count - 1;
        while (fieldFollowers.Count < followerCount) {
            int index = fieldFollowers.Count;
            var pos = ApplyFollowerOffset(playerTrans.position, index);

            GameObject followerObj = Instantiate(fieldFollowerPrefab, pos, Quaternion.identity, followersParent);
            fieldFollowers.Add(followerObj.GetComponent<FieldFollower>());
        }

        for (int i = 0; i < followerCount; i++) {
            fieldFollowers[i].SetupFollower(partyMembers[i + 1]);
        }

        RebuildTrailsAndSnapFollowers();
    }

    /// <summary>
    /// 应用跟随者的z轴偏移
    /// </summary>
    /// <param name="position"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private Vector3 ApplyFollowerOffset(Vector3 position, int index) {
        position.z += zOffset * (index + 1);
        return position;
    }

    /// <summary>
    /// 更新领队轨迹
    /// </summary>
    private void UpdateLeaderTrail() {
        Vector3 leaderPos = playerTrans.position;

        if (trail.Count == 0) {
            trail.Add(leaderPos);
            return;
        }

        float dist = Vector3.Distance(playerTrans.position, trail[0]);

        if (dist > sampleMinDistance) {
            trail.Insert(0, leaderPos);

            if (trail.Count > 100) {
                trail.RemoveAt(trail.Count - 1);
            }
        }

    }

    /// <summary>
    /// 获取路径上距离领导者指定距离的点
    /// </summary>
    /// <param name="distanceFromLeader"></param>
    /// <returns></returns>
    private Vector3 GetPointAtDistance(float distanceFromLeader) {
        if (trail.Count == 0) return playerTrans.position;

        float accumulated = 0f; //累计距离

        for (int i = 0; i < trail.Count - 1; i++) {
            Vector3 a = trail[i];
            Vector3 b = trail[i + 1];

            float dist = Vector3.Distance(a, b);

            if (accumulated + dist >= distanceFromLeader) {
                float t = (distanceFromLeader - accumulated) / dist;
                return Vector3.Lerp(a, b, t);
            }

            accumulated += dist;
        }

        // 如果所有轨迹线段长度相加之和仍然小于目标距离，返回路径最后一个点
        return trail[^1];
    }

    /// <summary>
    /// 重建轨迹队列并且将所有跟随者的位置刷新到领队处
    /// </summary>
    private void RebuildTrailsAndSnapFollowers() {
        trail.Clear();
        for (int i = 0; i < fieldFollowers.Count; i++) {
            fieldFollowers[i].SnapTo(ApplyFollowerOffset(playerTrans.position, i));
        }

        UpdateLeaderTrail();
    }

    /// <summary>
    /// 设置玩家角色的显示状态，通常在切换场景或进入战斗时调用
    /// </summary>
    /// <param name="active"></param>
    public void SetPlayerActive(bool active) {
        playerTrans.gameObject.SetActive(active);
    }

    /// <summary>
    /// 清除所有跟随者并重置轨迹
    /// </summary>
    public void ClearFollower() {
        foreach (FieldFollower follower in fieldFollowers) {
            if (follower != null) {
                Destroy(follower.gameObject);
            }
            
        }
        fieldFollowers.Clear();
        trail.Clear();
    }
}
