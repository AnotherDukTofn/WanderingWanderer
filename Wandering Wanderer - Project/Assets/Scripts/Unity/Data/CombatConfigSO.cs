using UnityEngine;

namespace Unity.Data 
{
    [CreateAssetMenu(fileName = "CombatConfigSO", menuName = "SO/Combat Config")]
    public class CombatConfigSO : ScriptableObject
    {
        public float HIT_THRESHOLD;
        public float BASE_DODGE;
        public float MAX_DODGE;
        public float BASE_MP_RECOVERY;
        public float HP_CAP;
        public float HP_HALF;
        public float MP_COEFF;
    }
}
