___
**Game:** Wandering Wanderer  
**Task:** TASK-008 — Tạo `EnemyDefinition`, `EquipmentDefinition`, `RuneDefinition` schema  
**Complexity:** S (few hours)  
**Depends:** TASK-005, TASK-007  
**Last Updated:** 15/04/2026  
___

# TASK-008 Guide

## Overview

TASK-008 requires you to create three core data definition ScriptableObject classes that define game entities' structure and capabilities. These are **pure data containers** (no behavior) that feed into Systems like `EquipmentSystem`, `RuneSystem`, and `EnemyController`.

---

## Dependencies

Before starting TASK-008, you must have:

- ✅ **TASK-005** — `StatModifier` struct and `StatType` enum complete
  - Need `StatModifier { StatType, ModType, float value }` and all stat types defined
  
- ✅ **TASK-007** — `SpellDefinition` class with 12 Rank I spells
  - Need to reference existing Spell assets by ID

---

## Part 1: `EnemyDefinition`

### Purpose

Define enemy properties (HP, potencies, resistances, spell pool, AI decision policy). Each enemy asset is one definition — no MonoBehaviour inheritance.

### Class Structure

```csharp
// Assets/Scripts/Logic/Data/EnemyDefinitionSO.cs
using UnityEngine;

public class EnemyDefinition : ScriptableObject
{
    public string id;                  // Unique identifier, e.g., "EN_FireMinion", "EN_IceElite"
    public string displayName;         // UI display name
    public EnemyType enemyType;        // Minion / Elite / Boss
    
    // Stats
    public int maxHp;
    
    // Potencies (base values before equipment/rune)
    public float baseFirePotency;
    public float baseWaterPotency;
    public float baseIcePotency;
    public float baseLightningPotency;
    
    // Resistances (base values)
    public float baseFireRes;
    public float baseWaterRes;
    public float baseIceRes;
    public float baseLightningRes;
    
    // Combat
    public SpellDefinition[] spells;             // Array of spell definitions this enemy can cast
    public DecisionPolicyType decisionPolicyType; // How enemy picks spells
    
    [SerializeReference]
    public IDecisionPolicyConfig decisionPolicyConfig;  // Type-specific config
}

public enum EnemyType
{
    Minion,  // 1 HP, weak spells
    Elite,   // ~2-3x HP, stronger spells
    Boss     // unique patterns
}

public enum DecisionPolicyType
{
    RandomPolicy,        // Pick random available spell each turn
    WeightedRandomPolicy, // Pick by weight distribution
    PriorityPolicy,      // Try priority list, fallback to random
    ScriptedPolicy       // Follow fixed sequence
}
```

### Acceptance Criteria (AC)

- [ ] `EnemyDefinition` is a `ScriptableObject` with all fields listed above
- [ ] `decisionPolicyConfig` uses `[SerializeReference]` allowing Inspector dropdown to select subtype
- [ ] File located at `Assets/Scripts/Logic/Data/EnemyDefinitionSO.cs`
- [ ] Create test asset `Assets/Data/Enemies/EN_TestMinion.asset`:
  - `enemyType: Minion`
  - `maxHp: 50`
  - Base stats: Fire potencies/resistances ≥ 0, non-negative
  - `spells: [SP_Fireball]` (drag-drop direct reference from TASK-007 asset)
  - `decisionPolicyType: RandomPolicy`
  - No serialize errors in Inspector
- [ ] Asset loads without error in Editor

### Decision Policy Interface

```csharp
// Assets/Scripts/Logic/Enemy/IDecisionPolicyConfig.cs
public interface IDecisionPolicyConfig { }

// RandomPolicy — no config needed
public class RandomPolicyConfig : IDecisionPolicyConfig { }

// WeightedRandomPolicy — configure spell weights
public class WeightedRandomPolicyConfig : IDecisionPolicyConfig
{
    [System.Serializable]
    public struct WeightedSpell
    {
        public SpellDefinition spell;
        public int weight;
    }
    
    public WeightedSpell[] weightedSpells;
}

// PriorityPolicy — ordered list, each rule points to a spell
public class PriorityPolicyConfig : IDecisionPolicyConfig
{
    [System.Serializable]
    public struct PriorityRule
    {
        public SpellDefinition spell;
        public int priority;        // 0 = highest priority
        public ConditionType condition;  // Which condition to check
    }
    
    public PriorityRule[] priorityRules;
}

// Condition types for priority-based decisions
public enum ConditionType
{
    Always,
    PlayerHpBelow50,
    PlayerHpBelow25,
    PlayerHasEffect,  // requires effectType parameter
    SelfHpBelow50
}

// ScriptedPolicy — fixed sequence
public class ScriptedPolicyConfig : IDecisionPolicyConfig
{
    public SpellDefinition[] sequence;  // spell sequence to repeat
}
```

