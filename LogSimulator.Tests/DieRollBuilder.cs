using LogSimulator.Rolls;

namespace LogSimulator.Tests;

public record DieRollBuilder(int[] FixedRolls, DieRollGenerator TerminalGenerator)
{
    public DieRollBuilder ThenUse(DieRollGenerator terminalGenerator) =>
        this with { TerminalGenerator = terminalGenerator };

    public DieRollBuilder ThenRepeat(int roll) => this with { TerminalGenerator = _ => roll };

    public static DieRollBuilder Provide(params int[] fixedRolls) => new(fixedRolls, _ => 0);

    public static implicit operator DieRollGenerator(DieRollBuilder builder)
    {
        var index = 0;
        return dieFace => index >= builder.FixedRolls.Length
            ? builder.TerminalGenerator(dieFace)
            : builder.FixedRolls[index++];
    }
};
