using Game.Logic.Shared;
using UnityEngine;

namespace Game.Unity.Data
{
    [CreateAssetMenu(fileName = "EnemyDefinitionSO", menuName = "SO/EnemyDefinitionSO")]
    public class EnemyDefinitionSO : ScriptableObject
    {
        public string id;
        public string displayName;
        public EnemyType type;
        public int maxHp;

        [Header("Potencies")]
        public float baseFirePot;
        public float baseWaterPot;
        public float baseIcePot;
        public float baseLightningPot;

        [Header("Resistances")]
        public float baseFireRes;
        public float baseWaterRes;
        public float baseIceRes;
        public float baseLightningRes;

        [Header("Spells")]
        public SpellDefinitionSO[] spells;
        public DecisionPolicyType decisionPolicyType;
        [SerializeReference]
        public IDecisionPolicyConfig decisionPolicy;
    }
}
