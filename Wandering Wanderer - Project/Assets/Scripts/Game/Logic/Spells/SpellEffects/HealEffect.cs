using System;
using Game.Logic.Shared.Structs;

namespace Game.Logic.Spells.SpellEffects
{
    [Serializable]
    public class HealEffect : SpellEffect
    {
        public PotencyRef potencyRef;
    }
}
