namespace Game.Logic.Shared
{
    public class RandomPolicyConfig : IDecisionPolicyConfig { }

    public class WeightedRandomPolicyConfig : IDecisionPolicyConfig
{
    [System.Serializable]
    public struct WeightedSpell
    {
        public SpellDefinition spell;
        public int weight;
    }
    
    public WeightedSpell[] weightedSpells;
}
    public class PriorityPolicyConfig : IDecisionPolicyConfig
    {
        [System.Serializable]
        public class PriorityRule
        {
            public SpellDefinition spell;
            public int priority;
            public ConditionType conditionType;
        }
    }

    public class ScriptedPolicyConfig : IDecisionPolicyConfig
    {
        public SpellDefinition[] sequence; 
    }
}