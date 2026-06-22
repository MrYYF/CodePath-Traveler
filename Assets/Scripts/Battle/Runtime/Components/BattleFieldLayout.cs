
/// <summary>
/// 战斗场地布局组件，负责提供战斗单位的站位信息
/// </summary>
public class BattleFieldLayout : MonoBehaviour {
    [Header("Scene Reference")]
    public Transform actionTrans;
    public Transform initTrans; // 单位出生点，友军从这里出生并移动到站位
    [Header("Ally Slot Segment")]
    [SerializeField] private Transform allyTopTrans;
    [SerializeField] private Transform allyBottomTrans;
    [Header("Enemy Slot Segment(Normal)")]
    [SerializeField] private Transform enemyTopTrans;
    [SerializeField] private Transform enemyBottomTrans;
    [Header("Enemy Slot Segment(Boss Triangle)")]
    [SerializeField] private Transform enemyBossTrans;

    public Vector3 GetAllySlotPos(int index, int total) {
        return LerpByMidpointRule(allyTopTrans.position, allyBottomTrans.position, index, total);
    }

    public Vector3 GetEnemySlotPos(int index, int total, EnemyLayoutFomation enemyLayoutFomation) {
        return enemyLayoutFomation switch {
            EnemyLayoutFomation.Line => LerpByMidpointRule(enemyTopTrans.position, enemyBottomTrans.position, index, total),
            EnemyLayoutFomation.BossTriangle => GetEnemyBossTrianglePos(index, total),
            _ => LerpByMidpointRule(enemyTopTrans.position, enemyBottomTrans.position, index, total)
        };
    }


    private Vector3 GetEnemyBossTrianglePos(int index, int total) {
        if (index == 0) {
            return enemyBossTrans.position;
        }
        else {
            return LerpByMidpointRule(enemyTopTrans.position, enemyBottomTrans.position, index, total);
        }
    }


    #region 线段等分算法
    private Vector3 LerpByMidpointRule(Vector3 start, Vector3 end, int index, int total) {
        float t = (index + 0.5f) / total;
        return Vector3.Lerp(start, end, t);
    }
    #endregion

    #region 获取位置相关
    /// <summary>
    /// 获取友军中心位置
    /// </summary>
    /// <returns></returns>
    public Vector3 GetAllyGroupCenter() {
        return Vector3.Lerp(allyTopTrans.position, allyBottomTrans.position, 0.5f);
    }

    /// <summary>
    /// 获取敌军中心位置
    /// </summary>
    /// <returns></returns>
    public Vector3 GetEnemyGroupCenter(EnemyLayoutFomation fomation) {
        if(fomation == EnemyLayoutFomation.BossTriangle) {
            return enemyBossTrans.position;
        }

        return Vector3.Lerp(enemyTopTrans.position, enemyBottomTrans.position, 0.5f);
    }

    #endregion
}
