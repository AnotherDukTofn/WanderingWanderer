using System;
using Game.Logic.Shared;

namespace Game.Logic.Spells
{
    [Serializable]
    public class CasterHasEffect : SpellCondition
    {
        public EffectType effectType;

        public override bool Evaluate(IEntity target, SpellCastContext context)
        {
            return context.Caster.HasEffect(effectType);
        }
    }
}
