namespace LogSimulator.ChacterSpec;

public record Character(string Name, StatBlock Stats)
{
    public AbilityScore Fortitude => Stats.Get(AbilityType.Fortitude);
    public AbilityScore Agility => Stats.Get(AbilityType.Agility);
    public AbilityScore Prowess => Stats.Get(AbilityType.Prowess);
};
