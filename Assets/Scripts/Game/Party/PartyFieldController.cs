

public class PartyFieldController : MonoBehaviour
{
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

        for(int i = 0; i < fieldFollowers.Count; i++) {
            var follower = fieldFollowers[i];
            float targetDistance = followDistance * (i + 1);
            Vector3 targetPos = GetPointAtDistance(targetDistance);

            follower.MoveTo(targetPos,followSpeed);
        }
    }

    // 更新跟随者列表
    public void UpdateFollowers(List<CharacterDefinitionSO> partyMembers) {
        int followerCount = partyMembers.Count - 1;
        while(fieldFollowers.Count <  followerCount) {
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

    // 应用跟随者的z轴偏移
    private Vector3 ApplyFollowerOffset(Vector3 position, int index) {
        position.z += zOffset * (index + 1);
        return position;
    }

    // 更新领队轨迹
    private void UpdateLeaderTrail() {
        Vector3 leaderPos = playerTrans.position;

        if (trail.Count == 0) {
            trail.Add(leaderPos);
            return;
        }

        float dist = Vector3.Distance(playerTrans.position, trail[0]);

        if (dist > sampleMinDistance) {
            trail.Insert(0, leaderPos);

            if (trail.Count > 50) {
                trail.RemoveAt(trail.Count - 1);
            }
        }

    }

    // 获取路径上距离领导者指定距离的点
    private Vector3 GetPointAtDistance(float distanceFromLeader) {
        if(trail.Count ==0) return playerTrans.position;

        float accumulated = 0f; //累计距离

        for(int i = 0;i<trail.Count - 1;i++) {
            Vector3 a = trail[i];
            Vector3 b = trail[i + 1];

            float dist = Vector3.Distance(a, b);

            if(accumulated + dist >= distanceFromLeader) {
                float t = (distanceFromLeader - accumulated) / dist;
                return Vector3.Lerp(a, b, t);
            }

            accumulated += dist;
        }

        // 如果所有轨迹线段长度相加之和仍然小于目标距离，返回路径最后一个点
        return trail[^1];
    }

    // 重建轨迹队列并且将所有跟随者的位置刷新到领队处
    private void RebuildTrailsAndSnapFollowers() {
        trail.Clear();
        for(int i = 0; i < fieldFollowers.Count; i++) {
            fieldFollowers[i].SnapTo(ApplyFollowerOffset(playerTrans.position, i));
        }

        UpdateLeaderTrail();
    }
}
