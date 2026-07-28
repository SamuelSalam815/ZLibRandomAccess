namespace LogSimulator.Logging;

public enum EventScope
{
    Global = 0,
    Game,
    GamePhase,
    // TODO: some intermediate scope
    Turn,
    CombatAction,
    Roll,
}
