public class DefendCommandHandler : BattleCommandHandleBase
{
    protected override IEnumerator ExecutionPhase() {
        Debug.Log("防御");
        yield break;
    }
}
