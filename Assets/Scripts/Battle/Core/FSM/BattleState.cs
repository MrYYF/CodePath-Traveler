public abstract class BattleState {

    protected readonly BattleContorller _contorller;

    protected BattleState(BattleContorller contorller) {
        _contorller = contorller;
    }

    public virtual IEnumerator Enter() {
        yield break;
    }

    public abstract IEnumerator Execute();

    public virtual IEnumerator Exit() {
        yield break;
    }
}
