using NUnit.Framework;
using Game.Unity.Data; // Namespace chứa SO của bạn

namespace Tests.EditMode.Meta
{
    [TestFixture]
    public class RewardRateConfigTests
    {
        private RewardRateConfigSO _config;

        [SetUp]
        public void SetUp()
        {
            // Sử dụng AssetDatabase để load trực tiếp từ Assets folder (không cần Resources)
            string path = "Assets/Data/Config/Progression/RewardRateConfig.asset";
            _config = UnityEditor.AssetDatabase.LoadAssetAtPath<RewardRateConfigSO>(path);
        }

        [Test]
        public void RewardRateConfig_AssetExists()
        {
            Assert.IsNotNull(_config, "Không tìm thấy file RewardRateConfig tại: Assets/Data/Config/Progression/RewardRateConfig.asset");
        }

        [Test]
        public void RewardRateConfig_MinionEliteRates_SumTo100()
        {
            // Giả sử struct RankProbability có hàm IsValid như mình gợi ý trước đó
            Assert.IsTrue(_config.MinionArc1.IsValid, "Arc 1 Minion: Tổng tỷ lệ không bằng 100%");
            Assert.IsTrue(_config.MinionArc2.IsValid, "Arc 2 Minion: Tổng tỷ lệ không bằng 100%");
            Assert.IsTrue(_config.MinionArc3.IsValid, "Arc 3 Minion: Tổng tỷ lệ không bằng 100%");
            
            // elite
            Assert.IsTrue(_config.EliteArc1.IsValid, "Arc 1 Elite: Tổng tỷ lệ không bằng 100%");
            Assert.IsTrue(_config.EliteArc2.IsValid, "Arc 2 Elite: Tổng tỷ lệ không bằng 100%");
            Assert.IsTrue(_config.EliteArc3.IsValid, "Arc 3 Elite: Tổng tỷ lệ không bằng 100%");
        }

        [Test]
        public void RewardRateConfig_BossRates_SumTo100()
        {
            Assert.IsTrue(_config.BossArc1.IsValid, "Arc 1 Boss: Tổng tỷ lệ không bằng 100%");
            Assert.IsTrue(_config.BossArc2.IsValid, "Arc 2 Boss: Tổng tỷ lệ không bằng 100%");
            Assert.IsTrue(_config.BossArc3.IsValid, "Arc 3 Boss: Tổng tỷ lệ không bằng 100%");
        }
    }
}