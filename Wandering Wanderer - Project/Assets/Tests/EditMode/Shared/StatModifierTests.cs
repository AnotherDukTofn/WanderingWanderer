using NUnit.Framework;
using Game.Logic.Shared.Enums;
using Game.Logic.Shared.Structs;

namespace Tests.EditMode.Shared
{
    [TestFixture]
    public class StatModifierTests
    {
        [Test]
        public void StatModifier_Fields_StoreAndRetrieveCorrectValues()
        {
            // Arrange
            var stat = StatType.POT;
            var modType = ModType.Flat;
            var value = 10.5f;

            // Act
            var modifier = new StatModifier
            {
                stat = stat,
                modType = modType,
                value = value
            };

            // Assert
            Assert.AreEqual(stat, modifier.stat, "StatType mismatch");
            Assert.AreEqual(modType, modifier.modType, "ModType mismatch");
            Assert.AreEqual(value, modifier.value, "Value mismatch");
        }

        [Test]
        public void StatModifier_PercentAdd_StoresCorrectValue()
        {
            // Arrange
            var modifier = new StatModifier
            {
                stat = StatType.MaxHp,
                modType = ModType.Percent,
                value = 0.15f
            };

            // Assert
            Assert.AreEqual(StatType.MaxHp, modifier.stat);
            Assert.AreEqual(ModType.Percent, modifier.modType);
            Assert.AreEqual(0.15f, modifier.value);
        }
    }
}
