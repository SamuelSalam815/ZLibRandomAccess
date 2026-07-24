namespace LogSimulator.Rolls;

/// <summary>
/// Represents a method for rolling a die. The result is expected to be in the range [1, <paramref name="dieSize"/>]
/// </summary>
/// <param name="dieSize">The number of faces on the die.</param>
public delegate int DieRollGenerator(int dieSize);
