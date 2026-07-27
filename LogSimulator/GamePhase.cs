using LogSimulator.ChacterSpec;

namespace LogSimulator;

public abstract record GamePhase(GameState GameState)
{
    public Character Hero => GameState.Hero;

    public abstract GamePhase? ProgressGame(DieRollGenerator dieRollGenerator);
};
