public class EscapeCommandHandler : BattleCommandHandleBase
{
    protected override IEnumerator ExecutionPhase() {
        Debug.Log("逃脱");
        yield break;
    }
}
