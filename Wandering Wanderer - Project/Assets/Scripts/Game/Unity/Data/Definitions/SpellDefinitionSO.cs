using UnityEngine;
using Game.Logic.Shared;

namespace Game.Unity.Data
{
    [CreateAssetMenu(fileName = "SP_NewSpell", menuName = "Wandering/Data/Spell Definition")]
    public class SpellDefinitionSO : ScriptableObject
    {
        public SpellDefinition data;
    }
}
