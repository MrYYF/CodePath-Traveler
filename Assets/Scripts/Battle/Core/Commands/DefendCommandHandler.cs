public class DefendCommandHandler : BattleCommandHandleBase
{
    protected override IEnumerator ExecutionPhase() {
        BattleEntity actor = Actor;

        actor.EnterDefendStance();

        yield break;
    }
}
