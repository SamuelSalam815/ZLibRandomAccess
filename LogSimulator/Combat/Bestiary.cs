using LogSimulator.ChacterSpec;

namespace LogSimulator.Combat;

public static class Bestiary
{
    public static Character Slime => new Character("Slime",  new StatBlock(4, 4, 4));

    public static readonly Character FinalBoss = new Character("King Slime",  new StatBlock(9, 9, 9));
}
