using LogSimulator.ChacterSpec;

namespace LogSimulator;

public static class Bestiary
{
    public static Character Slime => new Character("Slime",  new StatBlock(3, 3, 3));

    public static readonly Character FinalBoss = new Character("King Slime",  new StatBlock(9, 9, 9));
}
