using Game.Logic.Shared;

namespace Game.Logic.Combat
{
    public class CombatState
    {
        public IEntity Player { get; }
        public IEntity[] AliveEnemies { get; }

        public CombatState(IEntity player, IEntity[] aliveEnemies)
        {
            Player = player;
            AliveEnemies = aliveEnemies;
        }
    }
}
