using System;
using System.Collections.Generic;
using Game.Logic.Shared;

namespace Tests.EditMode.Stubs
{
    public class StubEntity : IEntity
    {
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }

        public float Potency { get; set; }
        public float Resistance { get; set; }
        public float AGI { get; set; }

        public HashSet<EffectType> Effects = new HashSet<EffectType>();
        public Dictionary<Element, float> Resistances = new Dictionary<Element, float>();

        public bool HasEffect(EffectType effectType)
        {
            return Effects.Contains(effectType);
        }

        public float GetEffectivePotency(Element element)
        {
            return Potency;
        }

        public float GetEffectiveResistance(Element element)
        {
            return Resistances.ContainsKey(element) ? Resistances[element] : Resistance;
        }

        public float GetEffectiveAGI()
        {
            return AGI;
        }
    }
}
