using LogSimulator.Rolls;

namespace LogSimulator;

public abstract record GamePhase
{
    public Hero Hero { get; }
    public Bestiary Bestiary { get; }

    private GamePhase(Hero hero, Bestiary bestiary)
    {
        Hero = hero;
        Bestiary = bestiary;
    }

    public abstract IEnumerable<RollRequest> GetRequiredRolls();

    public abstract GamePhase ProgressGameState(IEnumerable<RollResult> rolls, out IEnumerable<string> logs);

    public record ExplorationPhase(
        Hero Hero,
        Bestiary Bestiary,
        Adversary? ResolvedAdversary,
        TestResult? AdversaryEncounterTest,
        TestResult? DiscoveryTest) : GamePhase(Hero, Bestiary)
    {
        public override IEnumerable<RollRequest> GetRequiredRolls()
        {
            throw new NotImplementedException();
        }

        private RollRequest RollForEncounter()
        {
            throw new NotImplementedException();
        }

        private RollRequest RollToSpawnBeast()
        {
            throw new NotImplementedException();
        }

        private TestRequest TestDiscovery()
        {
            throw new NotImplementedException();
        }

        private RollRequest RollToRest()
        {
            throw new NotImplementedException();
        }

        public override GamePhase ProgressGameState(IEnumerable<RollResult> rolls, out IEnumerable<string> logs)
        {
            throw new NotImplementedException();
        }
    }

    public record RecoveryPhase(Hero Hero, Bestiary Bestiary) : GamePhase(Hero, Bestiary)
    {
        public override IEnumerable<RollRequest> GetRequiredRolls()
        {
            throw new NotImplementedException();
        }

        public override GamePhase ProgressGameState(IEnumerable<RollResult> rolls, out IEnumerable<string> logs)
        {
            throw new NotImplementedException();
        }
    }
};
