using UnityEngine;

namespace Unity.Data
{
    [CreateAssetMenu(fileName = "ArcConfig", menuName = "SO/Arc Config")]
    public class ArcConfigSO : ScriptableObject
    {
        public int TotalNodes;
        public int MinPath;
        public float CombatMinionsRate;
        public float CombatEliteRate;
        public float CombatBossRate;
        public float ShopRate;
        public float RestRate;
        public float EventRate;
        public int MinElites;
        public int MinShop;
        public int MinRest;
        public int MinEvent;
        public float Rank1ShopRate;
        public float Rank2ShopRate;
        public float Rank3ShopRate;
        public float GoldRewards;
    }
}
