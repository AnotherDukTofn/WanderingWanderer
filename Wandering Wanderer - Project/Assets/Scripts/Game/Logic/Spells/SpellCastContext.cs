using Game.Logic.Shared.Structs;
using Game.Logic.Shared;

namespace Game.Logic.Spells
{
    public class SpellCastContext
    {
        public IEntity Caster { get; }
        public IEntity SelectedEnemy { get; }
        public SpellDefinition Spell { get; }

        public SpellCastContext(IEntity caster, IEntity selectedEnemy, SpellDefinition spell)
        {
            Caster = caster;
            SelectedEnemy = selectedEnemy;
            Spell = spell;
        }
    }
}
