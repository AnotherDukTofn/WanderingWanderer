using System;
using System.Linq;
using Game.Logic.Combat;
using Game.Logic.Spells;

namespace Game.Logic.Shared
{
    [Serializable]
    public struct TargetResolver
    {
        public TargetResolverType type;
        public int targetCount;
        public TieBreaker tieBreaker;
        public Element tieBreakerElement;

        public IEntity[] Resolve(SpellCastContext context, CombatState state)
        {
            switch (type)
            {
                case TargetResolverType.Caster:
                    return new[] { context.Caster };

                case TargetResolverType.SelectedEnemy:
                    return context.SelectedEnemy != null ? new[] { context.SelectedEnemy } : Array.Empty<IEntity>();

                case TargetResolverType.AllEnemies:
                    return state.AliveEnemies;

                case TargetResolverType.RandomEnemies:
                    if (targetCount <= 0) return Array.Empty<IEntity>();
                    return state.AliveEnemies
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(targetCount)
                        .ToArray();

                case TargetResolverType.LowestHpEnemies:
                    IOrderedEnumerable<IEntity> sorted = state.AliveEnemies.OrderBy(e => e.CurrentHp);
                    if (tieBreaker == TieBreaker.LowestResistance)
                    {
                        Element localElement = tieBreakerElement;
                        sorted = sorted.ThenBy(e => e.GetEffectiveResistance(localElement));
                    }
                    return sorted.Take(targetCount).ToArray();


                case TargetResolverType.SecondaryRandom:
                    var eligible = state.AliveEnemies.Where(e => e != context.SelectedEnemy).ToArray();
                    if (eligible.Length == 0) return Array.Empty<IEntity>();
                    
                    return eligible
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(1)
                        .ToArray();

                default:
                    return Array.Empty<IEntity>();
            }
        }
    }
}
