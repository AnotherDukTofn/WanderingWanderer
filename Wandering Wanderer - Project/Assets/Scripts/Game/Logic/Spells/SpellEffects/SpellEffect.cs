using System;
using UnityEngine;
using Game.Logic.Spells.SpellConditions;
using Game.Logic.Shared.Structs;

namespace Game.Logic.Spells.SpellEffects
{
    [Serializable]
    public abstract class SpellEffect
    {
        [SerializeReference]
        public SpellCondition condition;
        public TargetResolver targetResolver;
    }
}
