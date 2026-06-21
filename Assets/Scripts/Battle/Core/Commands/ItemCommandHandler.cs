public class ItemCommandHandler : BattleCommandHandleBase {

    private ConsumableItemSO _consumableItem;
    private BattleEntity _target;
    private bool _isHpItem;

    protected override bool PreparePhase() {
        _consumableItem = (ConsumableItemSO)Command.ItemDefinition;
        _target = Controller.AllEntities.Find(e => e.ID == Command.TargetEntityID);
        _isHpItem = _consumableItem.itemIconKey == ItemIconKey.Healing;
        return _target != null;
    }

    protected override IEnumerator AnimationPhase() {
        Actor.Unit.PlayUseItemAnimation();
        float windup = Controller.Config.AttackWindupTime;
        if (windup > 0) {
            yield return new WaitForSeconds(windup);
        }
    }

    protected override IEnumerator ExecutionPhase() {
        InventoryManager.Instance.RemoveItem(_consumableItem, 1);

        int restoreAmount = _consumableItem.restoreAmount;

        if (_isHpItem) {
            _target.Heal(restoreAmount);
        }
        else {
            _target.restoreSP(restoreAmount);
            Controller.SpawnDamagePopup(_target, restoreAmount, DamagePopupType.Magic);
        }

        yield break;
    }

    protected override IEnumerator ResolvePhase() {
        float recover = Controller.Config.AttackRecoveryTime;
        if (recover > 0) {
            yield return new WaitForSeconds(recover);
        }
    }
}
