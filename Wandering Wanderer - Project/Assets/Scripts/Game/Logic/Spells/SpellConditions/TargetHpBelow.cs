using System;
using Game.Logic.Shared;

namespace Game.Logic.Spells
{
    [Serializable]
    public class TargetHpBelow : SpellCondition
    {
        public float threshold;

        public override bool Evaluate(IEntity target, SpellCastContext context)
        {
            float hpPercent = target.MaxHp > 0 ? (float)target.CurrentHp / target.MaxHp : 0f;
            return hpPercent < threshold;
        }
    }
}
