namespace Game.Logic.Shared
{
    public enum Element
    {
        Fire = 0,
        Water = 1,
        Ice = 2,
        Lightning = 3
    }

    public enum TargetResolverType
    {
        Caster = 0,
        SelectedEnemy = 1,
        AllEnemies = 2, 
        RandomEnemies = 3,
        LowestHpEnemies = 4,
        SecondaryRandom = 5
    }

    public enum TieBreaker
    {
        None = 0,
        LowestResistance = 1
    }

    public enum Rank
    {
        I = 1,
        II = 2,
        III = 3
    }
}
