using System;
using Game.Logic.Shared;

namespace Game.Logic.Spells
{
    [Serializable]
    public class ArmorEffect : SpellEffect
    {
        public PotencyRef potencyRef;
        public int duration;
    }
}
