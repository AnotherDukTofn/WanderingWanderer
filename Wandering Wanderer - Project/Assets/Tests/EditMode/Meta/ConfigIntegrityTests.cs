using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Unity.Data;
using Game.Unity.Data;

namespace Game.Tests.EditMode.Meta
{
    [TestFixture]
    public class ConfigIntegrityTests
    {
        #region Paths
        private const string ARC_1_PATH = "Assets/Data/Config/Arc/Arc_1_Config.asset";
        private const string ARC_2_PATH = "Assets/Data/Config/Arc/Arc_2_Config.asset";
        private const string ARC_3_PATH = "Assets/Data/Config/Arc/Arc_3_Config.asset";
        private const string COMBAT_CONFIG_PATH = "Assets/Data/Config/Combat/CombatConfig.asset";
        private const string WISDOM_CONFIG_PATH = "Assets/Data/Config/Combat/WisdomSlotConfig.asset";
        private const string REWARD_RATE_PATH = "Assets/Data/Config/Progression/RewardRateConfig.asset";
        #endregion

        [Test]
        public void ArcConfigs_AreValid()
        {
            string[] paths = { ARC_1_PATH, ARC_2_PATH, ARC_3_PATH };
            for (int i = 0; i < paths.Length; i++)
            {
                var config = AssetDatabase.LoadAssetAtPath<ArcConfigSO>(paths[i]);
                int arcNum = i + 1;
                
                Assert.IsNotNull(config, $"Arc {arcNum} Config missing at {paths[i]}");
                Assert.Greater(config.TotalNodes, 0, $"Arc {arcNum}: TotalNodes must be > 0");
                Assert.Greater(config.MinPath, 0, $"Arc {arcNum}: MinPath must be > 0");
                
                // Validate Node Type probabilities (assuming they should sum to 100)
                float totalRate = config.CombatMinionsRate + config.CombatEliteRate + config.CombatBossRate + 
                                 config.ShopRate + config.RestRate + config.EventRate;
                Assert.AreEqual(100f, totalRate, 0.01f, $"Arc {arcNum}: Node Type rates must sum to 100%");

                // Validate Shop Rank probabilities
                float shopTotal = config.Rank1ShopRate + config.Rank2ShopRate + config.Rank3ShopRate;
                Assert.AreEqual(100f, shopTotal, 0.01f, $"Arc {arcNum}: Shop Rank rates must sum to 100%");
            }
        }

        [Test]
        public void CombatConfig_IsValid()
        {
            var config = AssetDatabase.LoadAssetAtPath<CombatConfigSO>(COMBAT_CONFIG_PATH);
            Assert.IsNotNull(config, $"CombatConfig missing at {COMBAT_CONFIG_PATH}");
            
            Assert.Greater(config.HpCap, 0, "HpCap must be > 0");
            Assert.Greater(config.HpHalf, 0, "HpHalf must be > 0");
            Assert.GreaterOrEqual(config.BaseDodge, 0, "BaseDodge cannot be negative");
            Assert.LessOrEqual(config.MaxDodge, 1.0f, "MaxDodge should be <= 1.0 if using 0-1 range");
        }

        [Test]
        public void WisdomSlotConfig_IsIncreasing()
        {
            var config = AssetDatabase.LoadAssetAtPath<WisdomSlotConfigSO>(WISDOM_CONFIG_PATH);
            Assert.IsNotNull(config, $"WisdomSlotConfig missing at {WISDOM_CONFIG_PATH}");

            var t = config.WisdomEnlightenThreshold;
            Assert.Less(t.Slot1, t.Slot2, "WIS Slot 1 must be < Slot 2");
            Assert.Less(t.Slot2, t.Slot3, "WIS Slot 2 must be < Slot 3");
            Assert.Less(t.Slot3, t.Slot4, "WIS Slot 3 must be < Slot 4");
            Assert.Less(t.Slot4, t.Slot5, "WIS Slot 4 must be < Slot 5");
        }

        [Test]
        public void RewardRateConfig_IsNormalized()
        {
            var config = AssetDatabase.LoadAssetAtPath<RewardRateConfigSO>(REWARD_RATE_PATH);
            Assert.IsNotNull(config, $"RewardRateConfig missing at {REWARD_RATE_PATH}");

            // Validate all using the built-in IsValid property
            Assert.IsTrue(config.MinionArc1.IsValid, "RewardRate: Minion Arc 1 invalid");
            Assert.IsTrue(config.MinionArc2.IsValid, "RewardRate: Minion Arc 2 invalid");
            Assert.IsTrue(config.MinionArc3.IsValid, "RewardRate: Minion Arc 3 invalid");
            
            Assert.IsTrue(config.EliteArc1.IsValid, "RewardRate: Elite Arc 1 invalid");
            Assert.IsTrue(config.EliteArc2.IsValid, "RewardRate: Elite Arc 2 invalid");
            Assert.IsTrue(config.EliteArc3.IsValid, "RewardRate: Elite Arc 3 invalid");

            Assert.IsTrue(config.BossArc1.IsValid, "RewardRate: Boss Arc 1 invalid");
            Assert.IsTrue(config.BossArc2.IsValid, "RewardRate: Boss Arc 2 invalid");
            Assert.IsTrue(config.BossArc3.IsValid, "RewardRate: Boss Arc 3 invalid");
        }
    }
}
