using System;
using Game.Logic.Shared;

namespace Game.Logic.Spells
{
    [Serializable]
    public class HealEffect : SpellEffect
    {
        public PotencyRef potencyRef;
    }
}
