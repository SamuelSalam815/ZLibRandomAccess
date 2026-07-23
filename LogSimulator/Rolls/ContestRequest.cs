namespace LogSimulator.Rolls;

/// <summary>
/// Represents parameters used to perform a contested roll against an adversary.
/// </summary>
/// <param name="HeroRollRequest">The hero's roll parameters.</param>
/// <param name="AdversaryRollRequest">The adversary's roll parameters</param>
public readonly record struct ContestRequest(RollRequest HeroRollRequest, RollRequest AdversaryRollRequest);
