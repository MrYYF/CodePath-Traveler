


using System;
using UnityEngine.AddressableAssets;

public class BattleService : Singleton<BattleService> {

    private AssetReference _returnSceneAfterBattle;

    private BattleStartPreload _pendingPreload;
    private bool HasPendingPreload => _pendingPreload != null;

    public void StartBattleFromAction(ChallengeAction challengeAction) {
        List<CharacterRuntimeData> allies = PartyManager.Instance.PartyMembers;
        List<CharacterRuntimeData> enemy = new List<CharacterRuntimeData>();
        foreach (var enemyDef in challengeAction.enemyTeamMembers) {
            if (enemyDef == null) continue;
            enemy.Add(new CharacterRuntimeData(enemyDef));
        }

        StartBattle(allies,
            enemy,
            challengeAction.battleSceneReference,
            challengeAction.enemyLayoutFomation);
    }

    public void StartBattle(List<CharacterRuntimeData> allies,
        List<CharacterRuntimeData> enemy,
        AssetReference battleScene,
        EnemyLayoutFomation enemyLayoutFomation) {
        Debug.Log("Starting battle...");

        SceneLoadManager scnenLoadManager = SceneLoadManager.Instance;

        // 缓存当前场景，以便战斗结束后返回
        _returnSceneAfterBattle = scnenLoadManager.activeScene;

        // 重置战斗状态
        NormalizeBattleSessionState(allies);

        // 创建战斗预加载数据
        _pendingPreload = new BattleStartPreload(
            new List<CharacterRuntimeData>(allies),
            new List<CharacterRuntimeData>(enemy),
            enemyLayoutFomation);

        // 加载战斗场景
        scnenLoadManager.RequestLoad(new SceneLoadRequest(battleScene, FadeStyle.WipeMask, GameMode.Battle));
    }

    private void NormalizeBattleSessionState(List<CharacterRuntimeData> members) {
        foreach (var member in members) {
            member.ResetBattleBP();
            if (member.CurrentHP <= 0) {
                member.CurrentHP = 1;
            }
        }
    }
}
