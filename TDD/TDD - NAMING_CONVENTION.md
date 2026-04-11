___
 **Game:** Wandering Wanderer 
 **Author:** DukTofn 
 **Last Updated:** 09/04/2026
___
## Mục lục

1. [Folder Structure](#1-folder-structure)
   - [1.1. Cấu trúc cấp 1](#11-cấu-trúc-cấp-1)
   - [1.2. `Data/` — chi tiết](#12-data--chi-tiết)
   - [1.3. `Scripts/` — chi tiết](#13-scripts--chi-tiết)
   - [1.4. `Tests/` — chi tiết](#14-tests--chi-tiết)
   - [1.5. `Prefabs/` — chi tiết](#15-prefabs--chi-tiết)
2. [Assembly Structure](#2-assembly-structure)
3. [C# Naming Convention](#3-c-naming-convention)
   - [3.1. Classes và Interfaces](#31-classes-và-interfaces)
   - [3.2. Methods](#32-methods)
   - [3.3. Fields và Properties](#33-fields-và-properties)
   - [3.4. Events](#34-events)
   - [3.5. Async (UniTask) — quy tắc riêng](#35-async-unitask--quy-tắc-riêng)
   - [3.6. Namespaces](#36-namespaces)
   - [3.7. Những thứ cần tránh](#37-những-thứ-cần-tránh)
4. [Asset Naming Convention](#4-asset-naming-convention)
   - [4.1. ScriptableObject assets](#41-scriptableobject-assets)
   - [4.2. Prefabs](#42-prefabs)
   - [4.3. Sprites và Textures](#43-sprites-và-textures)
   - [4.4. Audio](#44-audio)
   - [4.5. Animations](#45-animations)
5. [Scene Naming](#5-scene-naming)
6. [Quick Reference Card](#6-quick-reference-card)
   - [C# — Tóm tắt 1 trang](#c-tóm-tắt-1-trang)
   - [Asset — Prefix nhanh](#asset-prefix-nhanh)
   - [Folder — nhanh](#folder-nhanh)
   - [Rank suffix](#rank-suffix-equipment-và-rune-asset)
   - [UniTask — rule nhanh](#unitask-rule-nhanh)

---

## 1. Folder Structure

### 1.1. Cấu trúc cấp 1

```
Assets/
  ├── Data/           ← ScriptableObject assets (config, definitions, events)
  ├── Prefabs/        ← Prefab của GameObject
  ├── Scenes/         ← Scene files
  ├── Scripts/        ← C# source code
  ├── Sprites/        ← 2D sprites và textures
  ├── Audio/          ← Sound effects và music
  ├── Fonts/          ← Font files
  ├── Materials/      ← Material assets
  ├── Animations/     ← Animation clips và controllers
  ├── VFX/            ← Particle systems và VFX
  ├── Resources/      ← Chỉ những gì cần Resources.Load() (config SO cho Edit Mode Test)
  └── Tests/          ← Test files
```

> **Quy tắc `Resources/`:** Chỉ đặt SO cần load trong Edit Mode test (`Resources.Load<T>()`). ScriptableObject còn lại inject qua Inspector. Không đặt Prefab hay Scene vào Resources.

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
  ├── Events/                           ← EventChannelSO assets (cross-boundary events)
  │     ├── CombatEndedSO.asset
  │     ├── NodeEnteredSO.asset
  │     ├── GoldChangedSO.asset
  │     ├── PendingModifierChangedSO.asset
  │     └── RewardReadySO.asset
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
  ├── Foundation/               ← Utility thuần, không biết game logic tồn tại (Assembly: Foundation)
  │     ├── Foundation.asmdef
  │     ├── Events/
  │     │     ├── SinglePayloadEvent/
  │     │     │     ├── EventChannelSO.cs         ← abstract EventChannelSO<T>
  │     │     │     └── EventListener.cs           ← abstract EventListener<T> : MonoBehaviour
  │     │     └── TwoPayloadEvent/
  │     │           ├── EventChannelSO.cs          ← abstract EventChannelSO<T1, T2>
  │     │           └── EventListener.cs           ← abstract EventListener<T1, T2>
  │     └── StateMachine/
  │           ├── IState.cs
  │           ├── IAsyncState.cs                   ← extend IState, thêm UniTask EnterAsync()
  │           └── StateMachineManager.cs
  │
  ├── Logic/                    ← Pure C#, KHÔNG reference UnityEngine (Assembly: Logic)
  │     ├── Logic.asmdef
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
  │           ├── Enums.cs             ← Element, EffectType, NodeType, Phase, CombatResult...
  │           ├── Structs.cs           ← StatModifier, CombatModifier, RewardOffer...
  │           ├── Interfaces.cs        ← IEntity, IActionCommand...
  │           └── Constants.cs         ← magic number tạm thời (nên chuyển sang SO)
  │
  ├── Unity/                    ← MonoBehaviour, Unity-specific (Assembly: Unity)
  │     ├── Unity.asmdef
  │     ├── Core/
  │     │     └── GameManagerDriver.cs    ← MonoBehaviour, gọi _fsm.Tick() trong Update()
  │     ├── Bridges/                      ← EventBridge adapters: relay C# event → EventChannelSO
  │     │     ├── CombatEndedBridge.cs
  │     │     ├── NodeEnteredBridge.cs
  │     │     ├── GoldChangedBridge.cs
  │     │     └── PendingModifierBridge.cs
  │     ├── Presentation/
  │     │     ├── VisualQueue.cs
  │     │     └── Commands/
  │     │           ├── ShowDamageNumberCommand.cs
  │     │           ├── ShowHpGainCommand.cs
  │     │           ├── ShowMpGainCommand.cs
  │     │           ├── PlaySpellAnimCommand.cs
  │     │           ├── PlayDamageAnimCommand.cs
  │     │           ├── PlayEffectApplyAnimCommand.cs
  │     │           ├── PlayEffectExpireAnimCommand.cs
  │     │           ├── PlayBurnAnimCommand.cs
  │     │           ├── PlayFrozenThawAnimCommand.cs
  │     │           ├── PlayCrystalizeShieldAnimCommand.cs
  │     │           ├── PlayDetonatesAnimCommand.cs
  │     │           ├── PlayDeathAnimCommand.cs
  │     │           └── ParallelCommand.cs           ← UniTask.WhenAll() wrapper
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
  │     │     ├── Config/                           ← SO class definitions (hoặc gom vào Definitions/)
  │     │     │     ├── CombatConfigSO.cs
  │     │     │     ├── ArcConfigSO.cs
  │     │     │     ├── ShopPriceConfigSO.cs
  │     │     │     ├── RewardRateConfigSO.cs
  │     │     │     ├── EventConfigSO.cs
  │     │     │     └── WisdomSlotConfigSO.cs
  │     │     ├── Definitions/                      ← SO class definitions (*SO suffix)
  │     │     │     ├── SpellDefinitionSO.cs        ← MinWisdomToImprint (WIS để Imprint)
  │     │     │     ├── EnemyDefinitionSO.cs
  │     │     │     ├── EquipmentDefinitionSO.cs
  │     │     │     └── RuneDefinitionSO.cs
  │     │     └── Events/                           ← Concrete EventChannelSO subclasses
  │     │           ├── CombatEndedSO.cs
  │     │           ├── NodeEnteredSO.cs
  │     │           ├── GoldChangedSO.cs
  │     │           ├── PendingModifierChangedSO.cs
  │     │           └── RewardReadySO.cs
  │     └── Bootstrap/
  │           └── ConfigLoader.cs       ← MonoBehaviour inject SO vào runtime
  │
  └── Editor/                   ← Editor-only tools (không build vào game)
        └── MapDebugWindow.cs
```

---

### 1.4. `Tests/` — chi tiết

```
Tests/
  ├── EditMode/
  │     ├── Tests.EditMode.asmdef
  │     ├── Foundation/
  │     │     └── StateMachineManagerTests.cs
  │     ├── Combat/
  │     │     ├── ArmorStackTests.cs
  │     │     ├── DamageCalculatorTests.cs
  │     │     ├── HitDodgeResolverTests.cs
  │     │     ├── EffectSystemTests.cs
  │     │     └── CombatResolverTests.cs
  │     ├── Entities/
  │     │     ├── PlayerControllerStatLayeringTests.cs   ← test L0–L2 RecalculateBaseAttributes
  │     │     ├── PlayerControllerGetterTests.cs         ← test L3 GetEffective*()
  │     │     └── AI/
  │     │           ├── SpellSelectorTests.cs
  │     │           ├── RandomPolicyTests.cs
  │     │           ├── PriorityPolicyTests.cs
  │     │           ├── WeightedRandomPolicyTests.cs
  │     │           └── ScriptedPolicyTests.cs
  │     ├── Spells/
  │     │     ├── SpellCasterValidationTests.cs
  │     │     ├── SpellCasterEffectTests.cs
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
  │           └── StubDecisionPolicy.cs
  └── PlayMode/
        ├── Tests.PlayMode.asmdef
        ├── Combat/
        │     ├── TurnManagerAsyncTests.cs    ← test StartCombatAsync, CancellationToken
        │     ├── PhaseHandlerTests.cs
        │     └── SpellCasterFlowTests.cs
        ├── Presentation/
        │     ├── VisualQueueDrainTests.cs    ← test DrainAsync
        │     ├── ActionCommandTests.cs       ← test ExecuteAsync + DOTween timing
        │     └── ParallelCommandTests.cs
        ├── Bridges/
        │     └── EventBridgeTests.cs         ← test relay C# event → SO.RaiseEvent()
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
  │           ├── UI_FloatingText.prefab
  │           └── UI_DamageNumber.prefab
  ├── Map/
  │     ├── Node_Minion.prefab
  │     ├── Node_Elite.prefab
  │     ├── Node_Boss.prefab
  │     ├── Node_Shop.prefab
  │     ├── Node_Rest.prefab
  │     └── Node_Event.prefab
  └── VFX/
        ├── VFX_Burn.prefab
        ├── VFX_Frozen.prefab
        ├── VFX_Detonates.prefab
        ├── VFX_Crystalize.prefab
        └── VFX_Overdrive.prefab
```

---

## 2. Assembly Structure

```
Foundation.asmdef     ← Core.Foundation.Events.*, Core.Foundation.StateMachine
                        Không reference ai. UniTask OK.

Logic.asmdef          ← Game.Logic.*
                        Reference: Foundation (optional)
                        KHÔNG reference UnityEngine, KHÔNG reference Unity.asmdef

Unity.asmdef          ← Game.Unity.*
                        Reference: Logic, Foundation, UnityEngine, DOTween, UniTask

Tests.EditMode.asmdef ← Reference: Logic, Foundation
                        Platform: Editor only

Tests.PlayMode.asmdef ← Reference: Logic, Foundation, Unity
                        Platform: All (cần runtime)
```

**Quy tắc dependency:**

```
Foundation  ←  Logic  ←  Unity  ←  (Tests)
    ↑                        ↑
(không ai phụ thuộc)    (Editor tools)
```

Logic **không bao giờ** reference Unity. Foundation **không bao giờ** reference Logic hoặc Unity.

---

## 3. C# Naming Convention

### 3.1. Classes và Interfaces

|Loại|Convention|Ví dụ|
|---|---|---|
|Class|PascalCase|`DamageCalculator`, `ArmorStack`|
|Interface|`I` + PascalCase|`IEntity`, `IRunePassive`, `IDecisionPolicy`, `IActionCommand`|
|Abstract class|PascalCase|`BaseView`|
|Enum|PascalCase|`EffectType`, `Element`, `NodeType`, `CombatResult`|
|Enum value|PascalCase|`EffectType.Burn`, `CombatResult.Win`|
|Struct|PascalCase|`StatModifier`, `CombatModifier`|
|ScriptableObject class|PascalCase|`CombatConfig`, `SpellDefinition`, `CombatEndedSO`|

---

### 3.2. Methods

|Loại|Convention|Ví dụ|
|---|---|---|
|Public method (sync)|PascalCase|`TakeDamage()`, `ApplyArmor()`|
|Private method (sync)|PascalCase|`CalculateOverflow()`|
|**Async method (UniTask)**|PascalCase + **`Async`** suffix|`StartCombatAsync()`, `DrainAsync()`, `ExecuteAsync()`|
|Test method|`MethodName_Condition_ExpectedResult`|`TakeDamage_LessThanArmor_ReturnsZeroOverflow`|
|Event handler|`On` + EventName (private)|`OnHpChanged()`, `OnCombatEnded()`|

> **Không dùng Coroutine.** Bất kỳ method nào cần async đều là `UniTask`, không phải `IEnumerator`. Convention cũ `PascalCase + Routine suffix` không còn áp dụng.

---

### 3.3. Fields và Properties

|Loại|Convention|Ví dụ|
|---|---|---|
|Public field|camelCase (tránh dùng)|—|
|Private field|`_` + camelCase|`_currentHp`, `_activeEffects`, `_fsm`|
|`[SerializeField]` private|`_` + camelCase|`[SerializeField] private CombatConfig _config;`|
|Public property (get)|PascalCase|`CurrentHp`, `StoredMaxHp`|
|Public property (get/set)|PascalCase|`CrystalizeFlag`|
|Static field|`s_` + camelCase|`s_instance`|
|Const|SCREAMING_SNAKE_CASE|`MAX_ARMOR_STACKS`, `HP_CAP`|

```csharp
// Ví dụ đúng — PlayerController
public class PlayerController
{
    [SerializeField] private CombatConfig _config;

    private int _currentHp;
    private int _storedMaxHp;

    public int CurrentHp   => _currentHp;
    public int StoredMaxHp => _storedMaxHp;   // "stored" phản ánh đã tính L0+L1+L2

    public event Action<int, int> OnHpChanged;

    public void TakeDamage(float amount)
    {
        _currentHp = Mathf.Max(0, _currentHp - (int)amount);
        OnHpChanged?.Invoke(_currentHp, _storedMaxHp);
    }

    private int CalculateStoredMaxHp(int vit, int flatBonus)
    {
        int l0 = (int)(_config.HpCap * vit / (vit + _config.HpHalf));
        return l0 + flatBonus;
    }
}
```

---

### 3.4. Events

|Loại|Convention|Ví dụ|
|---|---|---|
|C# event (internal)|`On` + VerbPhrase|`OnHpChanged`, `OnEffectApplied`, `OnCombatEnded`|
|EventChannelSO class|Tên mô tả + `SO` suffix|`CombatEndedSO`, `GoldChangedSO`|
|EventChannelSO asset|Tên class (không thêm gì)|`CombatEndedSO.asset`|

**Phân biệt C# event vs EventChannelSO:**

```csharp
// C# event — dùng trong Logic layer và internal communication
public event Action<CombatResult> OnCombatEnded;

// EventChannelSO — dùng cross-scene, wired qua Bridge MonoBehaviour
// (Logic không biết SO tồn tại)
[SerializeField] private CombatEndedSO _combatEndedChannel;
```

```csharp
// Invoke an toàn
OnHpChanged?.Invoke(_currentHp, _storedMaxHp);

// EventChannelSO raise
_combatEndedChannel.RaiseEvent(CombatResult.Win);
```

---

### 3.5. Async (UniTask) — quy tắc riêng

```csharp
// ✅ Đúng — async method
public async UniTask StartCombatAsync(CancellationToken ct = default) { ... }
public async UniTask DrainAsync(CancellationToken ct = default) { ... }
public async UniTask ExecuteAsync(CancellationToken ct = default) { ... }

// ✅ Đúng — fire-and-forget (dùng khi không cần await kết quả)
StartCombatAsync(ct).Forget();

// ✅ Đúng — DOTween bridge
await transform.DOShakePosition(0.3f).ToUniTask(ct);
await DOTween.To(() => 0f, x => { }, 1f, 0.5f).ToUniTask(ct);

// ❌ Sai — Coroutine
IEnumerator DrainQueueRoutine() { ... }
StartCoroutine(DrainQueueRoutine());

// ❌ Sai — async void (không dùng ngoại trừ Unity event callback bắt buộc)
async void Start() { ... }    // KHÔNG — dùng UniTaskVoid hoặc Forget()

// ✅ Đúng — thay async void
private async UniTaskVoid StartAsync()
{
    await SomeAsync();
}
```

**CancellationToken:** Mọi async method nhận `CancellationToken ct` là parameter cuối. Truyền token này xuống tất cả call bên trong, bao gồm DOTween `.ToUniTask(ct)`.

---

### 3.6. Namespaces

```csharp
// Foundation layer
namespace Core.Foundation.Events.SinglePayloadEvent { }
namespace Core.Foundation.Events.TwoPayloadEvent { }
namespace Core.Foundation.StateMachine { }

// Logic layer
namespace Game.Logic.Combat { }
namespace Game.Logic.Entities { }
namespace Game.Logic.Entities.AI { }
namespace Game.Logic.Spells { }
namespace Game.Logic.Passives { }
namespace Game.Logic.Meta { }
namespace Game.Logic.Map { }
namespace Game.Logic.Core { }
namespace Game.Logic.Shared { }

// Unity layer
namespace Game.Unity.Core { }
namespace Game.Unity.Bridges { }
namespace Game.Unity.Presentation { }
namespace Game.Unity.Presentation.Commands { }
namespace Game.Unity.UI.Combat { }
namespace Game.Unity.UI.Map { }
namespace Game.Unity.UI.Shop { }
namespace Game.Unity.Data.Config { }
namespace Game.Unity.Data.Definitions { }
namespace Game.Unity.Data.Events { }
```

---

### 3.7. Những thứ cần tránh

```csharp
// ❌ Tránh
public int hp;                              // public field không có property
private int currentHp;                      // thiếu underscore prefix
IEnumerator DrainQueueRoutine() { }         // Coroutine — dùng UniTask thay
async void OnButtonClick() { }             // async void — dùng UniTaskVoid
void DoStuff() { }                          // tên không mô tả
async UniTask LoadData() { }               // thiếu Async suffix
private int baseFirePotency;               // "base" sai — phải là "stored" (đã tính L0+L1+L2)

// ✅ Đúng
public int CurrentHp => _currentHp;
private int _currentHp;
async UniTask DrainAsync(CancellationToken ct) { }
private async UniTaskVoid HandleButtonClickAsync() { }
async UniTask ApplyBurnDamageAsync(CancellationToken ct) { }
async UniTask LoadDataAsync(CancellationToken ct) { }
private int _storedFirePotency;
```

---

## 4. Asset Naming Convention

### 4.1. ScriptableObject assets

|Loại|Prefix|Ví dụ|
|---|---|---|
|Spell Definition|`SP_`|`SP_Fireball.asset`, `SP_IceShield.asset`|

**Field bắt buộc trên `SpellDefinition` (SO class, suffix `SO` nếu đặt tên file theo convention repo):** ngoài `id`, `displayName`, `rank`, `element`, `baseCost`, `baseCooldown`, `targetType`, `effects[]`, có **`MinWisdomToImprint`** (`int`) — ngưỡng WIS tối thiểu để Imprint (GDD — Spells). Serialize trong Inspector dạng **PascalCase**.
|Enemy Definition|`EN_`|`EN_TestMinion.asset`, `EN_FireBoss.asset`|
|Equipment Definition|`EQ_`|`EQ_FlameWand_R1.asset`, `EQ_DragonFang_R3.asset`|
|Rune Definition|`RU_`|`RU_FireEmbrace_R1.asset`, `RU_Combustion_R3.asset`|
|Config SO|Tên mô tả, không prefix|`CombatConfig.asset`, `ArcConfig_1.asset`|
|EventChannelSO|Tên mô tả + `SO`|`CombatEndedSO.asset`, `GoldChangedSO.asset`|

**Rank suffix (Equipment và Rune):**

```
EQ_FlameWand_R1.asset      ← Rank I   (1 StatModifier flat)
EQ_PowerStaff_R2.asset     ← Rank II  (2 StatModifier flat)
EQ_DragonFang_R3.asset     ← Rank III (đến 3 StatModifier, có thể PercentAdd)

RU_FireEmbrace_R1.asset
RU_MageArmor_R2.asset
RU_Combustion_R3.asset
```

---

### 4.2. Prefabs

|Loại|Prefix|Ví dụ|
|---|---|---|
|Enemy Prefab|`EN_`|`EN_TestMinion.prefab`|
|Node View Prefab|`Node_`|`Node_Minion.prefab`, `Node_Boss.prefab`|
|VFX Prefab|`VFX_`|`VFX_Burn.prefab`, `VFX_Detonates.prefab`|
|UI Element Prefab|`UI_`|`UI_FloatingText.prefab`, `UI_DamageNumber.prefab`|
|Player Prefab|Không prefix|`Player.prefab`|

---

### 4.3. Sprites và Textures

|Loại|Prefix|Ví dụ|
|---|---|---|
|Effect icon|`Icon_`|`Icon_Burn.png`, `Icon_Overdrive.png`, `Icon_Crystalize.png`|
|Spell icon|`SpellIcon_`|`SpellIcon_Fireball.png`, `SpellIcon_IceShard.png`|
|UI element|`UI_`|`UI_HPBar.png`, `UI_ButtonBg.png`|
|Node icon|`Node_`|`Node_Shop.png`, `Node_Rest.png`, `Node_Boss.png`|
|Background|`BG_`|`BG_CombatArena.png`, `BG_MapView.png`|
|Character|`Char_`|`Char_Player.png`, `Char_FireBoss.png`|

---

### 4.4. Audio

|Loại|Prefix|Ví dụ|
|---|---|---|
|SFX|`SFX_`|`SFX_Fireball.wav`, `SFX_Detonates.wav`|
|Music|`BGM_`|`BGM_Combat.ogg`, `BGM_Map.ogg`|
|UI sound|`UI_`|`UI_ButtonClick.wav`, `UI_MenuOpen.wav`|

---

### 4.5. Animations

|Loại|Convention|Ví dụ|
|---|---|---|
|Animation Clip|`[Subject]_[Action]`|`Player_CastSpell.anim`, `EN_Minion_Die.anim`|
|Animator Controller|`[Subject]_AC`|`Player_AC.controller`, `EN_FireBoss_AC.controller`|

---

## 5. Scene Naming

|Scene|File|Mô tả|
|---|---|---|
|Bootstrap|`Boot.unity`|Khởi động, DOTween.Init(), ConfigLoader|
|Main Menu|`MainMenu.unity`|Menu chính|
|Map View|`MapView.unity`|Màn hình bản đồ, GameManagerDriver tồn tại ở đây|
|Combat|`Combat.unity`|Màn hình combat|
|Shop|`Shop.unity`|Màn hình shop|
|Rest|`Rest.unity`|Màn hình rest|
|Event|`Event.unity`|Màn hình event|
|Reward|`Reward.unity`|Màn hình reward sau combat|
|Game Over|`GameOver.unity`|Màn hình thua|
|Run Complete|`RunComplete.unity`|Màn hình thắng toàn bộ run|
|Test Combat|`Test_Combat.unity`|Scene test combat, chỉ dùng trong Editor|
|Test Map|`Test_Map.unity`|Scene test map generation|

---

## 6. Quick Reference Card

### C# — Tóm tắt 1 trang

```
CLASS / STRUCT / INTERFACE / ENUM  → PascalCase
  IEntity, StatModifier, EffectType, CombatEndedSO

METHOD (sync)                      → PascalCase
  TakeDamage(), ApplyArmor(), RecalculateBaseAttributes()

METHOD (async UniTask)             → PascalCase + Async suffix
  StartCombatAsync(), DrainAsync(), ExecuteAsync()

PRIVATE FIELD                      → _camelCase
  _currentHp, _storedFirePotency, _fsm, _config

PUBLIC PROPERTY                    → PascalCase
  CurrentHp, StoredMaxHp, CrystalizeFlag

C# EVENT                           → On + VerbPhrase
  OnHpChanged, OnCombatEnded, OnEffectApplied

CONST                              → SCREAMING_SNAKE_CASE
  HP_CAP, BASE_DODGE, MAX_ARMOR_STACKS

TEST METHOD                        → MethodName_Condition_Expected
  TakeDamage_LessThanArmor_ReturnsZeroOverflow
  StartCombatAsync_PlayerDies_RaisesLoseEvent

NAMESPACE                          → Game.[Layer].[Module]
  Core.Foundation.StateMachine
  Game.Logic.Combat
  Game.Unity.Bridges
```

### Asset — Prefix nhanh

```
SP_       → Spell Definition
EN_       → Enemy Definition / Enemy Prefab
EQ_       → Equipment Definition  (+_R1/_R2/_R3 suffix)
RU_       → Rune Definition       (+_R1/_R2/_R3 suffix)
VFX_      → VFX Prefab
UI_       → UI Prefab / UI Texture / UI Sound
Icon_     → Effect / Skill Icon
SpellIcon_→ Spell Icon
SFX_      → Sound Effect
BGM_      → Background Music
BG_       → Background Image
Node_     → Node View Prefab / Node Icon
Char_     → Character Sprite
```

### Folder — nhanh

```
Scripts/Foundation/     → EventChannelSO, StateMachineManager, IState, IAsyncState
Scripts/Logic/          → Pure C# (không UnityEngine)
Scripts/Unity/Core/     → GameManagerDriver
Scripts/Unity/Bridges/  → EventBridge adapters (relay C# event → SO)
Scripts/Unity/          → MonoBehaviour, Unity-specific
Scripts/Editor/         → Editor tools
Data/Config/            → Config SO assets
Data/Events/            → EventChannelSO assets
Data/Spells/            → Spell SO (theo Rank)
Data/Enemies/           → Enemy SO (theo loại)
Data/Equipment/         → Equipment SO (theo Rank)
Data/Runes/             → Rune SO (theo Rank)
Tests/EditMode/         → NUnit, không cần scene
Tests/PlayMode/         → UnityTest, cần runtime
Resources/Config/       → SO cần Resources.Load() trong Edit Mode test
```

### Rank suffix (Equipment và Rune asset)

```
_R1  → Rank I   (flat modifier đơn giản)
_R2  → Rank II  (flat modifier, có thể mix stat)
_R3  → Rank III (có thể có PercentAdd modifier)
```

### UniTask — rule nhanh

```
Mọi async method      → UniTask, KHÔNG dùng IEnumerator
Suffix bắt buộc       → Async (DrainAsync, ExecuteAsync, StartCombatAsync)
DOTween bridge        → .ToUniTask(ct)
Fire-and-forget       → .Forget() hoặc UniTaskVoid
Cancel khi destroy    → CancellationTokenSource.Cancel() trong OnDestroy()
Tween cleanup         → tween.Kill() trong ct callback
```