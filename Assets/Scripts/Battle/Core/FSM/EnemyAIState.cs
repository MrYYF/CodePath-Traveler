/// <summary>
/// 进入敌方AI行动回合
/// </summary>
public class EnemyAIState : BattleState {
    private class ActionDecision {
        public SkillDataSO SelectedSkill;
        public bool IsTelegraph;
        public float Weight;
    }

    public EnemyAIState(BattleController controller) : base(controller) { }

    public override IEnumerator Execute() {
        // 处理挂起的转阶段切换提示
        yield return PlayPendingBossPhaseIntro();

        // AI计算出一条行动命令
        _controller.CurrentCommandRequest = BuildAICommand();

        // 思考等待时间
        yield return new WaitForSeconds(_controller.Config.AIThinkDuration);

        // 进入执行表现阶段
        _controller.SetState(new PerformActionState(_controller));
    }

    /// <summary>
    /// 进行挂起的阶段转换
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayPendingBossPhaseIntro() {
        BattleEntity actor = _controller.CurrentEntity;
        // 应用挂起的转阶段缓存
        if (!actor.TryApplyPendingBossPhase(out BossPhaseConfig appliedPhase)) {
            yield break;
        }

        // 广播提示信息
        EventBus.Publish(new BattleNotificationEvent(appliedPhase.promptText));

        // 阶段切换延迟
        float introDelay = appliedPhase.introDelay;
        if(introDelay > 0) {
            yield return new WaitForSeconds(introDelay);
        }
    }

    /// <summary>
    /// 构建AI行动指令
    /// </summary>
    /// <returns></returns>
    private BattleCommandRequest BuildAICommand() {
        BattleEntity actor = _controller.CurrentEntity;

        // 上一回合蓄力则这回合直接释放
        if(actor.PreparedSkill != null) {
            return BuildPreparedSkillCommand();
        }

        // 进行行动候选评估
        ActionDecision decision = EvaluateActions();

        // 如果选择的是蓄力技能
        if (decision.IsTelegraph) {
            return BuildTelegraphCommand(decision.SelectedSkill);
        }
        return BuildCommand(decision.SelectedSkill);
    }

    /// <summary>
    /// 构建行动命令
    /// </summary>
    /// <param name="actor">行动者</param>
    /// <param name="selectedSkill">选择的技能</param>
    /// <returns>行动命令</returns>
    private BattleCommandRequest BuildCommand(SkillDataSO selectedSkill) {
        // 构建指令
        BattleCommandRequest command =
            selectedSkill == _controller.CurrentEntity.Definition.BasicAttack ?
            BattleCommandRequest.CreateAttack(_controller.CurrentEntity) :
            BattleCommandRequest.CreateSkill(selectedSkill);

        // 选取目标
        AutoTargetSelection(command);
        return command;
    }

    #region 蓄力技能
    private BattleCommandRequest BuildTelegraphCommand(SkillDataSO skill) {
        BattleEntity actor = _controller.CurrentEntity;

        // 下回合准备使用的技能
        string skillName = skill.skillName;
        actor.PreparedSkill = skill;

        // 播放蓄力特效
        actor.Unit.PlayTelegraphVfx(skill);

        // 技能名称与提示
        EventBus.Publish(new BattleNotificationEvent("敌人正在酝酿一次强大的攻击"));
        EventBus.Publish(new SkillNameDisplayEvent(actor, skillName));

        return BattleCommandRequest.CreateDefend(); // 跳过当前回合
    }

    /// <summary>
    /// 构建准备好的技能命令
    /// </summary>
    /// <param name="skill">技能数据</param>
    /// <returns>战斗命令</returns>
    private BattleCommandRequest BuildPreparedSkillCommand() {
        BattleEntity actor = _controller.CurrentEntity;

        // 准备释放的技能
        SkillDataSO skillToCast = actor.PreparedSkill;
        string skillName = skillToCast.skillName;

        // 清除缓存
        actor.PreparedSkill = null;
        actor.Unit.StopTelegraphVfx();

        // 通知
        EventBus.Publish(new BattleNotificationEvent($"{actor.Definition.Name}释放了[{skillName}]!"));
        EventBus.Publish(new SkillNameDisplayEvent(actor, skillName));

        return BuildCommand(skillToCast);
    }

