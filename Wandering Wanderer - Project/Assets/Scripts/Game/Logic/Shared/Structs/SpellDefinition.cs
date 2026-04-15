using System;
using Game.Logic.Spells.SpellEffects;

namespace Game.Logic.Shared.Structs
{
    [Serializable]
    public struct SpellDefinition
    {
        public string id;
        public string displayName;
        public Rank rank;
        public Element element;
        public int baseCost;
        public int baseCooldown;
        public int minWisdomToImprint;

        public SpellEffect[] effects;
    }
}
