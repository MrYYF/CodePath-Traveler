/// <summary>
/// 
/// </summary>
public class TurnEndState : BattleState
{
    public TurnEndState(BattleController controller) : base(controller) {}

    public override IEnumerator Execute() {
        yield break;
        //throw new System.NotImplementedException();
    }

}
