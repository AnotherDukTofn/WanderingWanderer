using NUnit.Framework;
using Game.Logic.Shared;
using Game.Logic.Combat;
using Game.Logic.Spells;
using Tests.EditMode.Stubs;

namespace Tests.EditMode.Spells
{
    public class TargetResolverTests
    {
        [Test]
        public void Resolve_AllEnemies_ReturnsAll()
        {
            var e1 = new StubEntity();
            var e2 = new StubEntity();
            var e3 = new StubEntity();
            var state = new CombatState(new StubEntity(), new IEntity[] { e1, e2, e3 });
            
            var resolver = new TargetResolver { type = TargetResolverType.AllEnemies };
            var result = resolver.Resolve(new SpellCastContext(new StubEntity(), e1, default), state);

            Assert.AreEqual(3, result.Length);
            Assert.Contains(e1, result);
            Assert.Contains(e2, result);
            Assert.Contains(e3, result);
        }

        [Test]
        public void Resolve_RandomEnemies_ReturnsCorrectCountUnique()
        {
            var enemies = new IEntity[] { new StubEntity(), new StubEntity(), new StubEntity(), new StubEntity() };
            var state = new CombatState(new StubEntity(), enemies);
            
            var resolver = new TargetResolver { type = TargetResolverType.RandomEnemies, targetCount = 2 };
            var result = resolver.Resolve(new SpellCastContext(new StubEntity(), enemies[0], default), state);

            Assert.AreEqual(2, result.Length);
            Assert.AreNotEqual(result[0], result[1]);
        }

        [Test]
        public void Resolve_LowestHpEnemies_TieBreakWithResistance()
        {
            var e1 = new StubEntity { CurrentHp = 10, Resistance = 50f }; 
            var e2 = new StubEntity { CurrentHp = 5, Resistance = 10f }; 
            var e3 = new StubEntity { CurrentHp = 5, Resistance = 5f }; // Tie but lower res
            var state = new CombatState(new StubEntity(), new IEntity[] { e1, e2, e3 });
            
            var resolver = new TargetResolver 
            { 
                type = TargetResolverType.LowestHpEnemies, 
                targetCount = 2,
                tieBreaker = TieBreaker.LowestResistance
            };
            
            var result = resolver.Resolve(new SpellCastContext(new StubEntity(), e1, default), state);

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(e3, result[0]); // Lowest HP + Lowest Res
            Assert.AreEqual(e2, result[1]); // Lowest HP + Higher Res
        }

        [Test]
        public void Resolve_SecondaryRandom_ExcludesSelectedEnemy()
        {
            var selected = new StubEntity();
            var other1 = new StubEntity();
            var other2 = new StubEntity();
            var state = new CombatState(new StubEntity(), new IEntity[] { selected, other1, other2 });
            
            var resolver = new TargetResolver { type = TargetResolverType.SecondaryRandom };
            var result = resolver.Resolve(new SpellCastContext(new StubEntity(), selected, default), state);

            Assert.AreEqual(1, result.Length);
            Assert.AreNotEqual(selected, result[0]);
            Assert.IsTrue(result[0] == other1 || result[0] == other2);
        }
    }
}
