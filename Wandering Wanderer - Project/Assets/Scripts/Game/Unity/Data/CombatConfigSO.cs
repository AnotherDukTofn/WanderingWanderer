using UnityEngine;

namespace Unity.Data
{
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "SO/Combat Config")]
    public class CombatConfigSO : ScriptableObject
    {
        public float HitThreshold;
        public float BaseDodge;
        public float MaxDodge;
        public float BaseMpRecovery;
        public float HpCap;
        public float HpHalf;
        public float MpCoeff;
    }
}
