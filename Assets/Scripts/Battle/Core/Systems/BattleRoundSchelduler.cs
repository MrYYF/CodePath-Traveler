/// <summary>
/// 正式的CTB / 双列表回合调度器
/// 
/// 维护当前回合剩余行动列表、下一回合预测列表
/// 决定下一位行动的BattleEntity
/// 把结果整理成时间轴UI能直接使用的预测节点(BattleTimelinePredictionNode)
/// 
/// 只负责排序，不涉及UI与命令执行的 调度层
/// </summary>
public class BattleRoundSchelduler {
    private List<BattleEntity> _currentRound = new List<BattleEntity>();
    private List<BattleEntity> _nextRound = new List<BattleEntity>();

    public void Initialize(List<BattleEntity> allEntities) {
        _currentRound = GenerateSortedOrder(allEntities);
        _nextRound = GenerateSortedOrder(allEntities);
        TriggerRoundStart(allEntities);
    }

    /// <summary>
    /// 获取下一位行动的单位
    /// </summary>
    /// <param name="allEntities"></param>
    /// <returns></returns>
    public BattleEntity GetNextActor(List<BattleEntity> allEntities) {
        int aliveCount = 0;

        foreach (BattleEntity entity in allEntities) {
            if (entity.IsAlive) {
                aliveCount++;
            }
        }

        // 如果场上没有存活目标时跳出
        if (aliveCount <= 0) {
            return null;
        }

        // 遍历当前回合行动者，取出下一位可以行动的单位
        int guard = aliveCount * 4;
        while (guard-- > 0) {
            // 如果当前回合已经取完，则推进到下一回合
            if (_currentRound.Count == 0) {
                StartNextRound(allEntities);
                if (_currentRound.Count == 0) {
                    continue;
                }
            }
            BattleEntity candidate = _currentRound[0];
            _currentRound.RemoveAt(0);
            if (!candidate.IsAlive) {
                continue;
            }

            return candidate;
        }

        //如果没有则返回null
        return null;
    }

    /// <summary>
    /// 开始下一个回合，将nextround的数据给到currentround并生成下一回合的预测
    /// </summary>
    /// <param name="allEntities"></param>
    private void StartNextRound(List<BattleEntity> allEntities) {
        _currentRound = _nextRound;
        _nextRound = GenerateSortedOrder(allEntities);
        TriggerRoundStart(allEntities);
    }

    /// <summary>
    /// 根据所有的entity生成一份按照速度优先、友方优先、id优先的列表
    /// </summary>
    /// <param name="allEntities"></param>
    /// <returns></returns>
    private List<BattleEntity> GenerateSortedOrder(List<BattleEntity> allEntities) {
        List<BattleEntity> result = new List<BattleEntity>();

        for (int i = 0; i < allEntities.Count; i++) {
            if (allEntities[i].IsAlive) {
                result.Add(allEntities[i]);
            }
        }
        result.Sort((a, b) => {
            int speedCompare = b.GetCurrentSpeed().CompareTo(a.GetCurrentSpeed());
            if (speedCompare != 0) {
                return speedCompare;
            }

            if (a.IsPlayer) return -1;
            if (b.IsPlayer) return 1;
            return string.CompareOrdinal(a.ID, b.ID);
        });

        string orderLog = "CTB顺序：";
        for (int i = 0; i < result.Count; i++) {
            BattleEntity entity = result[i];
            orderLog += $"{i + 1}.{entity.Definition.Name}({entity.GetCurrentSpeed()})";
        }
        return result;
    }

    /// <summary>
    /// 构建预测时间轴
    /// </summary>
    /// <returns>时间轴节点集合</returns>
    public List<BattleTimelinePredictionNode> BuildTimelinePrediction() {
        List<BattleTimelinePredictionNode> result = new List<BattleTimelinePredictionNode>(_currentRound.Count + _nextRound.Count) { };

        // 当前回合剩余行动者
        for (int i = 0; i < _currentRound.Count; i++) {
            BattleEntity entity = _currentRound[i];
            if (!entity.IsAlive) {
                continue;
            }
            result.Add(new BattleTimelinePredictionNode($"{entity.ID}_R0", entity, 0));
        }

        // 当前回合剩余行动者
        for (int i = 0; i < _nextRound.Count; i++) {
            BattleEntity entity = _nextRound[i];
            if (!entity.IsAlive) {
                continue;
            }
            result.Add(new BattleTimelinePredictionNode($"{entity.ID}_R1", entity, 1));
        }

        return result;
    }


    private void TriggerRoundStart(List<BattleEntity> allEntities) {
        for (int i = 0; i < allEntities.Count; i++) {
            BattleEntity entity = allEntities[i];
            if (!entity.IsAlive) {
                continue;
            }

            entity.ClearDefendStance();
            if (entity.IsPlayer) {
                entity.RecoverBP();
            }
        }
    }
}
