using Framework.Event;

public readonly struct GameModeChangedEvent : IEvent
{
    public readonly GameMode NewGameMode;

    public GameModeChangedEvent(GameMode newGameMode)
    {
        this.NewGameMode = newGameMode;
    }
}
