namespace LogSimulator;

public abstract record GamePhase
{
    public abstract GameProgress ProgressGame(DieRollGenerator dieRollGenerator);
};
