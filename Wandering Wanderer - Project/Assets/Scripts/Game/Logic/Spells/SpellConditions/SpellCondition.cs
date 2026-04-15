using System;
using Game.Logic.Shared;

namespace Game.Logic.Spells
{
    [Serializable]
    public abstract class SpellCondition
    {
        public abstract bool Evaluate(IEntity target, SpellCastContext context);
    }
}
