namespace Game.Logic.Shared
{
    public enum EnemyType
    {
        Minion = 0,
        Elite = 1,
        Boss = 2
    }

    public enum DecisionPolicyType
    {
        RandomPolicy = 0,
        WeightedRandomPolicy = 1,
        PriorityPolicy = 2, 
        ScriptedPolicy = 3
    }
}
