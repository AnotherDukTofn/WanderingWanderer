using System;
using Game.Logic.Shared.Structs;

namespace Game.Logic.Spells.SpellEffects
{
    [Serializable]
    public class ArmorEffect : SpellEffect
    {
        public PotencyRef potencyRef;
        public int duration;
    }
}