---

## Part 2: `EquipmentDefinition`

### Purpose

Define equipment items (weapons, armor, accessories). Each equipment asset specifies stat modifiers it grants when equipped.

### Class Structure

```csharp
// Assets/Scripts/Logic/Data/EquipmentDefinitionSO.cs
using UnityEngine;

public class EquipmentDefinition : ScriptableObject
{
    public string id;              // Unique ID, e.g., "EQ_FlameWand_R1", "EQ_IronRobe_R2"
    public string displayName;     // UI display name
    public EquipmentSlot slot;     // Which slot: Staff, Ring, Book, Garb, Boots
    public Rank rank;              // I, II, or III
    
    [SerializeField]
    public StatModifier[] modifiers;  // Stat bonuses when equipped
}

public enum EquipmentSlot
{
    Staff,  // Weapon — focus POT
    Ring,   // Accessory — focus SPI
    Book,   // Off-hand — focus WIS
    Garb,   // Armor — focus VIT
    Boots   // Footwear — focus AGI
}

public enum Rank
{
    I,    // Basic
    II,   // Intermediate
    III   // Advanced
}
```

### Stat Modifier Count By Rank

|Rank|Max Modifiers|Notes|
|---|---|---|
|I|1|Single flat modifier, simple|
|II|2|Two flat modifiers, or one high-value modifier|
|III|3|Can include Percent modifiers here|

### Acceptance Criteria (AC)

- [ ] `EquipmentDefinition` ScriptableObject created at `Assets/Scripts/Logic/Data/EquipmentDefinitionSO.cs`
- [ ] `slot` is an `EquipmentSlot` enum — all 5 values available
- [ ] `rank` is a `Rank` enum with I, II, III
- [ ] `modifiers` is a serializable array of `StatModifier` (from TASK-005)
- [ ] Create test asset `Assets/Data/Equipment/EQ_FlameWand_R1.asset`:
  - `slot: Staff`
  - `rank: Rank.I`
  - `modifiers: [{ stat: POT, modType: Flat, value: 5 }]`
  - No serialize errors
- [ ] Create test asset `Assets/Data/Equipment/EQ_IronRobe_R2.asset`:
  - `slot: Garb`
  - `rank: Rank.II`
  - `modifiers: [{ stat: VIT, modType: Flat, value: 8 }, { stat: MaxHp, modType: Flat, value: 40 }]`
  - No serialize errors
- [ ] Assets load without error in Editor

---

## Part 3: `RuneDefinition`

### Purpose

Define Rune passive items. Runes grant stat modifiers OR trigger-based effects when embedded in sockets. Each rune has a passive type determining how it works.

### Class Structure

```csharp
// Assets/Scripts/Logic/Data/RuneDefinitionSO.cs
using UnityEngine;

public class RuneDefinition : ScriptableObject
{
    public string id;              // Unique ID, e.g., "RU_FireEmbrace_R1"
    public string displayName;     // UI display name
    public Rank rank;              // I, II, or III
    
    public RunePassiveType passiveType;  // Type of passive effect
    
    [SerializeField]
    public StatModifier[] statModifiers;  // Used when passiveType == StatModifier
    
    [SerializeReference]
    public object passiveConfig;   // Type-specific config for trigger/hook runes
}

public enum RunePassiveType
{
    StatModifier,        // Pure stat bonus (no behavior)
    ConditionalTrigger,  // Trigger on condition, execute action
    TurnHook             // Trigger on turn event
}
```

### Passive Config Types

For `ConditionalTrigger` and `TurnHook` runes, you'll need configs:

```csharp
// For use with [SerializeReference] in passiveConfig
public interface IRunePassiveConfig { }

// ConditionalTrigger — react to effect being applied
public class ConditionalTriggerConfig : IRunePassiveConfig
{
    public EffectType triggerEffect;   // Which effect to watch for
    public string actionType;           // e.g., "HealSelf", "ApplyEffect"
    public float actionValue;           // percentage or amount
}

// TurnHook — react to turn event
public class TurnHookConfig : IRunePassiveConfig
{
    public string triggerPhase;         // e.g., "EndPhase", "StartPhase"
    public string actionType;
    public float actionValue;
}
```

