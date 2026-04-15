using UnityEngine;
using UnityEditor;
using Game.Unity.Data;
using Game.Logic.Shared;
using Game.Logic.Spells;

public class SpellDataGenerator
{
    [MenuItem("Wandering/Generate Rank I Spells")]
    public static void GenerateRank1Spells()
    {
        string dir = "Assets/Data/Spells/RankI";
        
        if (!AssetDatabase.IsValidFolder("Assets/Data")) AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Data/Spells")) AssetDatabase.CreateFolder("Assets/Data", "Spells");
        if (!AssetDatabase.IsValidFolder("Assets/Data/Spells/RankI")) AssetDatabase.CreateFolder("Assets/Data/Spells", "RankI");

        // Fireball
        CreateSpell(dir, "SP_Fireball", "Fireball", Rank.I, Element.Fire, 1, 1, 0, new SpellEffect[]
        {
            new DamageEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, potencyRef = new PotencyRef { element = Element.Fire, coefficient = 0.8f } },
            new StatusEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, effectType = EffectType.Burn }
        });

        // Ignite
        CreateSpell(dir, "SP_Ignite", "Ignite", Rank.I, Element.Fire, 1, 1, 0, new SpellEffect[]
        {
            new StatusEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, effectType = EffectType.Burn }
        });

        // Candle Ghost
        CreateSpell(dir, "SP_CandleGhost", "Candle Ghost", Rank.I, Element.Fire, 1, 1, 0, new SpellEffect[]
        {
            new DamageEffect { targetResolver = new TargetResolver { type = TargetResolverType.LowestHpEnemies, targetCount = 3, tieBreaker = TieBreaker.LowestResistance, tieBreakerElement = Element.Fire }, potencyRef = new PotencyRef { element = Element.Fire, coefficient = 0.5f } }
        });

        // Heal
        CreateSpell(dir, "SP_Heal", "Heal", Rank.I, Element.Water, 1, 1, 0, new SpellEffect[]
        {
            new HealEffect { targetResolver = new TargetResolver { type = TargetResolverType.Caster }, potencyRef = new PotencyRef { element = Element.Water, coefficient = 1f } }
        });

        // Splash
        CreateSpell(dir, "SP_Splash", "Splash", Rank.I, Element.Water, 1, 1, 0, new SpellEffect[]
        {
            new DamageEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, potencyRef = new PotencyRef { element = Element.Water, coefficient = 0.6f } },
            new StatusEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, effectType = EffectType.Drenched }
        });

        // Bubble
        CreateSpell(dir, "SP_Bubble", "Bubble", Rank.I, Element.Water, 1, 1, 0, new SpellEffect[]
        {
            new StatusEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, effectType = EffectType.Frozen }
        });
        
        // IceShard
        CreateSpell(dir, "SP_IceShard", "Ice Shard", Rank.I, Element.Ice, 1, 1, 0, new SpellEffect[]
        {
            new DamageEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, potencyRef = new PotencyRef { element = Element.Ice, coefficient = 0.7f } },
            new StatusEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, effectType = EffectType.Chilled }
        });

        // IceShield
        CreateSpell(dir, "SP_IceShield", "Ice Shield", Rank.I, Element.Ice, 1, 1, 0, new SpellEffect[]
        {
            new ArmorEffect { targetResolver = new TargetResolver { type = TargetResolverType.Caster }, potencyRef = new PotencyRef { element = Element.Ice, coefficient = 0.5f }, duration = 3 }
        });

        // ColdBreathe
        CreateSpell(dir, "SP_ColdBreathe", "Cold Breathe", Rank.I, Element.Ice, 1, 1, 0, new SpellEffect[]
        {
            new DamageEffect { targetResolver = new TargetResolver { type = TargetResolverType.AllEnemies }, potencyRef = new PotencyRef { element = Element.Ice, coefficient = 0.4f } }
        });

        // Shock
        CreateSpell(dir, "SP_Shock", "Shock", Rank.I, Element.Lightning, 1, 1, 0, new SpellEffect[]
        {
            new DamageEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, potencyRef = new PotencyRef { element = Element.Lightning, coefficient = 0.7f } },
            new StatusEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, effectType = EffectType.Dazed }
        });

        // Spark
        CreateSpell(dir, "SP_Spark", "Spark", Rank.I, Element.Lightning, 1, 1, 0, new SpellEffect[]
        {
            new DamageEffect { targetResolver = new TargetResolver { type = TargetResolverType.SelectedEnemy }, potencyRef = new PotencyRef { element = Element.Lightning, coefficient = 0.6f } },
            new DamageEffect { targetResolver = new TargetResolver { type = TargetResolverType.SecondaryRandom }, potencyRef = new PotencyRef { element = Element.Lightning, coefficient = 0.3f } }
        });

        // Pulse
        CreateSpell(dir, "SP_Pulse", "Pulse", Rank.I, Element.Lightning, 1, 1, 0, new SpellEffect[]
        {
            new DamageEffect { targetResolver = new TargetResolver { type = TargetResolverType.RandomEnemies, targetCount = 3 }, potencyRef = new PotencyRef { element = Element.Lightning, coefficient = 0.5f } }
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generate 12 Rank I Spells Completed!");
    }

    private static void CreateSpell(string dir, string assetName, string displayName, Rank rank, Element element, int cost, int cd, int minWisdom, SpellEffect[] effects)
    {
        string path = $"{dir}/{assetName}.asset";
        SpellDefinitionSO so = AssetDatabase.LoadAssetAtPath<SpellDefinitionSO>(path);
        bool created = false;
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<SpellDefinitionSO>();
            created = true;
        }

        so.data = new SpellDefinition
        {
            id = assetName,
            displayName = displayName,
            rank = rank,
            element = element,
            baseCost = cost,
            baseCooldown = cd,
            minWisdomToImprint = minWisdom,
            effects = effects
        };

        if (created)
        {
            AssetDatabase.CreateAsset(so, path);
        }
        else
        {
            EditorUtility.SetDirty(so);
        }
    }
}
