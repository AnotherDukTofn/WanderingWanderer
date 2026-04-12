using System;

namespace Game.Logic.Shared
{
    [Serializable]
    public struct StatModifier
    {
        public StatType stat;
        public ModType modType;
        public float value;
    }   
}