### Acceptance Criteria (AC)

- [ ] `RuneDefinition` ScriptableObject created at `Assets/Scripts/Logic/Data/RuneDefinitionSO.cs`
- [ ] `rank` is a `Rank` enum (I, II, III)
- [ ] `passiveType` is a `RunePassiveType` enum
- [ ] `statModifiers` array serialized when passiveType == StatModifier
- [ ] `passiveConfig` uses `[SerializeReference]` for type-specific configs
- [ ] Create test asset `Assets/Data/Runes/RU_FireEmbrace_R1.asset`:
  - `rank: Rank.I`
  - `passiveType: RunePassiveType.StatModifier`
  - `statModifiers: [{ stat: FirePot, modType: Flat, value: 10 }]`
  - No serialize errors
- [ ] Asset loads without error in Editor

---

## Implementation Checklist

### Step 1: Create Enemy Definition

1. Create file: `Assets/Scripts/Logic/Data/EnemyDefinitionSO.cs`
2. Implement `EnemyDefinition` class with all fields
3. Create file: `Assets/Scripts/Logic/Enemy/IDecisionPolicyConfig.cs`
4. Implement interface and 4 policy config classes
5. Create folder: `Assets/Data/Enemies/`
6. Right-click → Create → EnemyDefinition → name it `EN_TestMinion`
7. Verify Inspector shows all fields without errors
7. Drag-drop `SP_Fireball` asset (from TASK-007) into the `spells` array

### Step 2: Create Equipment Definition

1. Create file: `Assets/Scripts/Logic/Data/EquipmentDefinitionSO.cs`
2. Implement `EquipmentDefinition` class
3. Create enums: `EquipmentSlot` (5 values), `Rank` (I, II, III)
4. Create folder: `Assets/Data/Equipment/`
5. Right-click → Create → EquipmentDefinition → create two test assets:
   - `EQ_FlameWand_R1` (1 POT boost)
   - `EQ_IronRobe_R2` (VIT + MaxHp boost)
6. Verify Inspector shows modifiers array correctly

### Step 3: Create Rune Definition

1. Create file: `Assets/Scripts/Logic/Data/RuneDefinitionSO.cs`
2. Implement `RuneDefinition` class
3. Create enum: `RunePassiveType` (StatModifier, ConditionalTrigger, TurnHook)
4. Create file: `Assets/Scripts/Logic/Rune/IRunePassiveConfig.cs`
5. Implement interface and config classes
6. Create folder: `Assets/Data/Runes/`
7. Right-click → Create → RuneDefinition → create test asset:
   - `RU_FireEmbrace_R1` (FirePot boost)
8. Verify Inspector shows [SerializeReference] dropdown for passiveConfig

---

## Common Issues & Solutions

### Issue: "Script doesn't have [SerializeReference]"

**Cause:** Forgot `[SerializeReference]` on the field.

**Fix:**
```csharp
// ❌ Wrong
public IDecisionPolicyConfig decisionPolicyConfig;

// ✅ Correct
[SerializeReference]
public IDecisionPolicyConfig decisionPolicyConfig;
```

### Issue: Can't see concrete types in Inspector dropdown

**Cause:** Classes don't inherit from interface or aren't in correct namespace.

**Fix:**
```csharp
// ✅ Ensure concrete class inherits interface
public class RandomPolicyConfig : IDecisionPolicyConfig { }
```

### Issue: `StatModifier` array shows as hash code, not readable

**Cause:** `StatModifier` isn't `[Serializable]`.

**Fix:** Make sure from TASK-005 that `StatModifier` has `[Serializable]` attribute.

---

## Testing Verification

### By Inspector Visual Check

1. Open `EN_TestMinion` asset in Inspector
   - All fields populated ✓
   - No red errors ✓
   - spellIds array shows "SP_Fireball" ✓
   - decisionPolicyConfig shows a dropdown or object ✓

2. Open `EQ_FlameWand_R1` in Inspector
   - Shows slot = Staff ✓
   - Shows rank = I ✓
   - modifiers array has 1 element: POT Flat +5 ✓

3. Open `RU_FireEmbrace_R1` in Inspector
   - Shows rank = I ✓
   - Shows passiveType = StatModifier ✓
   - statModifiers shows 1 element: FirePot Flat +10 ✓