    #endregion 

    /// <summary>
    /// 根据SP数值选择第一个可用技能
    /// </summary>
    /// <param name="actor">当前行动对象</param>
    /// <returns>技能数据</returns>
    private SkillDataSO ChooseFirstAvailableSkill(BattleEntity actor) {
        if (actor.Definition.InitialSkills == null) {
            return null;
        }

        for (int i = 0; i < actor.Definition.InitialSkills.Count; i++) {
            SkillDataSO skill = actor.Definition.InitialSkills[i];
            if (skill.spCost <= actor.CurrentSP) {
                return skill;
            }
        }

        return actor.Definition.BasicAttack;
    }

    /// <summary>
    /// 自动根据行动指令选择目标
    /// </summary>
    /// <param name="command">行动指令</param>
    private void AutoTargetSelection(BattleCommandRequest command) {
        SkillDataSO skill = command.Skill;

        // 群体目标
        if (skill.targetType != TargetType.SingleAlly && skill.targetType != TargetType.SingleEnemy) {
            command.TargetEntityID = null;
            return;
        }

        // 单体目标
        List<BattleEntity> candidates = BattleTargeting.GetAliveTargetsByType(
            _controller.CurrentEntity,
            skill.targetType,
            _controller.AllEntities
            );

        if (candidates.Count == 0) {
            return;
        }

        bool isSingleAllyHeal = skill.targetType == TargetType.SingleAlly && skill.skillType == SkillType.Heal;
        BattleEntity target = isSingleAllyHeal ?
            GetLowestHPTarget(candidates) :
             candidates[Random.Range(0, candidates.Count)];
        command.TargetEntityID = target.ID;

    }

    /// <summary>
    /// 获取hp比例最低的实体
    /// </summary>
    /// <param name="candidates">待选实体集合</param>
    /// <returns></returns>
    private BattleEntity GetLowestHPTarget(List<BattleEntity> candidates) {
        BattleEntity bestTarget = candidates[0];
        float bestRatio = bestTarget.CurrentHP / bestTarget.TotalStats.MaxHP;

        for (int i = 0; i < candidates.Count; i++) {
            BattleEntity candidate = candidates[i];
            float ratio = candidate.CurrentHP / candidate.TotalStats.MaxHP;

            if (ratio >= bestRatio) {
                continue;
            }

            bestTarget = candidate;
            bestRatio = ratio;
        }

        return bestTarget;
    }

    /// <summary>
    /// 评估当前所有可行的行动并打分
    /// </summary>
    /// <returns></returns>
    private ActionDecision EvaluateActions() {
        BattleEntity actor = _controller.CurrentEntity;
        CharacterDefinitionSO def = actor.Definition;
        BossPhaseConfig phase = actor.GetActiveBossPhaseConfig();
        EnemyAITuningConfig tuning = ((EnemyDefinitionSO)def).aiTuning;

        // 待选技能权重列表
        List<ActionDecision> candidates = new List<ActionDecision>() {
            // 默认添加基础攻击
            new ActionDecision {
                SelectedSkill = def.BasicAttack,
                IsTelegraph = false,
                Weight = ApplyPhaseWeight(tuning.basicAttackWeight,
                phase != null ? phase.basicAttackWeightMultiplier : 1)
            }
        };

        int currentSP = actor.CurrentSP;
        foreach (var skill in def.InitialSkills) {
            if (skill.spCost > currentSP) {
                continue;
            }

            // 计算各技能的权重
            float weight = 0f;
            bool isTelegraph = false;
            switch (skill.skillType) {
                case SkillType.Damage:
                    // 技能是否属于蓄力必杀
                    if (IsTelegraphSkill(skill, tuning)) {
                        isTelegraph = true;
                        weight = ShouldPreferTelegraphSkill(tuning) ?
                            ApplyPhaseWeight(tuning.telgraphWeight, phase != null ? phase.telgraphWeightMultiplier : 1) :
                            ApplyPhaseWeight(tuning.damageSkillWeight, phase != null ? phase.damageSkillWeightMultiplier : 1);
                    }
                    break;
                case SkillType.Heal:
                    weight = EvaluateHealWeight(skill, tuning);
                    weight = ApplyPhaseWeight(weight, phase != null ? phase.healWeightMultiplier : 1);
                    break;
                case SkillType.Buff:
                case SkillType.Debuff:
                    weight = tuning.defaultSkillWeight;
                    break;
            }

            if (weight > 0) {
                candidates.Add(new ActionDecision {
                    SelectedSkill = skill,
                    IsTelegraph = isTelegraph,
                    Weight = weight
                });
            }
        }

        return PickByWeight(candidates);
    }

