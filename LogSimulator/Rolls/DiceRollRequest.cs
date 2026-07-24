using System.Diagnostics;

namespace LogSimulator.Rolls;

public record DiceRollRequest(
    int BaseDieCount,
    int DieSize,
    ExtraRollRequest? ExtraRollRequest = null
)
{
    public int RequiredRollCount { get; } = BaseDieCount + ExtraRollRequest.GetDieCount();

    public DiceRollResult ResolveWith(DieRollGenerator dieRollGenerator)
    {
        var rolls = new int[RequiredRollCount];
        for (var i = 0; i < rolls.Length; i++)
        {
            rolls[i] = dieRollGenerator(DieSize);
        }
        return ResolveWith(rolls);
    }

    public DiceRollResult ResolveWith(int[] rolls)
    {
        var rollDescription = new GameEventDescription();
        var extraRollCount = ExtraRollRequest.GetDieCount();
        var baseRollString = $"Rolled {BaseDieCount}d{DieSize}";

        switch (extraRollCount)
        {
            case 0:
                rollDescription.AddLine(baseRollString);
                break;
            case >0:
                switch (ExtraRollRequest)
                {
                    case ExtraRollRequest.Advantage { Count: 1 }:
                        rollDescription.AddLine("{0} with advantage", baseRollString);
                        break;
                    case ExtraRollRequest.Advantage {Count: var advantage}:
                        rollDescription.AddLine("{0} with advantage ({1})",baseRollString, advantage);
                        break;
                    case ExtraRollRequest.Disadvantage { Count: 1 }:
                        rollDescription.AddLine("{0} with disadvantage", baseRollString);
                        break;
                    case ExtraRollRequest.Disadvantage { Count: var disadvantage }:
                        rollDescription.AddLine("{0} with disadvantage ({1})", baseRollString, disadvantage);
                        break;
                    default:
                        throw UnexpectedCaseException.CreateFrom(ExtraRollRequest);
                }
                break;
            default:
                throw UnexpectedCaseException.CreateFrom(extraRollCount);
        }
        rollDescription.AddLine("Die Rolls: [{0}]", string.Join(", ", rolls));

        return DiceRollResult.Create(this, rolls, rollDescription);
    }
}
