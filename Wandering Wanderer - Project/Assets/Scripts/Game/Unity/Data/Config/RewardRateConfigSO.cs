using UnityEngine;
using System;

namespace Game.Unity.Data
{
    [System.Serializable]
    public struct RewardRankRate
    {
        public float Rank1;
        public float Rank2;
        public float Rank3;

        public bool IsValid => Mathf.Approximately(Rank1 + Rank2 + Rank3, 100f);
    }

    [CreateAssetMenu(fileName = "RewardRateConfigSO", menuName = "SO/RewardRateConfigSO")]
    public class RewardRateConfigSO : ScriptableObject
    {
        [Header("Minion Reward Rate")] 
        public RewardRankRate MinionArc1;
        public RewardRankRate MinionArc2;
        public RewardRankRate MinionArc3;

        [Header("Elite Reward Rate")] 
        public RewardRankRate EliteArc1;
        public RewardRankRate EliteArc2;
        public RewardRankRate EliteArc3;

        [Header("Boss Reward Rate")] 
        public RewardRankRate BossArc1;
        public RewardRankRate BossArc2;
        public RewardRankRate BossArc3;
    }
}
