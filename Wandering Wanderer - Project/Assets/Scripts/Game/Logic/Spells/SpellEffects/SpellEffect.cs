using System;
using UnityEngine;
using Game.Logic.Shared;

namespace Game.Logic.Spells
{
    [Serializable]
    public abstract class SpellEffect
    {
        [SerializeReference]
        public SpellCondition condition;
        public TargetResolver targetResolver;
    }
}
