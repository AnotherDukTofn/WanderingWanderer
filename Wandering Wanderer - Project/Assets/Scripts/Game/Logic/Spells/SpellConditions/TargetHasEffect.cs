using System;
using Game.Logic.Shared;

namespace Game.Logic.Spells.SpellConditions
{
    [Serializable]
    public class TargetHasEffect : SpellCondition
    {
        public EffectType effectType;

        public override bool Evaluate(IEntity target, SpellCastContext context)
        {
            return target.HasEffect(effectType);
        }
    }
}
