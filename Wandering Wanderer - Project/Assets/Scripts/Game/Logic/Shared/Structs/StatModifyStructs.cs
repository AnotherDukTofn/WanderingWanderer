using System;
using Game.Logic.Shared.Enums;

namespace Game.Logic.Shared.Structs
{
    [Serializable]
    public struct StatModifier
    {
        public StatType stat;
        public ModType modType;
        public float value;
    }

    [Serializable]
    public struct PotencyRef
    {
        public Element element;
        public float coefficient;

        public float GetValue(float potency) => potency * coefficient;
    }
}
