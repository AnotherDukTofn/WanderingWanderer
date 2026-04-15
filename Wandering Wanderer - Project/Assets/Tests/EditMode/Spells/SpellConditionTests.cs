using NUnit.Framework;
using Game.Logic.Spells;
using Game.Logic.Shared;
using Tests.EditMode.Stubs;

namespace Tests.EditMode.Spells
{
    public class SpellConditionTests
    {
        [Test]
        public void TargetHasEffect_Evaluate_TrueWhenHasEffect()
        {
            var target = new StubEntity();
            target.Effects.Add(EffectType.Burn);
            
            var cond = new TargetHasEffect { effectType = EffectType.Burn };
            var ctx = new SpellCastContext(new StubEntity(), target, default);

            Assert.IsTrue(cond.Evaluate(target, ctx));
        }

        [Test]
        public void TargetHasEffect_Evaluate_FalseWhenNoEffect()
        {
            var target = new StubEntity();
            
            var cond = new TargetHasEffect { effectType = EffectType.Burn };
            var ctx = new SpellCastContext(new StubEntity(), target, default);

            Assert.IsFalse(cond.Evaluate(target, ctx));
        }
    }
}