    /// <summary>
    /// 计算阶段权重
    /// </summary>
    /// <param name="baseWeight">基础权重</param>
    /// <param name="multiplier">加成系数</param>
    /// <returns>权重</returns>
    private float ApplyPhaseWeight(float baseWeight, float multiplier) {
        return baseWeight * multiplier;
    }

    /// <summary>
    /// 判断技能是否属于蓄力必杀
    /// </summary>
    /// <param name="skill">技能数据</param>
    /// <param name="tuning">行为配置</param>
    /// <returns>是返回true，否则返回false</returns>
    private static bool IsTelegraphSkill(SkillDataSO skill, EnemyAITuningConfig tuning)
        => skill.basePower >= tuning.telgraphMinBasePower &&
        skill.spCost >= tuning.telgraphMinSpCost;

    /// <summary>
    /// 判断是否使用蓄力必杀
    /// </summary>
    /// <param name="tuning">行为配置</param>
    /// <returns>血量满足使用条件阈值返回true，否则返回false</returns>
    private bool ShouldPreferTelegraphSkill(EnemyAITuningConfig tuning) {
        BattleEntity actor = _controller.CurrentEntity;
        return actor.CurrentHP / (float)actor.TotalStats.MaxHP <= tuning.telgraphHpRatioThreshold;
    }

    /// <summary>
    /// 评估治疗技能的权重
    /// </summary>
    /// <param name="skill">技能数据</param>
    /// <param name="tuning">行为配置</param>
    /// <returns></returns>
    private float EvaluateHealWeight(SkillDataSO skill, EnemyAITuningConfig tuning) {
        int lowHpCount = 0;
        foreach (var ally in _controller.AllEntities) {
            if (ally.IsPlayer || !ally.IsAlive) {
                continue;
            }

            if (ally.CurrentHP / (float)ally.TotalStats.MaxHP < tuning.healLowHpRatioThreshold) {
                lowHpCount++;
            }
        }

        // 无残血队友
        if (lowHpCount <= 0) {
            return 0;
        }

        // 治疗技能释放权重
        float weight = tuning.healBaseWeight + lowHpCount * tuning.healPreLowHpBonus;
        return skill.targetType == TargetType.SingleAlly && lowHpCount > 1 ?
            weight * tuning.singleHealMultiLowHpPenalty :
            weight;
    }

    /// <summary>
    /// 根据权重在待选行动列表中随机选择一个行动
    /// </summary>
    /// <param name="candidates">待选行动列表</param>
    /// <returns>选择的行动</returns>
    private ActionDecision PickByWeight(List<ActionDecision> candidates) {
        float totalWeight = 0;
        foreach (var candidate in candidates) {
            totalWeight += candidate.Weight;
        }

        float randomWeight = Random.Range(0, totalWeight);

        foreach (var candidate in candidates) {
            totalWeight -= candidate.Weight;
            if (totalWeight < 0) {
                return candidate;
            }
        }

        return candidates[^1];
    }
}
