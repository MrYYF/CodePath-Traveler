using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStartPreload {
    public List<CharacterRuntimeData> allies { get; }
    public List<CharacterRuntimeData> enemy { get; }

    public EnemyLayoutFomation enemyLayoutFomation { get; }

    public BattleStartPreload(List<CharacterRuntimeData> allies,
        List<CharacterRuntimeData> enemy,
        EnemyLayoutFomation enemyLayoutFomation) {
        this.allies = allies;
        this.enemy = enemy;
        this.enemyLayoutFomation = enemyLayoutFomation;
    }
}