### By Play Mode (optional, if you want to load data)

```csharp
// Add to a test MonoBehaviour if you want to verify loading
void Start()
{
    var enemyDef = Resources.Load<EnemyDefinition>("Enemies/EN_TestMinion");
    Debug.Log($"Loaded: {enemyDef.displayName}, HP: {enemyDef.maxHp}");
    
    var equipmentDef = Resources.Load<EquipmentDefinition>("Equipment/EQ_FlameWand_R1");
    Debug.Log($"Loaded: {equipmentDef.displayName}, Slot: {equipmentDef.slot}");
    
    var runeDef = Resources.Load<RuneDefinition>("Runes/RU_FireEmbrace_R1");
    Debug.Log($"Loaded: {runeDef.displayName}, Type: {runeDef.passiveType}");
}
```

---

## Naming Conventions

Refer to [TDD — NAMING_CONVENTION.md](TDD/TDD%20-%20NAMING_CONVENTION.md) for full conventions.

### File Paths

```
Assets/Scripts/Logic/Data/
  ├── EnemyDefinitionSO.cs
  ├── EquipmentDefinitionSO.cs
  └── RuneDefinitionSO.cs

Assets/Scripts/Logic/Enemy/
  └── IDecisionPolicyConfig.cs

Assets/Scripts/Logic/Rune/
  └── IRunePassiveConfig.cs

Assets/Data/
  ├── Enemies/
  │   └── EN_TestMinion.asset
  ├── Equipment/
  │   ├── EQ_FlameWand_R1.asset
  │   └── EQ_IronRobe_R2.asset
  └── Runes/
      └── RU_FireEmbrace_R1.asset
```

### Naming Rules for Assets

- Enemy: `EN_` prefix + name + rank if any, e.g., `EN_FireMinion`, `EN_FrostElite`, `EN_Overlord`
- Equipment: `EQ_` prefix + name + `_R` + rank, e.g., `EQ_FlameWand_R1`, `EQ_IronRobe_R2`
- Rune: `RU_` prefix + name + `_R` + rank, e.g., `RU_FireEmbrace_R1`

---

## Architecture Context (From DETAILED_ARCHITECTURE.md)

### Why SpellDefinition[] instead of string[] IDs?

The original architecture paper mentioned `SpellID[]` (string array). We're using **direct `SpellDefinition[]` references** instead because:

✅ **Type-safe** — Compiler catches missing/broken references automatically  
✅ **No resolver layer needed** — Enemy AI works directly with SpellDefinition objects  
✅ **Inspector-friendly** — Drag-drop visual reference instead of string ID entry  
✅ **Better performance** — No runtime string lookup overhead  
✅ **Refactoring-proof** — Unity auto-updates when you rename Spell assets  

This aligns with the project's data-driven design philosophy: strong typing, pure data assets, and minimal plumbing code.

---

### Why separate into three definitions?

1. **Data ↔️ Behavior separation** — Definition assets contain *only* data, no MonoBehaviour. Systems (`EnemyController`, `EquipmentSystem`, `RuneSystem`) read these assets.

2. **Balancing without recompile** — Edit stat modifiers in Inspector, change policy config, no C# rebuild needed.

3. **Reusability** — Multiple enemies can use the same decision policy by reference.

### How data flows into systems

```
EnemyDefinition asset (with SpellDefinition[] references)
  ↓
EnemyController.Initialize(enemyDef)
  ├── Create HP, potencies, resistances from definition
  ├── Build EffectSystem
  └── Initialize DecisionPolicy from decisionPolicyConfig

// SpellSelector directly works with SpellDefinition objects
// No ID lookup needed — type-safe reference via Inspector
```

---

## Next Steps After TASK-008

Once Task-008 is complete (all 3 AC groups passing):

1. **TASK-009** — Foundation Layer (`EventChannelSO`, `StateMachineManager`)
2. **TASK-020** — `ArmorStack` implementation
3. **TASK-023** — `PlayerController` HP/MP scaling with stat application

Your data definitions will be consumed by these systems in later tasks.

---

## Questions?

Refer to:
- [DETAILED_ARCHITECTURE.md](TDD/TDD%20-%20DETAILED_ARCHITECTURE.md) — sections 8, 10, 12
- [TASK_MANAGER.md](TDD/TASK_MANAGER.md) — TASK-005, TASK-007 definitions
- CLAUDE.md or AGENTS.md for setup guidance
