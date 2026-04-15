using System;
using Game.Logic.Shared;

namespace Game.Logic.Spells
{
    [Serializable]
    public class DamageEffect : SpellEffect
    {
        public PotencyRef potencyRef;
    }
}
