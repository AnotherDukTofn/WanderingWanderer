
___
 **Game:** Wandering Wanderer
 **Author:** DukTofn 
 **Last Updated:** 09/04/2026
 ___

# TDD — Naming Convention & Folder Structure

1. [Folder Structure](#1-folder-structure)
2. [C# Naming Convention](#2-c-naming-convention)
3. [Asset Naming Convention](#3-asset-naming-convention)
4. [Scene Naming](#4-scene-naming)
5. [Quick Reference Card](#5-quick-reference-card)

---

## 1. Folder Structure

### 1.1. Cấu trúc cấp 1

```
Assets/
  ├── Data/           ← ScriptableObject assets (config, definitions)
  ├── Prefabs/        ← Prefab của GameObject
  ├── Scenes/         ← Scene files
  ├── Scripts/        ← C# source code
  ├── Sprites/        ← 2D sprites và textures
  ├── Audio/          ← Sound effects và music
  ├── Fonts/          ← Font files
  ├── Materials/      ← Material assets
  ├── Animations/     ← Animation clips và controllers
  ├── VFX/            ← Particle systems và VFX
  ├── Resources/      ← Chỉ những gì cần Resources.Load() (config SO)
  └── Tests/          ← Test files
```

> **Quy tắc `Resources/`:** Chỉ đặt vào đây những SO cần load trong Edit Mode test (`Resources.Load<T>()`). ScriptableObject còn lại inject qua Inspector. **Không đặt Prefab hay Scene vào Resources.**

---

### 1.2. `Data/` — chi tiết

```
Data/
  ├── Config/
  │     ├── CombatConfig.asset
  │     ├── ArcConfig_1.asset
  │     ├── ArcConfig_2.asset
  │     ├── ArcConfig_3.asset
  │     ├── ShopPriceConfig.asset
  │     ├── RewardRateConfig.asset
  │     ├── EventConfig.asset
  │     └── WisdomSlotConfig.asset
  ├── Spells/
  │     ├── RankI/
  │     │     ├── SP_Fireball.asset
  │     │     ├── SP_Ignite.asset
  │     │     ├── SP_CandleGhost.asset
  │     │     ├── SP_Heal.asset
  │     │     ├── SP_Splash.asset
  │     │     ├── SP_Bubble.asset
  │     │     ├── SP_IceShard.asset
  │     │     ├── SP_IceShield.asset
  │     │     ├── SP_ColdBreathe.asset
  │     │     ├── SP_Shock.asset
  │     │     ├── SP_Spark.asset
  │     │     └── SP_Pulse.asset
  │     ├── RankII/
  │     └── RankIII/
  ├── Enemies/
  │     ├── Minions/
  │     │     └── EN_TestMinion.asset
  │     ├── Elites/
  │     └── Bosses/
  ├── Equipment/
  │     ├── RankI/
  │     ├── RankII/
  │     └── RankIII/
  └── Runes/
        ├── RankI/
        ├── RankII/
        └── RankIII/
```

---

### 1.3. `Scripts/` — chi tiết

```
Scripts/
  ├── Logic/                    ← Pure C#, KHÔNG reference UnityEngine (Assembly: Logic)
  │     ├── Combat/
  │     │     ├── ArmorStack.cs
  │     │     ├── CombatResolver.cs
  │     │     ├── DamageCalculator.cs
  │     │     ├── EffectSystem.cs
  │     │     ├── HitDodgeResolver.cs
  │     │     └── PhaseHandler.cs
  │     ├── Entities/
  │     │     ├── PlayerController.cs
  │     │     ├── EnemyController.cs
  │     │     └── AI/
  │     │           ├── CombatContext.cs
  │     │           ├── SpellSelector.cs
  │     │           ├── IDecisionPolicy.cs
  │     │           ├── RandomPolicy.cs
  │     │           ├── WeightedRandomPolicy.cs
  │     │           ├── PriorityPolicy.cs
  │     │           └── ScriptedPolicy.cs
  │     ├── Spells/
  │     │     ├── SpellCaster.cs
  │     │     ├── SpellSlotManager.cs
  │     │     └── CooldownTracker.cs
  │     ├── Passives/
  │     │     ├── EquipmentSystem.cs
  │     │     ├── RuneSystem.cs
  │     │     └── IRunePassive.cs
  │     ├── Meta/
  │     │     ├── GoldLedger.cs
  │     │     ├── RewardSystem.cs
  │     │     ├── ShopSystem.cs
  │     │     ├── EventSystem.cs
  │     │     ├── RestNode.cs
  │     │     └── SaveSystem.cs
  │     ├── Map/
  │     │     ├── MapSystem.cs
  │     │     └── NodeRouter.cs
  │     ├── Core/
  │     │     ├── GameManager.cs
  │     │     └── TurnManager.cs
  │     └── Shared/
  │           ├── Enums.cs            ← Element, EffectType, NodeType, Phase...
  │           ├── Interfaces.cs       ← IEntity, IActionCommand...
  │           └── Constants.cs        ← magic number tạm thời (nên chuyển sang SO)
  │
  ├── Unity/                    ← MonoBehaviour, Unity-specific (Assembly: Unity)
  │     ├── Presentation/
  │     │     ├── VisualQueue.cs
  │     │     └── Commands/
  │     │           ├── PlaySpellAnimCommand.cs
  │     │           ├── PlayDamageAnimCommand.cs
  │     │           ├── ShowDamageNumberCommand.cs
  │     │           ├── ShowHpGainCommand.cs
  │     │           ├── PlayEffectApplyAnimCommand.cs
  │     │           ├── PlayDetonatesAnimCommand.cs
  │     │           └── PlayDeathAnimCommand.cs
  │     ├── UI/
  │     │     ├── Combat/
  │     │     │     ├── PlayerStatusView.cs
  │     │     │     ├── EnemyStatusView.cs
  │     │     │     ├── EffectView.cs
  │     │     │     ├── SpellBarView.cs
  │     │     │     └── TurnIndicatorView.cs
  │     │     ├── Map/
  │     │     │     ├── MapGraphView.cs
  │     │     │     ├── NodeView.cs
  │     │     │     └── PendingModifierView.cs
  │     │     ├── Shop/
  │     │     │     ├── ShopInventoryView.cs
  │     │     │     ├── ShopServiceView.cs
  │     │     │     └── PlayerEquipmentView.cs
  │     │     ├── Reward/
  │     │     │     └── RewardChoiceView.cs
  │     │     ├── Event/
  │     │     │     └── EventView.cs
  │     │     └── Rest/
  │     │           └── RestView.cs
  │     ├── Data/
  │     │     ├── CombatConfig.cs       ← ScriptableObject class
  │     │     ├── ArcConfig.cs
  │     │     ├── ShopPriceConfig.cs
  │     │     ├── RewardRateConfig.cs
  │     │     ├── EventConfig.cs
  │     │     ├── WisdomSlotConfig.cs
  │     │     ├── SpellDefinition.cs
  │     │     ├── EnemyDefinition.cs
  │     │     └── RuneDefinition.cs
  │     └── Bootstrap/
  │           └── ConfigLoader.cs       ← MonoBehaviour inject SO vào runtime
  │
  └── Editor/                   ← Editor-only tools (không build vào game)
        └── MapDebugWindow.cs   ← Custom editor để visualize map graph
```

---

### 1.4. `Tests/` — chi tiết

```
Tests/
  ├── EditMode/
  │     ├── Tests.EditMode.asmdef
  │     ├── Combat/
  │     │     ├── ArmorStackTests.cs
  │     │     ├── DamageCalculatorTests.cs
  │     │     ├── HitDodgeResolverTests.cs
  │     │     ├── EffectSystemTests.cs
  │     │     └── CombatResolverTests.cs
  │     ├── Entities/
  │     │     ├── PlayerControllerTests.cs
  │     │     └── AI/
  │     │           ├── SpellSelectorTests.cs
  │     │           ├── RandomPolicyTests.cs
  │     │           ├── PriorityPolicyTests.cs
  │     │           └── WeightedRandomPolicyTests.cs
  │     ├── Spells/
  │     │     ├── SpellCasterValidationTests.cs
  │     │     ├── SpellSlotManagerTests.cs
  │     │     └── CooldownTrackerTests.cs
  │     ├── Passives/
  │     │     ├── EquipmentSystemTests.cs
  │     │     └── RuneSystemTests.cs
  │     ├── Meta/
  │     │     ├── GoldLedgerTests.cs
  │     │     ├── RewardSystemTests.cs
  │     │     ├── ShopSystemTests.cs
  │     │     ├── EventSystemTests.cs
  │     │     └── SaveSystemTests.cs
  │     ├── Map/
  │     │     └── MapSystemTests.cs
  │     └── Stubs/
  │           ├── StubEntity.cs
  │           ├── StubEffectSystem.cs
  │           └── StubVisualQueue.cs
  └── PlayMode/
        ├── Tests.PlayMode.asmdef
        ├── Combat/
        │     ├── TurnManagerTests.cs
        │     ├── PhaseHandlerTests.cs
        │     └── SpellCasterFlowTests.cs
        ├── Presentation/
        │     └── VisualQueueTests.cs
        └── UI/
              ├── PlayerStatusViewTests.cs
              └── SpellBarViewTests.cs
```

---

### 1.5. `Prefabs/` — chi tiết

```
Prefabs/
  ├── Combat/
  │     ├── Player.prefab
  │     ├── Enemies/
  │     │     ├── EN_TestMinion.prefab
  │     │     └── ...
  │     └── UI/
  │           ├── FloatingText.prefab
  │           └── DamageNumber.prefab
  ├── Map/
  │     ├── NodeView_Minion.prefab
  │     ├── NodeView_Elite.prefab
  │     ├── NodeView_Boss.prefab
  │     ├── NodeView_Shop.prefab
  │     ├── NodeView_Rest.prefab
  │     └── NodeView_Event.prefab
  └── VFX/
        ├── VFX_Burn.prefab
        ├── VFX_Frozen.prefab
        ├── VFX_Detonates.prefab
        └── VFX_Crystalize.prefab
```

---

## 2. C# Naming Convention

### 2.1. Classes và Interfaces

|Loại|Convention|Ví dụ|
|---|---|---|
|Class|PascalCase|`DamageCalculator`, `ArmorStack`|
|Interface|`I` + PascalCase|`IEntity`, `IRunePassive`, `IDecisionPolicy`|
|Abstract class|PascalCase (không prefix)|`BaseView`, `BaseCommand`|
|Enum|PascalCase|`EffectType`, `Element`, `NodeType`|
|Enum value|PascalCase|`EffectType.Burn`, `Element.Fire`|
|ScriptableObject class|PascalCase + `Config` hoặc `Definition`|`CombatConfig`, `SpellDefinition`|

### 2.2. Methods

|Loại|Convention|Ví dụ|
|---|---|---|
|Public method|PascalCase|`TakeDamage()`, `ApplyArmor()`|
|Private method|PascalCase|`CalculateOverflow()`|
|Test method|`MethodName_Condition_ExpectedResult`|`TakeDamage_LessThanArmor_ReturnsZeroOverflow`|
|Coroutine|PascalCase + `Routine` suffix (optional)|`DrainQueueRoutine()`|
|Event handler|`On` + EventName|`OnHpChanged()`, `OnEffectApplied()`|

### 2.3. Fields và Properties

|Loại|Convention|Ví dụ|
|---|---|---|
|Public field|camelCase (tránh dùng, dùng property thay)|—|
|Private field|`_` + camelCase|`_currentHp`, `_activeEffects`|
|`[SerializeField]` private|`_` + camelCase|`[SerializeField] private CombatConfig _config;`|
|Public property (get)|PascalCase|`CurrentHp`, `MaxHp`|
|Public property (get/set)|PascalCase|`CrystalizeFlag`|
|Static field|`s_` + camelCase|`s_instance`|
|Const|SCREAMING_SNAKE_CASE|`MAX_ARMOR_STACKS`|

```csharp
// Ví dụ đúng
public class PlayerController
{
    [SerializeField] private CombatConfig _config;
    
    private int _currentHp;
    private int _maxHp;
    
    public int CurrentHp => _currentHp;
    public int MaxHp => _maxHp;
    
    public event Action<int, int> OnHpChanged;
    
    public void TakeDamage(float amount)
    {
        _currentHp = Mathf.Max(0, _currentHp - (int)amount);
        OnHpChanged?.Invoke(_currentHp, _maxHp);
    }
    
    private int CalculateMaxHp(int vit)
    {
        return (int)(_config.HpCap * vit / (vit + _config.HpHalf));
    }
}
```

### 2.4. Events

|Loại|Convention|Ví dụ|
|---|---|---|
|Event field|`On` + VerbPhrase|`OnHpChanged`, `OnEffectApplied`|
|Event delegate type|PascalCase|`Action<int, int>` hoặc custom `HpChangedHandler`|

```csharp
// Đúng
public event Action<int, int> OnHpChanged;
public event Action<EffectType> OnEffectApplied;
public event Action<ComboType, IEntity> OnCombinationTriggered;

// Invoke an toàn (null check)
OnHpChanged?.Invoke(_currentHp, _maxHp);
```

### 2.5. Namespaces

```csharp
// Logic layer
namespace WanderingWanderer.Logic.Combat { }
namespace WanderingWanderer.Logic.Entities { }
namespace WanderingWanderer.Logic.Entities.AI { }
namespace WanderingWanderer.Logic.Spells { }
namespace WanderingWanderer.Logic.Passives { }
namespace WanderingWanderer.Logic.Meta { }
namespace WanderingWanderer.Logic.Map { }
namespace WanderingWanderer.Logic.Core { }
namespace WanderingWanderer.Logic.Shared { }

// Unity layer
namespace WanderingWanderer.Unity.Presentation { }
namespace WanderingWanderer.Unity.UI.Combat { }
namespace WanderingWanderer.Unity.UI.Map { }
namespace WanderingWanderer.Unity.Data { }
```

### 2.6. Những thứ cần tránh

```csharp
// ❌ Tránh
public int hp;                      // public field không có property
private int currentHp;              // thiếu underscore prefix
void update() { }                   // lowercase Unity message
class effectSystem { }              // lowercase class
const int maxDodge = 50;            // const không SCREAMING_SNAKE
GameObject go;                      // tên quá tắt, không rõ nghĩa
void DoStuff() { }                  // tên không mô tả

// ✅ Đúng
public int CurrentHp => _currentHp;
private int _currentHp;
void Update() { }
class EffectSystem { }
const int MAX_DODGE = 50;
GameObject _targetGameObject;
void ApplyBurnDamage() { }
```

---

## 3. Asset Naming Convention

### 3.1. ScriptableObject assets

|Loại|Prefix|Ví dụ|
|---|---|---|
|Spell Definition|`SP_`|`SP_Fireball.asset`, `SP_IceShield.asset`|
|Enemy Definition|`EN_`|`EN_TestMinion.asset`, `EN_FireBoss.asset`|
|Equipment Definition|`EQ_`|`EQ_FireStaff_R1.asset`|
|Rune Definition|`RU_`|`RU_BurnEcho_R2.asset`|
|Config SO|Tên mô tả, không prefix|`CombatConfig.asset`, `ArcConfig_1.asset`|

**Rank trong tên asset (Equipment và Rune):**

```
EQ_FireStaff_R1.asset    ← Rank I
EQ_PowerStaff_R2.asset   ← Rank II
EQ_DragonStaff_R3.asset  ← Rank III

RU_BurnEcho_R1.asset
RU_ComboTrigger_R3.asset
```

### 3.2. Prefabs

|Loại|Prefix|Ví dụ|
|---|---|---|
|Enemy Prefab|`EN_`|`EN_TestMinion.prefab`|
|Node View Prefab|`Node_`|`Node_Minion.prefab`, `Node_Boss.prefab`|
|VFX Prefab|`VFX_`|`VFX_Burn.prefab`, `VFX_Detonates.prefab`|
|UI Element|`UI_`|`UI_FloatingText.prefab`, `UI_DamageNumber.prefab`|
|Player Prefab|Không prefix|`Player.prefab`|

### 3.3. Sprites và Textures

|Loại|Prefix|Ví dụ|
|---|---|---|
|Effect icon|`Icon_`|`Icon_Burn.png`, `Icon_Overdrive.png`|
|Spell icon|`SpellIcon_`|`SpellIcon_Fireball.png`|
|UI element|`UI_`|`UI_HPBar.png`, `UI_ButtonBg.png`|
|Node icon|`Node_`|`Node_Shop.png`, `Node_Rest.png`|
|Background|`BG_`|`BG_CombatArena.png`|
|Character|`Char_`|`Char_Player.png`, `Char_FireBoss.png`|

### 3.4. Audio

|Loại|Prefix|Ví dụ|
|---|---|---|
|SFX|`SFX_`|`SFX_Fireball.wav`, `SFX_CritHit.wav`|
|Music|`BGM_`|`BGM_Combat.ogg`, `BGM_Map.ogg`|
|UI sound|`UI_`|`UI_ButtonClick.wav`, `UI_MenuOpen.wav`|

### 3.5. Animations

|Loại|Convention|Ví dụ|
|---|---|---|
|Animation Clip|`[Character]_[Action]`|`Player_CastSpell.anim`, `EN_Minion_Die.anim`|
|Animator Controller|`[Character]_AC`|`Player_AC.controller`, `EN_FireBoss_AC.controller`|

---

## 4. Scene Naming

| Scene        | File                | Mô tả                                    |
| ------------ | ------------------- | ---------------------------------------- |
| Bootstrap    | `Boot.unity`        | Khởi động game, load GameManager         |
| Main Menu    | `MainMenu.unity`    | Menu chính                               |
| Map View     | `MapView.unity`     | Màn hình bản đồ                          |
| Combat       | `Combat.unity`      | Màn hình combat                          |
| Shop         | `Shop.unity`        | Màn hình shop                            |
| Rest         | `Rest.unity`        | Màn hình rest                            |
| Event        | `Event.unity`       | Màn hình event                           |
| Reward       | `Reward.unity`      | Màn hình reward sau combat               |
| Game Over    | `GameOver.unity`    | Màn hình thua                            |
| Run Complete | `RunComplete.unity` | Màn hình thắng toàn bộ run               |
| Test Combat  | `Test_Combat.unity` | Scene test combat, chỉ dùng trong Editor |
| Test Map     | `Test_Map.unity`    | Scene test map generation                |

---

## 5. Quick Reference Card

### C# — Tóm tắt 1 trang

```
CLASS / INTERFACE / ENUM     → PascalCase
  IEntity, EffectSystem, NodeType

METHOD (tất cả)              → PascalCase
  TakeDamage(), GetEffectivePotency(), OnHpChanged()

PRIVATE FIELD                → _camelCase
  _currentHp, _activeEffects, _config

PUBLIC PROPERTY              → PascalCase
  CurrentHp, MaxHp, CrystalizeFlag

EVENT                        → On + VerbPhrase
  OnHpChanged, OnEffectApplied, OnCombinationTriggered

CONST                        → SCREAMING_SNAKE_CASE
  HP_CAP, BASE_DODGE, MAX_ARMOR_STACKS

TEST METHOD                  → MethodName_Condition_Expected
  TakeDamage_LessThanArmor_ReturnsZeroOverflow

NAMESPACE                    → WanderingWanderer.[Layer].[Module]
  WanderingWanderer.Logic.Combat
  WanderingWanderer.Unity.UI.Combat
```

### Asset — Prefix nhanh

```
SP_   → Spell Definition
EN_   → Enemy Definition / Enemy Prefab
EQ_   → Equipment Definition
RU_   → Rune Definition
VFX_  → VFX Prefab
Icon_ → Effect/UI Icon
SFX_  → Sound Effect
BGM_  → Background Music
BG_   → Background Image
UI_   → UI Prefab / UI Texture
Node_ → Node View Prefab / Node Icon
Char_ → Character Sprite
```

### Folder — nhanh

```
Scripts/Logic/      → Pure C# (không UnityEngine)
Scripts/Unity/      → MonoBehaviour, Unity-specific
Scripts/Editor/     → Editor tools
Data/Config/        → ScriptableObject config
Data/Spells/        → Spell SO assets (theo Rank)
Data/Enemies/       → Enemy SO assets (theo loại)
Tests/EditMode/     → NUnit tests, không cần scene
Tests/PlayMode/     → UnityTest, cần runtime
Resources/Config/   → SO cần Resources.Load() trong test
```

### Rank suffix (Equipment và Rune asset)

```
_R1  → Rank I
_R2  → Rank II
_R3  → Rank III
```