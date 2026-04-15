using UnityEngine;
using Game.Logic.Shared.Structs;

namespace Game.Unity.Data.Definitions
{
    [CreateAssetMenu(fileName = "SP_NewSpell", menuName = "Wandering/Data/Spell Definition")]
    public class SpellDefinitionSO : ScriptableObject
    {
        public SpellDefinition data;
    }
}
