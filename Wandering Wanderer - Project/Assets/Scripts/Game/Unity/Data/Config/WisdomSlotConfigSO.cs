using UnityEngine;
using System;

namespace Game.Unity.Data
{
    [System.Serializable]
    public struct WisThreshold
    {
        public int Slot1;
        public int Slot2;
        public int Slot3;
        public int Slot4;
        public int Slot5;
    }

    [CreateAssetMenu(fileName = "WisdomSlotConfig", menuName = "SO/WisdomSlotConfig")]
    public class WisdomSlotConfigSO : ScriptableObject
    {
        public WisThreshold WisdomEnlightenThreshold;
    }
}
