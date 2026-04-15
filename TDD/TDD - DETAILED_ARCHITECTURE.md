___
 **Game:** Wandering Wanderer 
 **Author:** DukTofn 
 **Last Updated:** 15/04/2026 
 ___
## Changelog

### v8

|Điểm|Thay đổi|
|---|---|
|Thêm `AvailableAttributePoints` system|Tách biệt "earn" và "spend" attribute point — `RestNode` và `GiveAttributeAction` chỉ cộng `availableAttributePoints`, player phân bổ tại `AttributeAllocationView`|
|Sửa `PlayerController`|Thêm `availableAttributePoints: int`, `SpendAttributePoint(statType)`, `HasAvailablePoints()`|
|Sửa `GiveAttributeAction`|Bỏ `playerChooses: bool` — luôn cộng vào `availableAttributePoints`, player luôn chọn|
|Sửa `RestNode`|Đổi từ trực tiếp `AddAttribute()` → `availableAttributePoints += 1`|
|Sửa `EventSystem.ResolveEvent`|`GiveAttributeAction` chỉ cần `+= amount`, không cần `choice.attributeType`|
|Sửa `EventChoice`|Bỏ `attributeType` — không còn cần thiết|

### v7

|Điểm|Thay đổi|
|---|---|
|Thiết kế `EventDefinition`|Định nghĩa data structure cho Event Node — trước đó chỉ có mô tả text trong GDD|
|Thêm `EventAction` typed subtypes|8 subtype: `GiveGoldAction`, `GiveAttributeAction`, `GiveRandomItemAction`, `TakeHpPercentAction`, `TakeGoldPercentAction`, `ForceCombatAction`, `ApplyCombatModifierAction`, `OpenMiniShopAction`|
|Thêm `EventCategory` enum|`Positive`, `TradeOff`, `Negative` — phân loại event theo GDD|
|Định nghĩa `CombatModifier` struct|Dùng `StatModifier[]` — áp dụng tạm thời vào combat tiếp theo|
|Chi tiết `EventSystem` resolve flow|Toàn bộ pipeline từ chọn event → xác nhận → thực thi action → chuyển scene|
|Mở rộng `EventConfigSO`|Chứa `EventDefinition[]` với `[SerializeReference]` cho EventAction|

### v6

|Điểm|Thay đổi|
|---|---|
|Thiết kế lại `SpellDefinition`|Tách `SpellEffect` thành 4 typed subtype, loại bỏ `valueFormula` string|
|Thêm `PotencyRef` struct|Thay string formula bằng `{ element, coefficient }` type-safe|
|Thêm `TargetResolver`|Tách target resolution ra khỏi spell-level `targetType`, từng effect có resolver riêng|
|Thêm `SecondaryRandom` resolver|Giải quyết Spark (primary target + random other at 50%)|
|Thêm `TieBreaker` cho `LowestHpEnemies`|Giải quyết Candle Ghost tie-break theo `fire_res`|
|Làm rõ hit/dodge per target type|SelectedEnemy miss → abort spell; AOE → check per enemy|
|Làm rõ Bubble "Stun"|Bubble apply `Frozen` trực tiếp, không cần combo|
|Thêm `SpellCastContext`|Execution context chứa caster, selected target, để resolve `SecondaryRandom`|
|Thiết kế lại `SpellCaster` pipeline|Tách target resolution, hit/dodge check, effect execution thành 3 bước rõ ràng|

### v5

|Điểm|Thay đổi|
|---|---|
|Thêm Foundation Layer|`EventChannelSO` và `StateMachineManager` là Foundation, không phụ thuộc vào game logic|
|Thay Coroutine → UniTask|`ActionCommand.ExecuteAsync()`, `VisualQueue.DrainAsync()`, `TurnManager` phase loop async|
|DOTween cho animation|Thay manual Coroutine anim, bridge với UniTask qua `.ToUniTask()`|
|`GameManager` dùng StateMachine|State: InMap / InCombat / InShop / InRest / InEvent — sync Tick() phù hợp|
|TurnManager **không** dùng StateMachine|Phase loop là sequential async flow, không phải FSM — UniTask `await` thẳng|
|Phân ranh EventChannelSO vs C# event|Cross-system/cross-scene → EventChannelSO; internal entity → C# event|
|Thêm `IAsyncState`|Extend `IState` cho state cần async trong `Enter()` (scene loading)|

### v4

|Điểm|Thay đổi|
|---|---|
|Thêm Stat Layering Model|Định nghĩa 4 layer tính stat, phân biệt flat/percent bonus|
|Thêm `StatModifier` struct|Ngôn ngữ chung cho Equipment và Rune mô tả stat modifier|
|Thêm `StatType` enum|Cover toàn bộ stat có thể modify (Main Attr, Sub Attr, Resource)|
|Sửa `PlayerController`|Tách rõ layer 0–2 trong `RecalculateBaseAttributes()`, `max_hp` nhận flat bonus|
|Sửa `EquipmentSystem`|Equipment cung cấp `StatModifier[]` thay vì "1–3 stat" mơ hồ|
|Sửa `RuneSystem`|`StatModifier` rune type dùng chung `StatModifier[]`, thống nhất với Equipment|
|Sửa `EquipmentDefinition`|Thêm `modifiers: StatModifier[]`|

### v3

|Điểm|Thay đổi|
|---|---|
|Thêm UI Layer|Observer pattern, `CastResult`, input flow, View per scene|

### v2

|Điểm|Thay đổi|
|---|---|
|Thêm Presentation Layer|`VisualQueue` + `ActionCommand` để sync logic và animation|
|Sửa Sub-Attribute model|Tách `baseXPotency` (static) và `GetEffectiveXPotency()` (dynamic)|
|Thêm Rune lifecycle|`IRunePassive` interface bắt buộc có `OnEmbed()` / `OnPurge()`|
|Thêm SaveSystem|Checkpoint-based save sau mỗi Node, không save mid-combat|
|Sửa Neutralize logic|Phân biệt rõ Buff+Debuff / Buff+Buff / Debuff+Debuff cùng nguyên tố|
|Thêm CombatResolver|Mediator giữa `EffectSystem` và `DamageCalculator`, phá dependency cycle|
|Ghi chú UI modifier|`pendingCombatModifier` cần hiển thị icon trên MapView|
|Mở rộng Enemy AI|Tách `SpellSelector` + `DecisionPolicy`, định nghĩa `CombatContext`|

---

## Mục lục

1. [Tổng quan kiến trúc](#1-tổng-quan-kiến-trúc)
2. [Foundation Layer](#2-foundation-layer)
    - [EventChannelSO (ScriptableObject Event System)](#eventchannelso-scriptableobject-event-system)
    - [StateMachineManager + IState](#statemachinemanager--istate)
3. [UI Layer](#3-ui-layer)
    - [Nguyên tắc](#nguyên-tắc)
    - [CastResult](#castresult)
    - [Events do Logic Layer phát](#events-do-logic-layer-phát)
    - [Views theo Scene](#views-theo-scene)
    - [Unsubscribe Pattern (bắt buộc)](#unsubscribe-pattern-bắt-buộc)
4. [Entry Point](#4-entry-point)
    - [GameManager](#gamemanager)
5. [Map Layer](#5-map-layer)
    - [MapSystem](#mapsystem)
    - [NodeRouter](#noderouter)
6. [Combat Layer](#6-combat-layer)
    - [TurnManager](#turnmanager)
    - [PhaseHandler](#phasehandler)
    - [CombatResolver](#combatresolver)
    - [EffectSystem](#effectsystem)
    - [DamageCalculator](#damagecalculator)
    - [HitDodgeResolver](#hitdodgeresolver)
7. [Presentation Layer](#7-presentation-layer)
    - [VisualQueue](#visualqueue)
    - [IActionCommand](#iactioncommand)
8. [Entity Layer](#8-entity-layer)
    - [Stat Layering Model](#stat-layering-model-thêm-mới-v4)
    - [StatModifier](#statmodifier-thêm-mới-v4)
    - [PlayerController](#playercontroller)
    - [EnemyController](#enemycontroller)
    - [Enemy AI Subsystem](#enemy-ai-subsystem)
    - [ArmorStack](#armorstack)
9. [Spell Layer](#9-spell-layer)
    - [SpellCaster](#spellcaster)
    - [SpellSlotManager](#spellslotmanager)
    - [CooldownTracker](#cooldowntracker)
10. [Passive Layer](#10-passive-layer)
    - [EquipmentSystem](#equipmentsystem)
    - [RuneSystem](#runesystem)
11. [Meta Layer](#11-meta-layer)
    - [ShopSystem](#shopsystem)
    - [RewardSystem](#rewardsystem)
    - [EventSystem](#eventsystem)
    - [RestNode](#restnode)
    - [GoldLedger](#goldledger)
12. [Data / Config Layer](#12-data--config-layer)
    - [ScriptableObjects](#scriptableobjects)
    - [EnemyDefinitions](#enemydefinitions)
    - [SpellDefinitions (Thiết kế lại v6)](#spelldefinitions-thiết-kế-lại-v6)
    - [EquipmentDefinition (Cập nhật v4)](#equipmentdefinition-cập-nhật-v4)
    - [RuneDefinition (Cập nhật v4)](#runedefinition-cập-nhật-v4)
    - [EventDefinitions (Thêm mới v7)](#eventdefinitions-thêm-mới-v7)
    - [CombatModifier (Thêm mới v7)](#combatmodifier-thêm-mới-v7)
    - [SaveSystem (Thêm mới v2)](#savesystem-thêm-mới-v2)
13. [Luồng dữ liệu chính](#13-luồng-dữ-liệu-chính)
14. [Ghi chú triển khai](#14-ghi-chú-triển-khai)

---

## 1. Tổng quan kiến trúc

Game được chia thành **11 layer** theo trách nhiệm. Các layer cấp cao phụ thuộc vào layer cấp thấp hơn — không có dependency ngược chiều.

```
[UI Layer]         ← subscribe event; không bị ai phụ thuộc ngược lại
    │
[Entry Point]
    └── [Map Layer]
            ├── [Combat Layer]
            │       ├── [Presentation Layer]
            │       ├── [Entity Layer]
            │       │       └── [Enemy AI Subsystem]
            │       └── [Spell Layer]
            │               └── [Passive Layer]
            └── [Meta Layer]

[Data / Config Layer]   ← pure data, không phụ thuộc vào ai
[Foundation Layer]      ← utility thuần, không biết game logic tồn tại
```

**Nguyên tắc thiết kế:**

- **ScriptableObject-driven config:** Mọi hằng số balancing đều nằm trong ScriptableObject, không hard-code trong logic.
- **Data tách khỏi behavior:** `EnemyDefinitions`, `SpellDefinitions`, `RuneDefinitions` là pure data — không chứa logic xử lý.
- **Effect là trung tâm combat:** Mọi tương tác giữa Spell, Rune, Equipment và Enemy đều đi qua `EffectSystem`.
- **Logic trước, Visual sau:** Combat Layer tính toán và đẩy Command vào `VisualQueue`. Presentation Layer tiêu thụ Queue bằng UniTask — hai bên không block nhau trực tiếp.
- **CombatResolver làm mediator:** Mọi tương tác yêu cầu cả `EffectSystem` lẫn `DamageCalculator` đều đi qua `CombatResolver`, tránh circular dependency.
- **Enemy AI là data-driven:** `DecisionPolicy` được cấu hình trong `EnemyDefinition`, không hard-code theo từng enemy.
- **UI Layer nằm ngoài cùng:** Logic không biết UI tồn tại. UI subscribe event một chiều.
- **Input không validate ở UI:** UI gọi API → nhận `Result`. Mọi validation nằm trong Logic.
- **UniTask thay toàn bộ Coroutine:** Async flow trong Presentation Layer và TurnManager dùng `async UniTask`, không dùng `IEnumerator`.
- **DOTween cho animation:** Tất cả tween animation dùng DOTween, bridge với UniTask qua `.ToUniTask()`.
- **EventChannelSO cho cross-boundary event:** Event cần đi qua scene boundary hoặc nhiều hệ thống không liên quan dùng `EventChannelSO`. Internal entity event giữ C# `event Action<T>`.
- **StateMachine cho game-state, không phải combat phase:** `GameManager` FSM (InMap/InCombat/InShop...) dùng `StateMachineManager`. TurnManager phase loop là sequential async — dùng UniTask `await` thẳng, không FSM.

---

## 2. Foundation Layer

Foundation Layer là tập hợp các **utility thuần** — không biết game logic tồn tại, không reference bất kỳ system nào của game. Các layer khác dùng Foundation như công cụ.

---

### `EventChannelSO` (ScriptableObject Event System)

Từ `Core.Foundation.Events`. Hai variant: `SinglePayloadEvent` và `TwoPayloadEvent`.

**Cơ chế:**

- `EventChannelSO<T>` là ScriptableObject — tồn tại độc lập, không bị destroy khi scene unload.
- `EventListener<T>` là MonoBehaviour — tự register/unregister khi `OnEnable`/`OnDisable`.
- Hỗ trợ cả `EventListener<T>` (Inspector-wired) lẫn `Action<T>` delegate (code-wired).
- Reentrancy-safe: `RaiseEvent` trong lúc đang raise → bị bỏ qua (không loop vô hạn).

**Dùng EventChannelSO khi:**

|Tình huống|Lý do|
|---|---|
|Event cần đi qua scene boundary|SO tồn tại xuyên scene, C# event không|
|Nhiều hệ thống không liên quan cùng lắng nghe|Không muốn tạo dependency trực tiếp|
|GD muốn wire event trong Inspector|`EventListener` cho phép kéo thả SO|

**Không dùng EventChannelSO khi:**

|Tình huống|Dùng gì thay|
|---|---|
|Event nội bộ trong cùng entity/system|C# `event Action<T>` — đơn giản hơn, không cần SO asset|
|Logic layer cần phát event|C# event (Logic không reference UnityEngine)|

**EventChannelSO assets cho Wandering Wanderer:**

|SO Asset|Payload|Phát bởi|Nhận bởi|
|---|---|---|---|
|`CombatEndedSO`|`CombatResult (Win/Lose)`|`TurnManager` (qua bridge)|`GameManager` state machine|
|`NodeEnteredSO`|`NodeType`|`NodeRouter`|`GameManager`, UI|
|`GoldChangedSO`|`int newBalance`|`GoldLedger` (qua bridge)|`PlayerInfoView` (MapView)|
|`PendingModifierChangedSO`|`CombatModifier?`|`GameManager`|`PendingModifierView`|
|`RewardReadySO`|`RewardOffer`|`RewardSystem` (qua bridge)|`RewardChoiceView`|

> **"Qua bridge"** — Logic layer phát C# event trước, một `EventBridge` MonoBehaviour trong scene subscribe C# event và gọi `SO.RaiseEvent()`. Logic không biết SO tồn tại. Bridge là adapter mỏng, không chứa business logic.

**Ví dụ EventBridge:**

```csharp
// Assets/Scripts/Unity/Bridges/CombatEndedBridge.cs
public class CombatEndedBridge : MonoBehaviour
{
    [SerializeField] private CombatEndedSO combatEndedChannel;
    private TurnManager _turnManager;

    private void Start()
    {
        _turnManager = GetComponent<TurnManager>();
        _turnManager.OnCombatEnded += HandleCombatEnded;
    }

    private void OnDestroy()
    {
        if (_turnManager != null)
            _turnManager.OnCombatEnded -= HandleCombatEnded;
    }

    private void HandleCombatEnded(CombatResult result)
    {
        combatEndedChannel.RaiseEvent(result);
    }
}
```

---

### `StateMachineManager` + `IState`

Từ `Core.Foundation.StateMachine`. Transition-based FSM, Tick() driven.

**Cơ chế:**

- `SetState(IState)` → gọi `Exit()` state cũ, `Enter()` state mới.
- `Tick()` → check transition, nếu condition thỏa → `SetState`. Sau đó gọi `Action()` state hiện tại.
- `AddAnyTransition(to, predicate)` → transition từ bất kỳ state nào.
- `AddTransition(from, to, predicate)` → transition từ state cụ thể.

**`IAsyncState` — extend cho state cần async Enter:** _(Thêm mới v5)_

```csharp
// Assets/Scripts/Unity/Foundation/StateMachine/IAsyncState.cs
public interface IAsyncState : IState
{
    UniTask EnterAsync();
}
```

`StateMachineManager` khi `SetState` nhận `IAsyncState` → gọi `EnterAsync()` thay vì `Enter()`. Dùng `UniTask.Void` hoặc fire-and-forget khi cần.

**Dùng StateMachine khi:**

|Tình huống|Lý do|
|---|---|
|Game-state level (`GameManager`)|InMap → InCombat → InShop... là FSM điển hình|
|State có transition rõ ràng, sync|Tick() + predicate phù hợp|

**Không dùng StateMachine khi:**

|Tình huống|Dùng gì thay|
|---|---|
|TurnManager phase (Start→Action→End)|Sequential async flow — `await` UniTask thẳng|
|Logic cần await bên trong mỗi step|UniTask pipeline|

**`GameManager` States:**

```
States:
  InMapState        → Tick() update map navigation
  InCombatState     → gọi TurnManager.StartCombatAsync(), await kết quả
  InShopState       → mở ShopSystem, đợi player exit
  InRestState       → xử lý Rest, đợi player chọn attribute
  InEventState      → xử lý Event, đợi player confirm
  InRewardState     → hiển thị reward, đợi player chọn

Transitions (AddAnyTransition):
  → InCombatState   : khi NodeRouter.LastEnteredNode == Combat
  → InShopState     : khi NodeRouter.LastEnteredNode == Shop
  → InRestState     : khi NodeRouter.LastEnteredNode == Rest
  → InEventState    : khi NodeRouter.LastEnteredNode == Event
  → InMapState      : khi current scene = Map và không có node đang active
```

**MonoBehaviour driver:**

```csharp
// Assets/Scripts/Unity/Core/GameManagerDriver.cs
public class GameManagerDriver : MonoBehaviour
{
    private StateMachineManager _fsm;
    private GameManager _gameManager;

    private void Awake()
    {
        _gameManager = new GameManager();
        _fsm = _gameManager.BuildStateMachine(); // GameManager tự setup FSM
    }

    private void Update()   => _fsm.Tick();
    private void FixedUpdate() => _fsm.FixedTick();
}
```

---

## 3. UI Layer

### Nguyên tắc

**Observer pattern — một chiều:** Logic phát event, UI lắng nghe và tự cập nhật. Logic Layer (Combat, Map, Meta) không import, không reference, không gọi bất kỳ class UI nào. Đây là ranh giới cứng nhất trong toàn bộ architecture.

**Input không validate ở UI:** Khi player tap Spell button, UI không tự kiểm tra MP hay cooldown. Nó gọi `SpellCaster.TryCast(spellId)` và nhận `CastResult`. UI chỉ phản hồi kết quả (animation thành công, shake animation nếu thất bại).

**Unsubscribe bắt buộc:** Mọi View phải unsubscribe toàn bộ event khi bị destroy — tương tự quy tắc `IRunePassive.OnPurge()`. Dùng `OnDestroy()` trong Unity MonoBehaviour.

---

### `CastResult`

Kiểu trả về của `SpellCaster.TryCast()` — UI dùng để quyết định phản hồi thế nào.

```
enum CastResult {
    Success,
    NotYourTurn,       // không phải Action Phase của Player
    SpellOnCooldown,   // spell đang cooldown
    NotEnoughMp,       // không đủ mana
    SpellNotImprinted  // spell không có trong slot
}
```

---

### Events do Logic Layer phát

Các C# event mà UI subscribe vào. Logic Layer phát, UI nhận — không chiều ngược lại.

**`PlayerController` phát:**

|Event|Khi nào|Tham số|
|---|---|---|
|`OnHpChanged`|HP thay đổi (damage, heal)|`(int current, int max)`|
|`OnMpChanged`|MP thay đổi (cast, recovery)|`(int current, int max)`|
|`OnArmorChanged`|Armor stack thay đổi|`(int totalArmor)`|

**`EffectSystem` phát (per entity):**

|Event|Khi nào|Tham số|
|---|---|---|
|`OnEffectApplied`|Buff/debuff được apply|`(EffectType)`|
|`OnEffectRemoved`|Buff/debuff hết hạn hoặc bị giải|`(EffectType)`|

**`TurnManager` phát:**

|Event|Khi nào|Tham số|
|---|---|---|
|`OnPhaseChanged`|Chuyển phase|`(Phase, EntityType owner)`|
|`OnCombatEnded`|Combat kết thúc|`(CombatResult: Win/Lose)`|

**`CooldownTracker` phát:**

|Event|Khi nào|Tham số|
|---|---|---|
|`OnCooldownChanged`|Cooldown của spell thay đổi|`(SpellID, int remaining)`|

**`EnemyController` phát:**

|Event|Khi nào|Tham số|
|---|---|---|
|`OnHpChanged`|Enemy HP thay đổi|`(int current, int max)`|
|`OnArmorChanged`|Enemy Armor thay đổi|`(int totalArmor)`|
|`OnDied`|Enemy HP về 0|—|

**`GameManager` phát:**

|Event|Khi nào|Tham số|
|---|---|---|
|`OnPendingModifierChanged`|`pendingCombatModifier` thay đổi|`(CombatModifier?)`|
|`OnGoldChanged`|Gold thay đổi|`(int newBalance)`|

---

### Views theo Scene

Architecture doc định nghĩa ranh giới trách nhiệm của từng View, không đi vào chi tiết widget. Chi tiết layout và visual là việc của UI Design doc riêng.

#### CombatScene Views

**`PlayerStatusView`**

- Subscribe: `PlayerController.OnHpChanged`, `OnMpChanged`, `OnArmorChanged`
- Hiển thị: HP bar, MP bar, Armor indicator
- Không có input

**`EnemyStatusView`** (một instance per enemy)

- Subscribe: `EnemyController.OnHpChanged`, `OnArmorChanged`, `OnDied`
- Hiển thị: HP bar, Armor indicator, intent (spell enemy sắp dùng — xem lưu ý bên dưới)
- Không có input

**`EffectView`** (một instance per entity)

- Subscribe: `EffectSystem.OnEffectApplied`, `OnEffectRemoved`
- Hiển thị: Danh sách icon buff/debuff đang active, duration còn lại

**`SpellBarView`**

- Subscribe: `CooldownTracker.OnCooldownChanged`, `TurnManager.OnPhaseChanged`
- Hiển thị: Các spell đang imprint, cooldown overlay, disable khi không phải Action Phase của Player
- Input: Player tap spell → gọi `SpellCaster.TryCast(spellId)` → nhận `CastResult` → phản hồi

**`TurnIndicatorView`**

- Subscribe: `TurnManager.OnPhaseChanged`
- Hiển thị: Lượt của ai, phase nào đang active

**`CombatLogView`** _(optional)_

- Subscribe: nhiều event
- Hiển thị: Text log các action trong combat ("Enemy cast Fireball — 45 damage")

> **Enemy Intent:** Hiển thị spell enemy _sắp_ dùng là UX tốt cho turn-based game (xem: Slay the Spire). Điều này yêu cầu `EnemyController` expose `GetIntendedSpell(context)` sau khi `DecisionPolicy` đã chọn nhưng chưa thực thi. Cần thống nhất với GD xem có muốn feature này không trước khi implement.

---

#### MapView Views

**`MapGraphView`**

- Đọc: `MapSystem` graph (static sau khi sinh) + `GameManager.visitedNodes`
- Hiển thị: Đồ thị Node, đường đã đi, các Node kề phía trước có thể chọn, icon loại Node
- Input: Player tap Node → gọi `NodeRouter.EnterNode(nodeId)`

**`PendingModifierView`**

- Subscribe: `GameManager.OnPendingModifierChanged`
- Hiển thị: Icon + tooltip khi `pendingCombatModifier != null`, ẩn khi null
- Không có input

**`PlayerInfoView`** (compact, góc màn hình)

- Subscribe: `PlayerController.OnHpChanged`, `GoldLedger.OnBalanceChanged`
- Hiển thị: HP hiện tại, Gold

---

#### ShopScene Views

**`ShopInventoryView`**

- Đọc: `ShopSystem.currentInventory` (static trong session)
- Hiển thị: 15 item card chia 3 gian, giá, rank
- Input: Player tap item → gọi `ShopSystem.TryPurchase(itemId)` → nhận `PurchaseResult`

**`ShopServiceView`**

- Hiển thị: Các dịch vụ (Enlighten, Embed, Purge, Socket), giá, điều kiện khả dụng
- Input: Player chọn dịch vụ → gọi API tương ứng trên `ShopSystem`

**`PlayerEquipmentView`** (hiển thị equipment + rune đang mang)

- Subscribe: `EquipmentSystem`, `RuneSystem` khi có thay đổi
- Hiển thị: 5 equipment slot, 4 rune socket, stat tổng hiện tại

```
enum PurchaseResult {
    Success,
    NotEnoughGold,
    InventoryFull,      // không có slot equipment/rune tương ứng
    ServiceUnavailable  // VD: Enlighten nhưng không đủ WIS
}
```

---

#### RewardScreen View

**`RewardChoiceView`**

- Đọc: `RewardSystem.currentOffer` (3 item)
- Hiển thị: 3 card (Equipment / Spell / Rune), stat/effect của từng item
- Input: Player tap 1 card → gọi `RewardSystem.SelectReward(index)` → về MapView

---

#### EventScene View

**`EventView`**

- Đọc: `EventSystem.currentEvent`
- Hiển thị: Tên event, mô tả, các lựa chọn (nếu event có choice)
- Input: Player xác nhận / chọn option → gọi `EventSystem.ResolveEvent(choice)`

---

#### RestScene View

**`RestView`**

- Hiển thị: Xác nhận hồi HP, danh sách 5 attribute để chọn +1
- Input: Player chọn attribute → gọi `RestNode.ApplyRest(attributeType)` → về MapView

---

### Unsubscribe Pattern (bắt buộc)

Mọi View MonoBehaviour phải unsubscribe trong `OnDestroy()`:

```
// Ví dụ PlayerStatusView
void OnEnable() {
    PlayerController.OnHpChanged += UpdateHpBar;
    PlayerController.OnMpChanged += UpdateMpBar;
}

void OnDestroy() {
    PlayerController.OnHpChanged -= UpdateHpBar;
    PlayerController.OnMpChanged -= UpdateMpBar;
}
```

Nếu không unsubscribe: View bị destroy nhưng delegate vẫn còn trong event list → `MissingReferenceException` hoặc memory leak, tùy Unity version.

---

## 4. Entry Point

### `GameManager`

**Trách nhiệm:** Quản lý trạng thái toàn bộ một run và điều phối việc chuyển đổi giữa các scene.

**State machine của một run:**

```
Idle
  └─[Start Run]──► MapView
                      ├─[Enter Combat Node]──► CombatScene
                      │                            ├─[Win]──► RewardScreen ──► MapView
                      │                            └─[Lose]──► GameOver
                      ├─[Enter Shop Node]────► ShopScene ──► MapView
                      ├─[Enter Rest Node]────► RestScene ──► MapView
                      ├─[Enter Event Node]───► EventScene ──► MapView
                      └─[Complete Arc 3]─────► RunComplete
```

**Dữ liệu do `GameManager` giữ:**

|Dữ liệu|Mô tả|
|---|---|
|`currentArc`|Arc hiện tại (1, 2, 3)|
|`playerSnapshot`|Toàn bộ trạng thái player (attributes, inventory, equipped items, runes)|
|`gold`|Số vàng hiện có — chỉ modify qua `GoldLedger`|
|`visitedNodes`|Tập các Node đã đi qua (để tránh quay lại)|
|`pendingCombatModifier`|Modifier tạm thời từ Event (Force Trade, Fading Curse) — áp dụng cho trận tiếp theo|

**`pendingCombatModifier`:**

- Nullable. Khi `CombatScene` khởi động, nó đọc giá trị này, áp dụng vào combat, rồi xóa sau khi trận kết thúc.
- **UI requirement:** MapView phải hiển thị icon/tooltip khi `pendingCombatModifier != null` để player biết họ đang mang modifier vào trận tiếp theo. Đây là trách nhiệm của UI Layer — data đã có sẵn trong `GameManager`.

---

## 5. Map Layer

### `MapSystem`

**Trách nhiệm:** Sinh đồ thị Node cho từng Arc theo các ràng buộc thiết kế, lưu kết quả để render lên màn hình bản đồ.

**Input:** Cấu hình Arc (tổng số Node, tỷ lệ từng loại, ràng buộc đường đi tối thiểu) — đọc từ ScriptableObject.

**Output:** Một đồ thị có hướng `Graph<Node>` với các cạnh đã được xác định.

**Ràng buộc sinh map (áp dụng cho mọi đường dẫn đến Boss cuối):**

|Arc|Tổng Node|Min path|Elite tối thiểu|Rest tối thiểu|Shop tối thiểu|Event tối thiểu|Optional Boss|
|---|---|---|---|---|---|---|---|
|Arc 1|50|15|1|2|2|2|1|
|Arc 2|75|20|2|2|2|3|2|
|Arc 3|100|25|2|3|3|3|3|

> **Optional Boss:** Ngoài Boss bắt buộc ở Node cuối cùng, các Boss Node khác trên map là Optional Boss. Dùng cùng `NodeType.Boss`, phân biệt bằng vị trí trong graph (không phải Node cuối = Optional). Gold reward và item reward của Optional Boss và Mandatory Boss có thể khác nhau — xác định bởi `RewardRateConfig`.

**Thuật toán sinh (tham khảo — chi tiết trong Tech Design — Map Generation):**

1. Chia Node thành các "hàng" (row) theo chiều sâu.
2. Phân bổ loại Node theo tỷ lệ cấu hình vào từng hàng, đảm bảo ràng buộc tối thiểu.
3. Tạo cạnh ngẫu nhiên giữa các hàng kề nhau (mỗi Node có 1–3 cạnh ra).
4. **Đảm bảo không có 2 Node cùng loại liên tiếp trên cùng một path, trừ Combat Minion Node.**
5. Validate tất cả đường dẫn có thể đến Boss cuối đều thỏa ràng buộc — nếu không, regenerate.

### `NodeRouter`

**Trách nhiệm:** Khi player chọn đi vào một Node, `NodeRouter` nhận loại Node và gọi đúng hệ thống tương ứng.

```
player chọn Node
  ├── NodeType.MINION / ELITE / BOSS  ──► khởi động CombatLayer (truyền EnemyDefinition)
  ├── NodeType.SHOP                   ──► khởi động ShopSystem
  ├── NodeType.REST                   ──► khởi động RestNode handler
  └── NodeType.EVENT                  ──► khởi động EventSystem
```

---

## 6. Combat Layer

### `TurnManager`

**Trách nhiệm:** Điều phối vòng lặp lượt giữa Player và Enemy bằng `async UniTask`. Phase chỉ tiếp tục sau khi `VisualQueue.DrainAsync()` hoàn thành.

**Vòng lặp — async UniTask:**

```csharp
public async UniTask StartCombatAsync(CancellationToken ct)
{
    while (!IsCombatOver() && !ct.IsCancellationRequested)
    {
        // Player Turn
        await RunPhaseAsync(Phase.Start, EntityType.Player, ct);
        await visualQueue.DrainAsync(ct);

        await RunPhaseAsync(Phase.Action, EntityType.Player, ct);
        await visualQueue.DrainAsync(ct);

        await RunPhaseAsync(Phase.End, EntityType.Player, ct);
        await visualQueue.DrainAsync(ct);

        if (IsCombatOver()) break;

        // Enemy Turn
        await RunPhaseAsync(Phase.Start, EntityType.Enemy, ct);
        await visualQueue.DrainAsync(ct);

        await RunPhaseAsync(Phase.Action, EntityType.Enemy, ct);
        await visualQueue.DrainAsync(ct);

        await RunPhaseAsync(Phase.End, EntityType.Enemy, ct);
        await visualQueue.DrainAsync(ct);
    }

    var result = AllEnemiesDead() ? CombatResult.Win : CombatResult.Lose;
    OnCombatEnded?.Invoke(result);   // C# event → EventBridge → CombatEndedSO
}
```

**Tại sao không dùng StateMachine cho phase:**  
Phase Start→Action→End là sequential flow — mỗi phase phải hoàn thành (kể cả animation) trước khi phase sau bắt đầu. StateMachine với `Tick()` là synchronous và không thể `await` bên trong. Dùng `StateMachineManager` ở đây sẽ cần flag phức tạp để bridge async/sync — UniTask `await` thẳng clean hơn nhiều.

**Cancellation:** `CancellationToken` truyền vào để có thể cancel combat loop khi cần (game over, scene unload).

**`OnPhaseChanged` event** phát tại đầu mỗi `RunPhaseAsync()` — UI subscribe để cập nhật turn indicator.

**Điều kiện kết thúc combat:**

- **Win:** Tất cả Enemy `currentHp <= 0`.
- **Lose:** Player `currentHp <= 0`.

Sau khi kết thúc, `OnCombatEnded` C# event phát → `CombatEndedBridge` relay lên `CombatEndedSO` → `GameManager` state machine nhận và chuyển state.

---

### `PhaseHandler`

**Trách nhiệm:** Thực thi từng Phase theo đúng resolve order. Mỗi bước tính logic xong → đẩy Command tương ứng vào `VisualQueue`.

**Start Phase — resolve order:**

|Thứ tự|Hành động|Visual Command đẩy vào Queue|
|---|---|---|
|1|MP Recovery|`ShowMpGain(entity, amount)`|
|2|Frozen check|Set flag skip Action Phase, giải Frozen|
|3|Crystalize check|Bật flag miễn nhiễm damage + debuff|
|4|Regen|Hồi HP|
|5|Burn (DoT)|Gây damage qua `CombatResolver.ResolveBurn()`. Bỏ qua nếu Crystalize flag bật|
|6|Các status còn lại|Enrage, Drenched, Chilled, Dazed, Fortified, Energized|
|7|Combination check|`CombatResolver.CheckCombinations(entity)`|

**End Phase — resolve order:**

|Thứ tự|Hành động|
|---|---|
|1|Giảm duration tất cả Effect|
|2|Xóa Effect hết duration|
|3|Giảm duration Armor stack|
|4|Xóa Armor stack hết duration|
|5|Giảm cooldown tất cả Spell|

---

### `CombatResolver`

**Trách nhiệm:** Mediator giữa `EffectSystem` và `DamageCalculator`. Xử lý các tình huống yêu cầu cả hai hệ thống phối hợp, tránh circular dependency.

**Lý do tồn tại:** `EffectSystem` không được phép gọi trực tiếp `DamageCalculator` — nếu vậy sẽ tạo vòng tròn `EffectSystem → DamageCalculator → EffectSystem` (khi damage trigger effect mới). `CombatResolver` đứng ngoài cả hai, subscribe event từ `EffectSystem` và điều phối `DamageCalculator`.

**Detonates trigger:**

```
EffectSystem phát: OnCombinationTriggered(Detonates, target)
  │
  CombatResolver nhận
  ├── damage = 30% × target.maxHp
  ├── DamageCalculator.ApplyRawDamage(target, damage, ignoreResistance=true, ignoreArmor=true)
  ├── VisualQueue.Enqueue: PlayDetonatesAnim(target), ShowDamageNumber(target, damage)
  └── Không gọi lại EffectSystem (tránh loop)
```

**Burn DoT (gọi bởi PhaseHandler):**

```
CombatResolver.ResolveBurn(entity):
  ├── damage = 10% × entity.maxHp
  ├── Kiểm tra Crystalize flag → return nếu bật
  ├── actual = DamageCalculator.ApplyWithResistance(entity, damage, fire_res)
  └── Gọi DamageCalculator.ApplyToArmor(entity, actual)
```

**CheckCombinations:**

```
CombatResolver.CheckCombinations(entity):
  ├── Đọc entity.EffectSystem.activeEffects
  ├── Enrage + Energized → Remove(Enrage), Remove(Energized), Apply(Overdrive)
  ├── Refreshing + Fortified → Remove(Refreshing), Remove(Fortified), Apply(Crystalize)
  ├── Burn + Dazed → Remove(Burn), Remove(Dazed), trigger Detonates
  └── Drenched + Chilled → Remove(Drenched), Remove(Chilled), Apply(Frozen)
```

---

### `EffectSystem`

**Trách nhiệm:** Quản lý toàn bộ buff và debuff trên một entity. Không tự gọi `DamageCalculator` — mọi damage đi qua `CombatResolver`.

**`EffectType` enum — định nghĩa tất cả effect trong game:**

```
enum EffectType {
    None,
    // Buffs (Fire/Water/Ice/Lightning self-cast)
    Enrage, Refreshing, Fortified, Energized,
    // Combo buffs
    Overdrive, Crystalize,
    // Standalone buff
    Regen,
    // Debuffs (hit by element)
    Burn, Drenched, Chilled, Dazed,
    // Combo debuffs
    Detonates, Frozen,
    // Independent debuff
    Distracted
}
```

**Cấu trúc nội bộ:**

```
EffectSystem(entity) {
    activeEffects           : Dictionary<EffectType, EffectInstance>
    OnEffectApplied         : event Action<EffectType>
    OnEffectRemoved         : event Action<EffectType>
    OnCombinationTriggered  : event Action<ComboType, entity>
}
```

**Logic tương tác nguyên tố khi apply Effect mới A:**

|Tình huống|Điều kiện|Kết quả|
|---|---|---|
|A khắc B đang active|A.element > B.element|B bị giải, A được apply|
|B khắc A|B.element > A.element|A bị giải. Abort.|
|Cùng nguyên tố, khác loại (Buff + Debuff)|A.element == B.element, A.type != B.type|**Neutralize** cả hai. Abort.|
|Cùng nguyên tố, cùng loại (Buff + Buff hoặc Debuff + Debuff)|A.element == B.element, A.type == B.type|**Refresh** duration của B. Abort.|

> **Ví dụ làm rõ:**
> 
> - Player có `Enrage` (Fire Buff), bị trúng Fire debuff `Burn` → cùng nguyên tố khác loại → **Neutralize**: mất cả hai.
> - Enemy bị `Burn`, bị trúng Fire spell thêm lần nữa → cùng nguyên tố cùng loại → **Refresh**: Burn vẫn còn, reset duration.
> - Player có `Enrage`, tự cast Fire spell lên bản thân → non-stackable, cùng loại → **Refresh**.

**Bảng Effect:**

|Effect|Loại|On Same-type Collision|Duration|Trigger|
|---|---|---|---|---|
|Enrage|Buff|Refresh|∞|Dùng phép Lửa lên bản thân|
|Refreshing|Buff|Refresh|∞|Dùng phép Nước lên bản thân|
|Fortified|Buff|Refresh|∞|Dùng phép Băng lên bản thân|
|Energized|Buff|Refresh|∞|Dùng phép Sét lên bản thân|
|Overdrive|Buff|Refresh|This turn|Enrage + Energized|
|Crystalize|Buff|Refresh|Next turn|Refreshing + Fortified|
|Regen|Buff|Self-stackable|Depends|Spell / Item|
|Burn|Debuff|Refresh|∞|Trúng phép Lửa|
|Drenched|Debuff|Refresh|∞|Trúng phép Nước|
|Chilled|Debuff|Refresh|∞|Trúng phép Băng|
|Dazed|Debuff|Refresh|∞|Trúng phép Sét|
|Detonates|Instant|—|Instant|Burn + Dazed|
|Frozen|Debuff|Refresh|Next turn|Drenched + Chilled|
|Distracted|Debuff|Depends|Depends|Spell / Item|

---

### `DamageCalculator`

**Trách nhiệm:** Tính toán và áp dụng sát thương. Không biết đến `EffectSystem` — chỉ nhận số liệu thuần từ caller.

**Pipeline đòn tấn công thông thường:**

```
1. raw_damage = base_value × caster.GetEffectivePotency(element)
2. R = target.GetEffectiveResistance(element)
3. actual_damage = raw_damage × 90 / (R + 90)
4. Nếu target.crystalizeFlag == true → actual_damage = 0
5. overflow = ArmorStack.TakeDamage(actual_damage)
6. target.currentHp -= overflow
```

**ApplyRawDamage (dùng bởi CombatResolver cho Detonates):**

```
ApplyRawDamage(target, damage, ignoreResistance, ignoreArmor):
    actual = ignoreResistance ? damage : damage × 90 / (R + 90)
    if ignoreArmor:
        target.currentHp -= actual
    else:
        overflow = ArmorStack.TakeDamage(actual)
        target.currentHp -= overflow
```

---

### `HitDodgeResolver`

**Trách nhiệm:** Xác định đòn tấn công có trúng không.

**Công thức:**

```
hit_delta = AGI(attacker) - AGI(defender)

if hit_delta >= HIT_THRESHOLD → guaranteed hit
else:
    dodge_chance = clamp(BASE_DODGE × (1 - hit_delta / HIT_THRESHOLD), 0, MAX_DODGE)
    roll random → miss nếu < dodge_chance
```

Hằng số đọc từ `CombatConfig` ScriptableObject. Khi Overdrive active → AGI(attacker) = ∞, guaranteed hit.

---

## 7. Presentation Layer

**Trách nhiệm:** Nhận `ActionCommand` từ Combat Layer, thực thi tuần tự bằng UniTask. Mọi animation dùng DOTween. `TurnManager` `await DrainAsync()` trước khi chuyển phase.

### `VisualQueue`

```csharp
public class VisualQueue : MonoBehaviour
{
    private readonly Queue<IActionCommand> _queue = new();
    private bool _isDraining;
    private UniTaskCompletionSource _drainTcs;

    public void Enqueue(IActionCommand command)
    {
        _queue.Enqueue(command);
    }

    // Gọi bởi TurnManager sau mỗi phase logic
    public async UniTask DrainAsync(CancellationToken ct = default)
    {
        while (_queue.Count > 0 && !ct.IsCancellationRequested)
        {
            var command = _queue.Dequeue();
            await command.ExecuteAsync(ct);
        }
    }
}
```

**Điểm khác biệt so với Coroutine cũ:**

- Không cần `OnQueueDrained` event — `TurnManager` `await DrainAsync()` trực tiếp, code tuyến tính hơn.
- `CancellationToken` cho phép abort toàn bộ queue khi scene unload hoặc game over.
- Nếu muốn chạy một số command song song (VD: nhiều floating text cùng lúc), dùng `UniTask.WhenAll()` thay vì enqueue tuần tự.

---

### `IActionCommand`

```csharp
public interface IActionCommand
{
    UniTask ExecuteAsync(CancellationToken ct = default);
}
```

**Thay `IEnumerator Execute()` → `UniTask ExecuteAsync()`** — loại bỏ hoàn toàn Coroutine trong Presentation Layer.

**Các command và cách dùng DOTween:**

|Command|DOTween / UniTask|
|---|---|
|`ShowDamageNumberCommand`|`text.DOCounter(0, amount, 0.3f).ToUniTask(ct)`|
|`PlaySpellAnimCommand`|`transform.DOPunchScale(...)` + `await UniTask.Delay(ms, ct)`|
|`PlayDamageAnimCommand`|`transform.DOShakePosition(...)`.ToUniTask(ct)`|
|`ShowHpGainCommand`|`DOTween.To(() => hp, x => hp = x, target, 0.4f).ToUniTask(ct)`|
|`PlayDetonatesAnimCommand`|Sequence: `DOScale` + `DOFade` → `.ToUniTask(ct)`|
|`PlayDeathAnimCommand`|`transform.DOScale(0, 0.3f).SetEase(Ease.InBack).ToUniTask(ct)`|
|`PlayEffectApplyAnimCommand`|Instantiate VFX prefab, `await UniTask.Delay(duration, ct)`, Destroy|
|`PlayFrozenThawAnimCommand`|DOTween color tween từ xanh → normal|

**Cách dùng DOTween với UniTask:**

```csharp
// Bridge DOTween → UniTask: dùng package Cysharp/UniTask DOTween integration
// hoặc extension method tự viết:
public static UniTask ToUniTask(this Tween tween, CancellationToken ct = default)
{
    var tcs = new UniTaskCompletionSource();
    tween.OnComplete(() => tcs.TrySetResult());
    ct.RegisterWithoutCapturedSynchronizationContext(() => {
        tween.Kill();
        tcs.TrySetCanceled();
    });
    return tcs.Task;
}

// Dùng trong command:
public async UniTask ExecuteAsync(CancellationToken ct)
{
    await transform.DOShakePosition(0.3f, strength: 0.2f)
                   .SetEase(Ease.OutBounce)
                   .ToUniTask(ct);
}
```

**Command song song (parallel):**

```csharp
// Enqueue một ParallelCommand thay vì nhiều command riêng lẻ
public class ParallelCommand : IActionCommand
{
    private readonly IActionCommand[] _commands;
    public ParallelCommand(params IActionCommand[] commands) => _commands = commands;

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        await UniTask.WhenAll(_commands.Select(c => c.ExecuteAsync(ct)));
    }
}

// Ví dụ: Detonates — VFX nổ + floating text cùng lúc
visualQueue.Enqueue(new ParallelCommand(
    new PlayDetonatesAnimCommand(target),
    new ShowDamageNumberCommand(target, damage)
));
```

---

## 8. Entity Layer

### `IEntity` — Interface chung cho Entity

**Trách nhiệm:** Định nghĩa contract chung cho tất cả entity (Player, Enemy) để các hệ thống (`TargetResolver`, `SpellEffect`, `SpellCondition`, `DamageCalculator`) sử dụng mà không cần biết implementation cụ thể.

```
interface IEntity {
    CurrentHp   : int { get; set; }
    MaxHp       : int { get; }

    HasEffect(effectType: EffectType) → bool
    GetEffectivePotency(element: Element) → float
    GetEffectiveResistance(element: Element) → float
    GetEffectiveAGI() → float
}
```

> **Mở rộng theo TASK:** Property `ArmorStack`, `EffectSystem` sẽ bổ sung vào `IEntity` khi implement TASK-020 (ArmorStack) và TASK-030 (EffectSystem). `PlayerController` và `EnemyController` implement `IEntity`.

---

### Stat Layering Model _(Thêm mới v4)_

Mọi stat của player được tính theo **4 layer** theo thứ tự nghiêm ngặt. Layer thấp hơn tính trước, layer cao hơn áp lên kết quả.

```
Layer 0 — Formula Base
    Kết quả trực tiếp từ formula Main Attribute.
    Tính một lần khi RecalculateBaseAttributes() chạy.
    Ví dụ: fire_potency_L0 = POT_total × pot_scale
            max_hp_L0       = 1000 × VIT_total / (VIT_total + 90)
            max_mp_L0       = 10 × SPI_total

Layer 1 — Flat Bonus
    Cộng thẳng từ equipment hoặc rune (không qua formula).
    Tính cùng lúc với Layer 0 trong RecalculateBaseAttributes().
    Ví dụ: max_hp_L1       = sum của flat MaxHp modifiers từ equipment/rune
            fire_potency_L1 = sum của flat FirePotency modifiers

    → base_value = L0 + L1

Layer 2 — Percent Additive Bonus
    Nhân % lên (L0 + L1). Equipment rank cao và Rune có thể cung cấp.
    Tính cùng lúc trong RecalculateBaseAttributes().
    Ví dụ: fire_potency_L2 = sum của percent FirePotency modifiers

    → stored_value = (L0 + L1) × (1 + L2)
    Đây là giá trị lưu vào baseXValue.

Layer 3 — Combat Effect Multiplier
    Nhân % từ Enrage / Drenched / Fortified / Dazed / Energized / Chilled / Overdrive.
    KHÔNG tính trong RecalculateBaseAttributes() — tính REALTIME trong GetEffective*().
    Ví dụ: effective_fire_potency = stored_fire_potency × (1 + enrage_bonus - drenched_penalty)
```

**Tóm tắt:**

```
stored_value    = (L0 + L1) × (1 + L2)   ← tính trong RecalculateBaseAttributes()
effective_value = stored_value × L3       ← tính realtime trong GetEffective*()
```

**Lý do tách biệt L2 (rune/equipment %) và L3 (combat effect %):**  
L2 là bonus cố định theo build của player — ổn định suốt run. L3 là trạng thái combat tạm thời — thay đổi theo từng lượt. Gộp chung vào một chỗ sẽ làm mất đi sự tách biệt này và khó debug khi số liệu sai.

---

### `StatModifier` _(Thêm mới v4)_

Kiểu dữ liệu chung dùng cho cả `EquipmentDefinition` và `RuneDefinition` để mô tả một stat modifier.

```
StatModifier {
    stat   : StatType
    modType: ModType   // Flat | Percent
    value  : float
}

enum ModType {
    Flat,        // Cộng thẳng vào Layer 1
    Percent      // Cộng vào Layer 2 (% additive)
}
```

**`StatType` enum — toàn bộ stat có thể modify:**

```
enum StatType {
    // Main Attributes
    POT, SPI, WIS, VIT, AGI,

    // Sub Attributes — Potency (flat)
    FirePot, WaterPot, IcePot, LightningPot,

    // Sub Attributes — Resistance (flat)
    FireRes, WaterRes, IceRes, LightningRes,

    // Resources (flat)
    MaxHp, MaxMp,

    // Derived
    MpRecovery,       // flat bonus cộng thêm vào turn_mp_recovery
    AllPot,           // shorthand: apply cho cả 4 nguyên tố cùng lúc
    AllRes            // shorthand: apply cho cả 4 nguyên tố cùng lúc
}
```

**Ví dụ Equipment Rank III:**

```
Staff of Wildfire (Rank III):
  modifiers: [
    { stat: POT,     modType: Flat,    value: 8  },
    { stat: FirePot, modType: Flat,    value: 15 },
    { stat: FirePot, modType: Percent, value: 0.10 }  // +10%
  ]
```

**Ví dụ Equipment cộng MaxHp:**

```
Warlock's Garb (Rank II):
  modifiers: [
    { stat: VIT,   modType: Flat, value: 5  },
    { stat: MaxHp, modType: Flat, value: 50 }  // +50 HP ngoài formula
  ]
```

---

### `PlayerController`

**Trách nhiệm:** Lưu trữ và cung cấp toàn bộ trạng thái hiện tại của player trong combat.

**Dữ liệu quản lý:**

```
PlayerController {
    // Main Attributes — tổng sau khi cộng Flat modifier từ equipment/rune
    POT, SPI, WIS, VIT, AGI : int

    // Attribute Point Pool — điểm chưa phân bổ
    availableAttributePoints : int   // số điểm có thể cộng vào Main Attribute
    // Earn: RestNode (+1), GiveAttributeAction (+N)
    // Spend: player chọn attribute → SpendAttributePoint(statType)

    // Stored values — kết quả của Layer 0 + Layer 1 + Layer 2
    // Chỉ recalculate khi equip/unequip/embed/purge, KHÔNG recalculate trong combat
    storedFirePotency, storedWaterPotency, storedIcePotency, storedLightningPotency : float
    storedFireRes, storedWaterRes, storedIceRes, storedLightningRes                 : float
    storedMaxHp   : int     // kết quả formula(VIT) + flat MaxHp bonus
    storedMaxMp   : int     // kết quả formula(SPI) + flat MaxMp bonus
    storedMpRecovery : float

    // Resources (runtime)
    currentHp, currentMp   : int
    maxHp => storedMaxHp   // readonly alias
    maxMp => storedMaxMp   // readonly alias
    crystalizeFlag         : bool

    // Combat state
    effectSystem   : EffectSystem
    armorStack     : ArmorStack
    spellSlots     : SpellSlotManager
    equippedItems  : Equipment[5]
    embeddedRunes  : Rune[4]

    // --- Methods ---

    HasAvailablePoints() → bool:
        return availableAttributePoints > 0

    SpendAttributePoint(statType: StatType) → bool:
        if availableAttributePoints <= 0: return false
        if statType not in {POT, SPI, WIS, VIT, AGI}: return false
        mainAttributes[statType] += 1
        availableAttributePoints -= 1
        RecalculateBaseAttributes()
        return true
}
```

**`RecalculateBaseAttributes()` — thứ tự tính theo layer:**

```
RecalculateBaseAttributes():

  // Bước 1: Thu thập tất cả StatModifier từ equipment và rune
  allModifiers = equippedItems.flatMap(e => e.modifiers)
               + embeddedRunes.flatMap(r => r.modifiers nếu passiveType == StatModifier)

  // Bước 2: Tính Main Attributes (L0 base + L1 flat modifier)
  POT = BASE_POT + sum(allModifiers where stat==POT and modType==Flat)
  SPI = BASE_SPI + sum(...)
  VIT = BASE_VIT + sum(...)
  // ... tương tự cho WIS, AGI

  // Bước 3: Tính stored potency theo từng nguyên tố
  for each element in [Fire, Water, Ice, Lightning]:
    L0 = pot_scale × POT                              // formula base
    L1 = sum(flat potency modifiers cho element này)
       + sum(flat AllPot modifiers)
    L2 = sum(percent potency modifiers cho element này)
       + sum(percent AllPot modifiers)
    storedXPotency = (L0 + L1) × (1 + L2)

  // Bước 4: Tính stored resistance
  for each element in [Fire, Water, Ice, Lightning]:
    L0 = res_scale × VIT
    L1 = sum(flat res modifiers cho element này)
       + sum(flat AllRes modifiers)
    L2 = sum(percent res modifiers cho element này)
       + sum(percent AllRes modifiers)
    storedXRes = (L0 + L1) × (1 + L2)

  // Bước 5: Tính maxHp và maxMp
  L0_hp = 1000 × VIT / (VIT + 90)
  L1_hp = sum(flat MaxHp modifiers)
  storedMaxHp = L0_hp + L1_hp       // Không có L2 cho HP (thiết kế hiện tại)

  L0_mp = 10 × SPI
  L1_mp = sum(flat MaxMp modifiers)
  storedMaxMp = L0_mp + L1_mp

  // Bước 6: Tính MpRecovery
  storedMpRecovery = BASE_MP_RECOVERY + SPI + sum(flat MpRecovery modifiers)
```

> **Không có L2 (percent) cho MaxHp và MaxMp trong thiết kế hiện tại** — giữ đơn giản. Nếu sau này cần thêm, chỉ cần thêm case vào bước 5.

**Dynamic getter — Layer 3, tính realtime lúc cast/damage:**

```
GetEffectivePotency(element: Element) → float:
    base = storedXPotency tương ứng
    multiplier = 1.0
    if effectSystem.Has(Enrage):    multiplier += 0.15
    if effectSystem.Has(Drenched):  multiplier -= 0.10
    if effectSystem.Has(Overdrive): multiplier += 0.50
    return base × multiplier

GetEffectiveResistance(element: Element) → float:
    base = storedXRes tương ứng
    multiplier = 1.0
    if effectSystem.Has(Fortified): multiplier += 0.15
    if effectSystem.Has(Dazed):     multiplier -= 0.15
    return base × multiplier

GetEffectiveAGI() → float:
    if effectSystem.Has(Overdrive): return float.PositiveInfinity
    base = (float)AGI
    if effectSystem.Has(Energized): base *= 1.30
    if effectSystem.Has(Chilled):   base *= 0.70
    return base
```

> **Layer 3 chỉ chứa combat effect** (Enrage, Drenched, Fortified, Dazed, Energized, Chilled, Overdrive). Equipment và Rune không bao giờ modify Layer 3 — chúng đã được xử lý ở Layer 1 và 2 trong `RecalculateBaseAttributes()`.

---

### `EnemyController`

**Trách nhiệm:** Lưu trạng thái runtime của một enemy instance và thực thi hành vi thông qua Enemy AI Subsystem.

**Dữ liệu quản lý:**

```
EnemyController {
    // Từ EnemyDefinition (immutable)
    maxHp                                                       : int
    baseFirePotency, baseWaterPotency, baseIcePotency, baseLightningPotency : float
    baseFireRes, baseWaterRes, baseIceRes, baseLightningRes     : float

    // Runtime (mutable)
    currentHp      : int
    effectSystem   : EffectSystem
    armorStack     : ArmorStack
    spellCooldowns : Dictionary<SpellID, int>

    // AI
    spellSelector  : SpellSelector
    decisionPolicy : DecisionPolicy
}
```

Enemy cũng có `GetEffectivePotency()` và `GetEffectiveResistance()` với logic tương tự Player.

---

### Enemy AI Subsystem

**Tổng quan:** Hai tầng tách biệt — `SpellSelector` lọc spell _có thể_ cast, `DecisionPolicy` quyết định _nên_ cast cái nào. Dữ liệu AI được cấu hình trong `EnemyDefinition`, không hard-code.

#### `CombatContext`

Snapshot read-only của combat state tại thời điểm enemy cần ra quyết định. Được tạo mới mỗi lần `TurnManager` bắt đầu Enemy Action Phase.

```
CombatContext {
    // Trạng thái player
    playerHpPercent     : float    // currentHp / maxHp
    playerMpPercent     : float
    playerActiveEffects : EffectType[]  // read-only list

    // Trạng thái enemy tự thân
    selfHpPercent       : float
    selfActiveEffects   : EffectType[]

    // Thông tin spell
    availableSpells     : SpellID[]    // sau khi SpellSelector lọc
    spellCooldowns      : Dictionary<SpellID, int>

    // Thông tin combat
    roundNumber         : int
}
```

#### `SpellSelector`

**Trách nhiệm:** Lọc danh sách spell của enemy, trả về những spell _hợp lệ_ để cast trong lượt này.

**Điều kiện lọc:**

```
SpellSelector.GetAvailableSpells(enemy, context) → SpellID[]:
    return enemy.spells.Where(spell =>
        spell.cooldown == 0
    )
```

> Lưu ý: Enemy không có MP — không cần kiểm tra mana cost. Nếu thiết kế sau này thêm MP cho enemy thì bổ sung điều kiện này vào đây.

#### `DecisionPolicy`

Interface mà mọi AI policy phải implement:

```
interface DecisionPolicy {
    SelectSpell(available: SpellID[], context: CombatContext) → SpellID
}
```

**Các implementation:**

**`RandomPolicy`** — Chọn ngẫu nhiên uniform từ danh sách available. Dùng cho Minion.

```
SelectSpell(available, context):
    return available[Random(0, available.Length)]
```

**`WeightedRandomPolicy`** — Mỗi spell có weight riêng, roll theo xác suất. Dùng cho Elite đơn giản.

```
WeightedRandomPolicy {
    weights: Dictionary<SpellID, float>  // cấu hình trong EnemyDefinition
}

SelectSpell(available, context):
    filtered = available có trong weights
    roll theo weights → return spell
```

**`PriorityPolicy`** — Duyệt danh sách rule theo thứ tự ưu tiên, chọn rule đầu tiên thỏa điều kiện. Dùng cho Elite phức tạp và Boss.

```
PriorityPolicy {
    rules: PriorityRule[]  // sorted by priority, cấu hình trong EnemyDefinition
}

PriorityRule {
    spellId   : SpellID
    condition : Condition   // xem bảng Condition bên dưới
    priority  : int
}

SelectSpell(available, context):
    for each rule in rules (theo priority giảm dần):
        if rule.spellId in available AND rule.condition.Evaluate(context):
            return rule.spellId
    return available[Random]  // fallback nếu không có rule nào thỏa
```

**Bảng Condition khả dụng cho PriorityPolicy:**

|Condition|Mô tả|Ví dụ dùng|
|---|---|---|
|`PlayerHpBelow(x%)`|Player HP < x%|Cast healing khi có thể exploit player yếu|
|`PlayerHpAbove(x%)`|Player HP > x%|Cast debuff setup khi player còn nhiều máu|
|`PlayerHas(effect)`|Player đang có effect X|Cast Dazed khi player đang bị Burn để trigger Detonates|
|`PlayerLacks(effect)`|Player không có effect X|Không cast Burn nếu player đã bị Burn|
|`SelfHpBelow(x%)`|Self HP < x%|Cast defensive spell khi HP thấp|
|`SelfHas(effect)`|Self đang có effect X|Không dùng nếu đang bị Drenched|
|`RoundNumberIs(n)`|Round == n|Boss phase trigger ở round cụ thể|
|`RoundNumberAbove(n)`|Round > n|Boss enrage sau round n|
|`Always`|Luôn đúng|Fallback priority thấp nhất|

**Ví dụ cấu hình PriorityPolicy cho một Elite Fire/Lightning:**

```
rules:
  [priority=10] SpellID=Lightning_Shock,  condition=PlayerHas(Burn)
    // Cố ý trigger Detonates khi player đang bị Burn
  [priority=5]  SpellID=Fire_Fireball,    condition=PlayerLacks(Burn)
    // Apply Burn nếu player chưa bị
  [priority=1]  SpellID=Lightning_Shock,  condition=Always
    // Fallback
```

**`ScriptedPolicy`** — Thực thi chuỗi spell theo thứ tự cố định, không quan tâm context. Dùng cho Boss có pattern muốn GD kiểm soát hoàn toàn.

```
ScriptedPolicy {
    sequence: SpellID[]  // cấu hình trong EnemyDefinition
    currentIndex: int    // runtime, reset khi combat bắt đầu
}

SelectSpell(available, context):
    // Tìm spell tiếp theo trong sequence còn available (chưa cooldown)
    for i in range(sequence.Length):
        idx = (currentIndex + i) % sequence.Length
        if sequence[idx] in available:
            currentIndex = (idx + 1) % sequence.Length
            return sequence[idx]
    return available[Random]  // fallback
```

> **Lưu ý thiết kế:** `DecisionPolicy` không đảm bảo tối ưu combat — nó chỉ cung cấp hành vi _có vẻ hợp lý_ theo ý GD. Việc enemy có "thông minh" hay không phụ thuộc vào cách GD cấu hình rule trong `EnemyDefinition`. Đây là điểm cần đồng bộ với **GDD — Enemies** (hiện đang [TBD]).

---

### `ArmorStack`

**Trách nhiệm:** Quản lý chuỗi Armor stack theo `hurt_order`, xử lý damage tràn.

**Cấu trúc:**

```
ArmorStack {
    stacks: SortedList<int, ArmorStackInstance>  // key = hurt_order, descending
}

ArmorStackInstance {
    value      : int
    duration   : int
    hurt_order : int
}
```

**TakeDamage:**

```
TakeDamage(damage) → overflow:
    remaining = damage
    for each stack (hurt_order cao → thấp):
        absorbed = min(stack.value, remaining)
        stack.value -= absorbed
        remaining -= absorbed
        if stack.value == 0: remove stack
    return remaining  // overflow sang HP thật
```

**ApplyArmor:**

```
ApplyArmor(value, duration):
    newOrder = stacks.MaxKey + 1 (hoặc 1 nếu trống)
    stacks.Add(newOrder, ArmorStackInstance(value, duration, newOrder))
```

**End Phase Tick:**

```
Tick():
    foreach stack: stack.duration -= 1
    remove stacks where duration == 0
```

---

## 9. Spell Layer

### `SpellCaster`

**Trách nhiệm:** Validate, build `SpellCastContext`, resolve hit/dodge cho SelectedEnemy, thực thi từng `SpellEffect` theo đúng thứ tự.

**Pipeline — 3 bước tách biệt:**

```
SpellCaster.TryCast(spellId, selectedEnemy?) → CastResult:

// Bước 1: Validation
    if !SpellSlotManager.IsImprinted(spell)       → NotImprinted
    if CooldownTracker.GetCooldown(spell) > 0     → OnCooldown
    if player.currentMp < cost                    → NotEnoughMp
    if !TurnManager.IsPlayerActionPhase           → NotYourTurn

// Bước 2: Hit/Dodge check (CHỈ cho SelectedEnemy)
    if spell có bất kỳ effect nào với SelectedEnemy resolver:
        hitResult = HitDodgeResolver.Resolve(
            attacker: player.GetEffectiveAGI(),
            defender: selectedEnemy.GetEffectiveAGI()
        )
        if Miss → VisualQueue.Enqueue(PlayMissAnim), return Success
        // return Success vì cast hợp lệ — chỉ là miss
        // MP và cooldown vẫn bị tiêu

// Bước 3: Execute effects
    context = new SpellCastContext(caster: player, selectedEnemy, spell)
    VisualQueue.Enqueue(PlaySpellAnim(player, spell))

    for each effect in spell.effects:
        if effect.condition != null && !effect.condition.Evaluate(target, context): skip

        targets = effect.targetResolver.Resolve(context, combatState)

        for each target in targets:
            // AOE/Random: hit/dodge per target
            if effect.targetResolver.type ∈ {AllEnemies, RandomEnemies, LowestHpEnemies}:
                if !HitDodgeResolver.Resolve(player.GetEffectiveAGI(), target.GetEffectiveAGI()):
                    VisualQueue.Enqueue(PlayMissAnim(target))
                    continue

            ExecuteEffect(effect, caster: player, target, context)

    player.currentMp -= cost
    CooldownTracker.Set(spell, spell.baseCooldown)
    return Success
```

**`ExecuteEffect` per subtype:**

```
ExecuteEffect(effect, caster, target, context):
    switch effect:

    DamageEffect:
        raw    = caster.GetEffectivePotency(potencyRef.element) × potencyRef.coefficient
        actual = DamageCalculator.Calculate(raw, target, potencyRef.element)
        // DamageCalculator áp resistance, crystalize flag, armor chain
        VisualQueue.Enqueue(PlayDamageAnimCommand(target))
        VisualQueue.Enqueue(ShowDamageNumberCommand(target, actual))

    HealEffect:
        amount = caster.GetEffectivePotency(potencyRef.element) × potencyRef.coefficient
        target.currentHp = min(target.currentHp + (int)amount, target.StoredMaxHp)
        VisualQueue.Enqueue(ShowHpGainCommand(target, amount))

    ArmorEffect:
        value = caster.GetEffectivePotency(potencyRef.element) × potencyRef.coefficient
        target.armorStack.ApplyArmor((int)value, effect.duration)
        VisualQueue.Enqueue(PlayArmorAnimCommand(target))

    StatusEffect:
        target.effectSystem.Apply(effect.effectType)
        CombatResolver.CheckCombinations(target)  // check combo sau mỗi effect apply
        VisualQueue.Enqueue(PlayEffectApplyAnimCommand(target, effect.effectType))
```

**Pipeline cast Spell (Enemy):**

```
SpellCaster.ExecuteEnemySpell(spellId, context: SpellCastContext):
    // Enemy không có hit/dodge check với single player (bỏ qua bước 2)
    for each effect in spell.effects:
        targets = effect.targetResolver.Resolve(context, combatState)
        for each target in targets:
            ExecuteEffect(effect, caster: enemy, target, context)
    CooldownTracker.Set(spell, spell.baseCooldown)
```

**Lưu ý Spark — ví dụ execution flow:**

```
SP_Spark cast lên Enemy A (player có AGI = 10, Enemy A AGI = 10):

Bước 2: SelectedEnemy hit check: hit_delta = 0 → dodge_chance = BASE_DODGE → roll → HIT

Bước 3:
  Effect 0: DamageEffect { SelectedEnemy }
    → targets = [Enemy A]
    → (không check hit/dodge lại vì SelectedEnemy đã check ở bước 2)
    → damage = player.GetEffectivePotency(Lightning) × 0.7
    → DamageCalculator → Enemy A nhận damage
    → Enqueue(PlayDamageAnim(A), ShowDamageNumber(A, dmg))

  Effect 1: DamageEffect { SecondaryRandom }
    → targets = AliveEnemies.Except(Enemy A).RandomPick(1) = [Enemy B]
    → (SecondaryRandom không check hit/dodge)
    → damage = player.GetEffectivePotency(Lightning) × 0.35
    → DamageCalculator → Enemy B nhận damage
    → Enqueue(ParallelCommand(PlayDamageAnim(B), ShowDamageNumber(B, dmg)))
```

---

### `SpellSlotManager`

**Trách nhiệm:** Quản lý số Spell Slot và danh sách spell đang imprint.

```
SpellSlotManager {
    slots    : SpellSlot[5]
    openSlots: int            // đọc từ WisdomSlotConfig dựa trên player.WIS
}

SpellSlot {
    isUnlocked : bool
    imprinted  : Spell?
}
```

`Imprint` / `Forget` chỉ thực hiện ngoài combat. Số slot mở dựa trên WIS threshold trong `WisdomSlotConfig` ScriptableObject.

---

### `CooldownTracker`

**Trách nhiệm:** Theo dõi cooldown của spell đang trong slot (Player) hoặc spell pool (Enemy).

- Khi spell được cast thành công → `Set(spellId, baseCooldown)`.
- End Phase → `Tick()` giảm tất cả cooldown > 0 xuống 1.
- `SpellCaster` / `SpellSelector` check `GetCooldown(spellId) == 0` trước khi cho phép cast.

---

## 10. Passive Layer

### `EquipmentSystem`

**Trách nhiệm:** Quản lý 5 equipment slot. Thu thập `StatModifier[]` từ tất cả equipment đang equipped và cung cấp cho `PlayerController.RecalculateBaseAttributes()`.

**5 slot:**

|Slot|Equipment|Focus Stat|
|---|---|---|
|Staff Slot|Staff|POT|
|Ring Slot|Ring|SPI|
|Book Slot|Book|WIS|
|Garb Slot|Garb|VIT|
|Boot Slot|Boots|AGI|

> Focus Stat chỉ là định hướng thiết kế cho GD — về mặt kỹ thuật, một Staff có thể có bất kỳ `StatModifier` nào. Focus Stat không được enforce bởi code.

**Khi equip/unequip:**

1. Cập nhật `equippedItems[]`
2. Gọi `PlayerController.RecalculateBaseAttributes()`

**Aggregate modifiers (được gọi bởi RecalculateBaseAttributes):**

```
EquipmentSystem.GetAllModifiers() → StatModifier[]:
    return equippedItems
        .Where(item => item != null)
        .SelectMany(item => item.modifiers)
```

**Rank và số modifier:**

|Rank|Số `StatModifier` tối đa|Ghi chú|
|---|---|---|
|Rank I|1|Flat modifier đơn giản|
|Rank II|2|Có thể mix Flat + Flat, hoặc 1 modifier giá trị cao hơn|
|Rank III|3|Có thể có Percent modifier|

> Percent modifier chỉ xuất hiện ở Rank III để giữ Rank I và II đơn giản và dễ đọc.

---

### `RuneSystem`

**Trách nhiệm:** Quản lý Rune Socket và lifecycle của passive Rune. Rune `StatModifier` type dùng chung cơ chế `StatModifier[]` với Equipment — không có logic riêng.

**Socket:** 0 mặc định, tối đa 4, mở tại Magic Shop. `Embed`/`Purge` chỉ tại Magic Shop.

**Socket unlock:**

```
RuneSystem {
    maxSockets     : int = 4
    currentSockets : int = 0     // số socket đã mở
    embeddedRunes  : Rune[4]     // null nếu chưa embed

    UnlockSocket() → bool:
        if currentSockets ≥ maxSockets: return false
        currentSockets += 1
        return true

    CanEmbed() → bool:
        return embeddedRunes.Count(r => r != null) < currentSockets
}
```

Giá mở Socket lũy tiến — đọc từ `ShopPriceConfig`. `ShopSystem` gọi `RuneSystem.UnlockSocket()` sau khi verify đủ Gold.

**`IRunePassive` interface — bắt buộc với mọi Rune:**

```
interface IRunePassive {
    OnEmbed(player: PlayerController): void
    OnPurge(player: PlayerController): void
}
```

**Phân loại passive theo cơ chế:**

|passiveType|Implement|OnEmbed|OnPurge|
|---|---|---|---|
|`StatModifier`|Không hook gì — modifier được đọc qua `GetAllModifiers()`|Gọi `RecalculateBaseAttributes()`|Gọi `RecalculateBaseAttributes()`|
|`ConditionalTrigger`|Subscribe `EffectSystem.OnEffectApplied`|Subscribe callback|Unsubscribe callback|
|`TurnHook`|Subscribe `TurnManager.OnStartPhase`|Subscribe callback|Unsubscribe callback|

**`RuneSystem.GetAllStatModifiers()` — aggregate modifier từ rune:**

```
RuneSystem.GetAllStatModifiers() → StatModifier[]:
    return embeddedRunes
        .Where(r => r != null && r.passiveType == PassiveType.StatModifier)
        .SelectMany(r => r.statModifiers)
```

`PlayerController.RecalculateBaseAttributes()` gọi cả `EquipmentSystem.GetAllModifiers()` lẫn `RuneSystem.GetAllStatModifiers()` để có tổng modifier đầy đủ.

**Verify no memory leak sau Purge:**

Với `ConditionalTrigger` và `TurnHook` rune, sau khi `OnPurge()` chạy:

```csharp
// Verify trong test
Assert.AreEqual(0,
    playerController.EffectSystem.OnEffectApplied.GetInvocationList()
        .Count(d => d.Target == purgedRune));
```

---

## 11. Meta Layer

### `ShopSystem`

**Trách nhiệm:** Sinh inventory ngẫu nhiên và xử lý giao dịch tại Magic Shop.

**Inventory mỗi lần vào Shop:**

```
ShopInventory {
    equipmentSection : Equipment[5]
    spellSection     : Spell[5]
    runeSection      : Rune[5]
}
```

Tỷ lệ Rank phụ thuộc Arc hiện tại — đọc từ `ArcConfig` ScriptableObject.

**Các dịch vụ:**

|Dịch vụ|Điều kiện|Tác động|
|---|---|---|
|Enlighten|Đủ WIS threshold|Mở thêm 1 Spell Slot|
|Embed|Có Rune + Socket trống|Gắn Rune: gọi `IRunePassive.OnEmbed()`|
|Purge|Có Rune embedded|Tháo Rune: gọi `IRunePassive.OnPurge()` trước|
|Rune Socket|< 4 Socket|Mở thêm 1 Socket, giá tăng lũy tiến|

---

### `RewardSystem`

**Trách nhiệm:** Offer 1 Equipment + 1 Spell + 1 Rune sau combat thắng, player chọn 1.

```
1. Xác định combatType và currentArc
2. Lấy bảng tỷ lệ từ RewardRateConfig ScriptableObject
3. Roll Rank cho từng loại item độc lập
4. Sample 1 item của đúng Rank từ pool
5. Hiển thị 3 lựa chọn → player chọn → áp vào inventory
```

---

### `EventSystem`

**Trách nhiệm:** Chọn ngẫu nhiên Event từ pool khi vào Event Node, xử lý tương tác của player, và thực thi các `EventAction` tương ứng.

**Cơ chế chọn Event:**

- Mỗi Arc dùng **shuffled queue** riêng — không lặp Event trong Arc nếu pool đủ.
- Queue shuffle lại đầu Arc mới.
- Khi vào Event Node: dequeue event tiếp theo → gọi `EventView` để hiển thị.

**Cấu trúc nội bộ:**

```
EventSystem {
    eventQueues     : Dictionary<int, Queue<EventDefinition>>  // key = arcNumber
    currentEvent    : EventDefinition?
    
    OnEventResolved : event Action  // phát khi event xử lý xong
}
```

**Pipeline resolve:**

```
EventSystem.EnterEvent(arcNumber: int):
    if eventQueues[arcNumber].isEmpty:
        eventQueues[arcNumber] = Shuffle(EventConfig.events)  // reshuffle
    
    currentEvent = eventQueues[arcNumber].Dequeue()
    // UI (EventView) hiển thị currentEvent

EventSystem.ResolveEvent(choice: EventChoice):
    event = currentEvent
    
    // TradeOff events: player có thể từ chối
    if event.category == TradeOff && choice == Decline:
        currentEvent = null
        OnEventResolved?.Invoke()
        return
    
    // Thực thi từng action theo thứ tự
    for each action in event.actions:
        switch action:

        GiveGoldAction:
            amount = Random(action.minAmount, action.maxAmount)
            GoldLedger.Earn(amount)

        GiveAttributeAction:
            player.availableAttributePoints += action.amount
            // UI (AttributeAllocationView) sẽ hiển thị khi HasAvailablePoints()

        GiveRandomItemAction:
            item = ItemPool.SampleOne(action.itemType, action.rank)
            player.AddToInventory(item)

        TakeHpPercentAction:
            damage = (int)(player.currentHp * action.percent)
            player.currentHp -= damage

        TakeGoldPercentAction:
            amount = (int)(GoldLedger.Balance * action.percent)
            GoldLedger.Spend(amount)

        ForceCombatAction:
            // Chuyển sang CombatScene với config đặc biệt
            GameManager.SetForcedCombat(
                enemyType:       action.enemyType,
                giveReward:      action.giveReward,
                guaranteedRank:  action.guaranteedRewardRank,
                guaranteedType:  action.guaranteedRewardType
            )
            // GameManager state machine sẽ transition sang InCombatState
            return  // không tiếp tục action list — combat xử lý riêng

        ApplyCombatModifierAction:
            GameManager.pendingCombatModifier = new CombatModifier(action.modifiers)
            // UI sẽ hiển thị modifier icon qua PendingModifierChangedSO

        OpenMiniShopAction:
            ShopSystem.OpenMiniShop(action.itemCount, action.discountPercent)
            return  // mini-shop xử lý riêng

    currentEvent = null
    OnEventResolved?.Invoke()
```

> **Lưu ý `ForceCombatAction` và `OpenMiniShopAction`:** Hai action này chuyển scene (combat hoặc shop) nên `return` sớm — các action còn lại (nếu có) không được thực thi. GD khi thiết kế event nên đặt các action này cuối danh sách.

> **`EventChoice`** là input từ UI: `{ accepted: bool }`. Không cần `attributeType` — attribute point cộng vào `availableAttributePoints`, player phân bổ qua `AttributeAllocationView` riêng.

**Event combat modifier flow:**

```
Force Trade event:
  EventSystem.ResolveEvent(Accept)
    ├── ApplyCombatModifierAction
    │     GameManager.pendingCombatModifier = {
    │       modifiers: [
    │         { stat: AllRes, modType: Percent, value: -0.15 },
    │         { stat: AllPot, modType: Percent, value:  0.15 }
    │       ]
    │     }
    └── PendingModifierChangedSO.RaiseEvent(modifier)  // qua bridge
          └── PendingModifierView hiển thị icon trên MapView

Khi vào CombatScene:
  TurnManager đọc GameManager.pendingCombatModifier
    ├── Áp modifier vào player (AllRes -15%, AllPot +15%)
    └── Sau combat kết thúc: GameManager.pendingCombatModifier = null
```


---

### `RestNode`

Khi player chọn Rest Node:

1. `player.currentHp = player.maxHp`
2. `player.availableAttributePoints += 1`
3. UI hiển thị `AttributeAllocationView` — player phân bổ điểm → `SpendAttributePoint(statType)`

---

### `GoldLedger`

```
GoldLedger {
    balance: int
    Earn(amount: int)
    Spend(amount: int) → bool
    CanAfford(amount: int) → bool
}
```

Tất cả giao dịch đều đi qua `GoldLedger` — không có gì trực tiếp modify `GameManager.gold`.

---

## 12. Data / Config Layer

### `ScriptableObjects`

|ScriptableObject|Nội dung|
|---|---|
|`CombatConfig`|`HIT_THRESHOLD`, `BASE_DODGE`, `MAX_DODGE`, `BASE_MP_RECOVERY`, `HP_CAP`, `HP_HALF`, `MP_COEFF`|
|`ArcConfig[3]`|Tổng Node, tỷ lệ từng loại Node, tỷ lệ Rank item trong Shop, Gold reward|
|`ShopPriceConfig`|Giá theo Rank, giá dịch vụ Enlighten/Embed/Purge, giá Socket lũy tiến|
|`RewardRateConfig`|Bảng tỷ lệ Rank reward theo combatType × Arc|
|`EventConfig`|`EventDefinition[]` — pool event với typed `EventAction` subtypes, serialize bằng `[SerializeReference]`|
|`WisdomSlotConfig`|WIS threshold để mở từng Spell Slot|

---

### `EnemyDefinitions`

```
EnemyDefinition {
    id                  : string
    displayName         : string
    enemyType           : Minion | Elite | Boss
    maxHp               : int
    baseFirePotency, baseWaterPotency, baseIcePotency, baseLightningPotency : float
    baseFireRes, baseWaterRes, baseIceRes, baseLightningRes                 : float
    spells              : SpellID[]
    decisionPolicyType  : Random | WeightedRandom | Priority | Scripted
    decisionPolicyConfig: object   // tham số tùy theo policy type
}
```

`EnemyController` nhận `EnemyDefinition` khi khởi tạo, tạo instance runtime từ đó.

> **Dependency với GDD — Enemies:** Nội dung cụ thể của từng enemy (HP, spells, policy config) phụ thuộc vào **GDD — Enemies** — tài liệu này cần được viết trước khi implement `EnemyDefinitions`. Kiến trúc AI đã sẵn sàng nhận data từ GDD đó.

---

### `SpellDefinitions` _(Thiết kế lại v6)_

---

#### Vấn đề với thiết kế cũ

`EffectApplication { effectType, valueFormula: string, condition }` có 3 vấn đề:

1. **`valueFormula` là string không implement được** — ai parse? Runtime interpreter sẽ fragile và không type-safe.
2. **Gộp 4 cơ chế khác nhau** — damage, heal, armor, status effect đều là "EffectApplication" nhưng cách tính và cách apply hoàn toàn khác nhau.
3. **`targetType` là global cho cả spell** — không mô tả được Spark (primary target damage + secondary random damage ở 50%).

---

#### Typed SpellEffect Subtypes

Thay `EffectApplication` bằng 4 subtype rõ ràng, serialize bằng `[SerializeReference]`:

```
// Base class — serialize bằng [SerializeReference] trong Unity
abstract class SpellEffect {
    targetResolver : TargetResolver
    condition      : SpellCondition?   // nullable — effect chỉ fire nếu condition thỏa
}

// 1. Gây sát thương
class DamageEffect : SpellEffect {
    potencyRef : PotencyRef   // element + hệ số nhân
}

// 2. Hồi HP
class HealEffect : SpellEffect {
    potencyRef : PotencyRef   // thường dùng water_potency
}

// 3. Apply Armor
class ArmorEffect : SpellEffect {
    potencyRef : PotencyRef   // value = caster.GetEffectivePotency(element) × coefficient
    duration   : int
}

// 4. Apply buff/debuff
class StatusEffect : SpellEffect {
    effectType : EffectType   // Burn, Drenched, Chilled, Frozen, Enrage, v.v.
    // Không cần value — buff/debuff có giá trị cố định theo GDD
}
```

---

#### `PotencyRef` — thay `valueFormula` string

```
struct PotencyRef {
    element     : Element   // Fire | Water | Ice | Lightning
    coefficient : float     // VD: 0.8 → damage = caster.GetEffectivePotency(element) × 0.8
}
```

Tính giá trị thực tế tại thời điểm cast:

```
actualValue = caster.GetEffectivePotency(potencyRef.element) × potencyRef.coefficient
```

Type-safe, không cần parser, dễ edit trong Inspector.

---

#### `TargetResolver` — target resolution per effect

Mỗi `SpellEffect` có `TargetResolver` riêng thay vì dùng chung `targetType` của spell. Điều này cho phép một spell có nhiều effect với target khác nhau (như Spark).

```
struct TargetResolver {
    type                : TargetResolveType
    targetCount         : int           // dùng cho RandomEnemies(targetCount) và LowestHpEnemies(targetCount)
    tieBreaker          : TieBreaker    // dùng cho LowestHpEnemies khi có tie
    tieBreakerElement   : Element       // dùng khi tieBreaker = LowestResistance
}

enum TargetResolveType {
    Caster,              // bản thân caster — heal, armor, self buff
    SelectedEnemy,       // player chọn tại thời điểm cast — UI hiện target selection
    AllEnemies,          // tất cả enemy đang sống
    RandomEnemies,       // targetCount enemy ngẫu nhiên trong số đang sống (không trùng)
    LowestHpEnemies,     // targetCount enemy có HP thấp nhất
    SecondaryRandom      // 1 enemy ngẫu nhiên NGOẠI TRỪ primary target — dùng cho Spark
}

enum TieBreaker {
    None,                // không cần — HP bằng nhau thì random
    LowestResistance     // HP bằng nhau → chọn enemy có resistance thấp nhất
                         // (theo tieBreakerElement)
}
```

**Resolve tại runtime:**

```
TargetResolver.Resolve(castContext: SpellCastContext, combatState: CombatState) → IEntity[]:
    switch type:
        Caster            → [ castContext.Caster ]
        SelectedEnemy     → [ castContext.SelectedEnemy ]
        AllEnemies        → combatState.AliveEnemies
        RandomEnemies     → combatState.AliveEnemies.Shuffle().Take(targetCount)
        LowestHpEnemies   → Sort by HP asc (tieBreaker nếu cần), Take(targetCount)
        SecondaryRandom   → combatState.AliveEnemies
                              .Except(castContext.SelectedEnemy)
                              .Shuffle().Take(1)
```

---

#### `SpellCastContext` — execution context

```
class SpellCastContext {
    Caster         : IEntity         // người cast (Player hoặc Enemy)
    SelectedEnemy  : IEntity?        // null nếu spell không có SelectedEnemy target
    Spell          : SpellDefinition
}
```

`SelectedEnemy` được set bởi UI khi player tap target, trước khi `SpellCaster.ExecuteSpell()` được gọi. Enemy AI set dựa trên `DecisionPolicy.SelectTarget()`.

---

#### `CombatState` — snapshot trạng thái combat

```
class CombatState {
    Player       : IEntity          // reference đến PlayerController
    AliveEnemies : IEntity[]        // danh sách enemy còn sống
}
```

Tạo mới mỗi lượt bởi `TurnManager`. `TargetResolver.Resolve()` nhận `CombatState` để resolve target mà không cần biết chi tiết quản lý entity.

---

#### Hit/Dodge rule theo target type

|Resolver type|Hit/dodge rule|
|---|---|
|`SelectedEnemy`|Check 1 lần cho selected enemy. **Miss → abort toàn bộ spell** (tất cả effect không fire).|
|`AllEnemies`|Check per enemy — mỗi enemy tự dodge độc lập.|
|`RandomEnemies`|Check per enemy.|
|`LowestHpEnemies`|Check per enemy.|
|`SecondaryRandom`|**Không check hit/dodge** — đây là splash damage, không phải đòn tấn công riêng.|
|`Caster`|Không check (heal/armor/buff không có dodge).|

---

#### `SpellDefinition` struct cuối cùng

```
SpellDefinition {
    id                  : string
    displayName         : string
    rank                : Rank (I | II | III)
    element             : Element            // nguyên tố chính của spell
    baseCost            : int
    baseCooldown        : int
    minWisdomToImprint  : int                // WIS tối thiểu để Imprint vào Spell Slot
    effects             : SpellEffect[]      // [SerializeReference] — typed subtypes
}
```

> `element` ở spell level xác định buff/debuff nguyên tố nào được apply (theo GDD: dùng phép lửa lên bản thân → Enrage; dùng lên kẻ địch → Burn). `element` trong `PotencyRef` xác định potency nào dùng để tính giá trị — thường khớp nhau nhưng có thể khác (edge case spell lai nguyên tố).

---

#### Ví dụ concrete — Rank I Spells

```
// SP_Fireball — Rank I Fire, CD 1
effects: [
    DamageEffect {
        targetResolver: { type: SelectedEnemy },
        potencyRef:     { element: Fire, coefficient: 0.8 }     // [TBD]
    },
    StatusEffect {
        targetResolver: { type: SelectedEnemy },
        effectType:     Burn
    }
]
// Lưu ý: StatusEffect tự động fire sau DamageEffect trên cùng target
```

```
// SP_Spark — Rank I Lightning, CD 1
// "Damage to selected enemy + 50% to 1 random other"
effects: [
    DamageEffect {
        targetResolver: { type: SelectedEnemy },
        potencyRef:     { element: Lightning, coefficient: 0.7 }   // [TBD]
    },
    DamageEffect {
        targetResolver: { type: SecondaryRandom },
        potencyRef:     { element: Lightning, coefficient: 0.35 }   // 50% của main
        // SecondaryRandom → không check hit/dodge
    }
]
```

```
// SP_CandleGhost — Rank I Fire, CD 2
// "3 lowest HP enemies; tie → pick lowest fire_res"
effects: [
    DamageEffect {
        targetResolver: {
            type:               LowestHpEnemies,
            n:                  3,
            tieBreaker:         LowestResistance,
            tieBreakerElement:  Fire
        },
        potencyRef: { element: Fire, coefficient: 0.6 }   // [TBD]
    }
]
```

```
// SP_ColdBreathe — Rank I Ice, CD 2 — "all enemies"
effects: [
    DamageEffect {
        targetResolver: { type: AllEnemies },
        potencyRef:     { element: Ice, coefficient: 0.5 }   // [TBD]
    }
]
```

```
// SP_Heal — Rank I Water, CD 2
effects: [
    HealEffect {
        targetResolver: { type: Caster },
        potencyRef:     { element: Water, coefficient: 1.0 }   // [TBD]
    }
]
```

```
// SP_IceShield — Rank I Ice, CD 2
// "Apply Armor (X% ice_pot, 3 turns)"
effects: [
    ArmorEffect {
        targetResolver: { type: Caster },
        potencyRef:     { element: Ice, coefficient: 0.8 },   // [TBD]
        duration:       3
    }
]
```

```
// SP_Bubble — Rank I Water, CD 3 — "Stun target 1 turn"
// GDD không có "Stun" effect — Bubble apply Frozen trực tiếp,
// không yêu cầu combo Drenched + Chilled
effects: [
    StatusEffect {
        targetResolver: { type: SelectedEnemy },
        effectType:     Frozen    // apply trực tiếp, bỏ qua combo requirement
    }
]
```

> **Lưu ý Bubble/Frozen:** `EffectSystem.Apply(Frozen)` khi Bubble cast sẽ đi qua element interaction check bình thường (Water vs các effect đang có), nhưng **không** yêu cầu Drenched + Chilled prerequisite. Frozen là effect type, không phải combo — combo Drenched + Chilled là _một cách khác_ để apply Frozen. `CombatResolver.CheckCombinations()` vẫn chạy như bình thường sau đó.

```
// SP_Pulse — Rank I Lightning, CD 2 — "3 random enemies"
effects: [
    DamageEffect {
        targetResolver: { type: RandomEnemies, n: 3 },
        potencyRef:     { element: Lightning, coefficient: 0.5 }   // [TBD]
    }
]
```

---

#### `SpellCondition` — điều kiện kích hoạt effect

Một số Rank II/III spell có effect chỉ fire trong điều kiện nhất định:

```
abstract class SpellCondition {
    abstract bool Evaluate(IEntity target, SpellCastContext context)
}

class TargetHasEffect : SpellCondition {
    EffectType effectType
    // Evaluate: target.effectSystem.Has(effectType)
}

class TargetHpBelow : SpellCondition {
    float threshold   // 0.0 – 1.0
    // Evaluate: target.CurrentHp / target.StoredMaxHp < threshold
}

class CasterHasEffect : SpellCondition {
    EffectType effectType
}
```

Dùng trong Rank II/III spell. Rank I spell hiện tại đều `condition = null`.

---

### `EquipmentDefinition` _(Cập nhật v4)_

```
EquipmentDefinition {
    id          : string
    displayName : string
    slot        : EquipmentSlot    // Staff | Ring | Book | Garb | Boots
    rank        : Rank (I | II | III)
    modifiers   : StatModifier[]   // tối đa 1/2/3 theo Rank; Percent chỉ xuất hiện Rank III
}
```

**Ví dụ:**

```
// Rank I
Staff_FlameWand:
  modifiers: [{ stat: POT, modType: Flat, value: 5 }]

// Rank II — mix Main Attr + flat MaxHp
Garb_IronRobe:
  modifiers: [
    { stat: VIT,   modType: Flat, value: 8  },
    { stat: MaxHp, modType: Flat, value: 40 }
  ]

// Rank III — có Percent
Staff_DragonFang:
  modifiers: [
    { stat: POT,     modType: Flat,    value: 12   },
    { stat: FirePot, modType: Flat,    value: 20   },
    { stat: FirePot, modType: Percent, value: 0.15 }
  ]
```

---

### `RuneDefinition` _(Cập nhật v4)_

```
RuneDefinition {
    id            : string
    displayName   : string
    rank          : Rank (I | II | III)
    passiveType   : StatModifier | ConditionalTrigger | TurnHook
    statModifiers : StatModifier[]   // dùng khi passiveType == StatModifier
    passiveConfig : object           // dùng khi passiveType == ConditionalTrigger | TurnHook
}
```

**Ví dụ:**

```
// Rank I — StatModifier thuần
Rune_FireEmbrace:
  passiveType: StatModifier
  statModifiers: [{ stat: FirePot, modType: Flat, value: 10 }]

// Rank II — StatModifier với AllRes Percent
Rune_MageArmor:
  passiveType: StatModifier
  statModifiers: [{ stat: AllRes, modType: Percent, value: 0.10 }]

// Rank III — ConditionalTrigger (không dùng statModifiers)
Rune_Combustion:
  passiveType: ConditionalTrigger
  passiveConfig: { trigger: OnEffectApplied(Burn), action: HealSelf(5%MaxHp) }
```

---

### `EventDefinitions` _(Thêm mới v7)_

#### Vấn đề trước v7

GDD — Progression Design liệt kê 10 event với mô tả text, nhưng không có data schema. Mỗi event có hiệu ứng hoàn toàn khác nhau (cho Gold, bắt combat, thay đổi attribute, mở mini-shop, áp combat modifier) — không thể gom vào 1 struct đơn giản.

---

#### `EventCategory` enum

```
enum EventCategory {
    Positive,    // (+) — chỉ lợi ích, auto-resolve
    TradeOff,    // (=) — có cả lợi và hại, player quyết định accept/decline
    Negative     // (−) — chỉ bất lợi, auto-resolve
}
```

> TradeOff events hiển thị nút Accept / Decline trong `EventView`. Positive và Negative events chỉ có nút Confirm.

---

#### `EventAction` subtypes

Theo mô hình typed subtypes giống `SpellEffect` — serialize bằng `[SerializeReference]`:

```
// Base class
abstract class EventAction { }

// 1. Cho Gold ngẫu nhiên trong range
class GiveGoldAction : EventAction {
    minAmount : int
    maxAmount : int
}

// 2. Cho điểm Main Attribute (cộng vào availableAttributePoints)
class GiveAttributeAction : EventAction {
    amount : int   // số điểm cộng vào availableAttributePoints
}

// 3. Cho item ngẫu nhiên
class GiveRandomItemAction : EventAction {
    itemType : ItemCategory   // Equipment | Spell | Rune
    rank     : Rank?          // null → random rank theo Arc config
}

// 4. Trừ % HP hiện tại
class TakeHpPercentAction : EventAction {
    percent : float   // 0.0–1.0, VD: 0.20 = mất 20% HP hiện tại
}

// 5. Trừ % Gold hiện có
class TakeGoldPercentAction : EventAction {
    percent : float   // VD: 0.25 = mất 25% Gold
}

// 6. Bắt buộc combat ngay
class ForceCombatAction : EventAction {
    enemyType             : EnemyType     // Minion | Elite
    giveReward            : bool          // false cho Ambush
    guaranteedRewardRank  : Rank?         // null → normal reward; Rank.III → guarantee
    guaranteedRewardType  : ItemCategory? // null → normal; Spell → guaranteed Spell
}

// 7. Áp combat modifier cho trận tiếp theo
class ApplyCombatModifierAction : EventAction {
    modifiers : StatModifier[]   // reuse StatModifier — áp vào pendingCombatModifier
}

// 8. Mở mini-shop đặc biệt
class OpenMiniShopAction : EventAction {
    itemCount       : int     // số item hiển thị
    discountPercent : float   // 0.25 = giảm 25% giá
}
```

---

#### `ItemCategory` enum

```
enum ItemCategory {
    Equipment,
    Spell,
    Rune
}
```

Dùng cho `GiveRandomItemAction` và `ForceCombatAction`. Không trùng với `EquipmentSlot` (Staff/Ring/Book/Garb/Boots) — đây là phân loại item ở mức cao nhất.

---

#### `EventDefinition` struct cuối cùng

```
EventDefinition {
    id              : string
    displayName     : string
    description     : string           // flavor text hiển thị trong EventView
    category        : EventCategory    // Positive | TradeOff | Negative
    actions         : EventAction[]    // [SerializeReference] — typed subtypes
}
```

> `category == TradeOff` → `EventView` hiển thị Accept / Decline. Player decline → không action nào fire.
> `category == Positive / Negative` → `EventView` chỉ hiển thị Confirm.

---

#### `EventConfigSO`

```
EventConfigSO : ScriptableObject {
    events : EventDefinition[]   // pool toàn bộ event trong game
}
```

`EventSystem` đọc `EventConfigSO.events` để tạo shuffled queue cho mỗi Arc. Một file `EventConfig.asset` duy nhất chứa tất cả event.

---

#### Ví dụ concrete — 10 Events từ GDD

```
// (+) Windfall — Nhận Gold ngẫu nhiên
Windfall:
  category: Positive
  actions: [
    GiveGoldAction { minAmount: [TBD], maxAmount: [TBD] }
  ]
```

```
// (+) Ancient Shrine — +1 Main Attribute tùy chọn
AncientShrine:
  category: Positive
  actions: [
    GiveAttributeAction { amount: 1 }
  ]
```

```
// (+) Wandering Merchant — Mini-shop 3 items, giảm 25%
WanderingMerchant:
  category: Positive
  actions: [
    OpenMiniShopAction { itemCount: 3, discountPercent: 0.25 }
  ]
```

```
// (+) Hidden Cache — Nhận miễn phí 1 Rune ngẫu nhiên
HiddenCache:
  category: Positive
  actions: [
    GiveRandomItemAction { itemType: Rune, rank: null }
  ]
```

```
// (=) Cursed Altar — Mất [TBD]% HP hiện tại, nhận +3 Attribute tùy chọn
CursedAltar:
  category: TradeOff
  actions: [
    TakeHpPercentAction    { percent: [TBD] },
    GiveAttributeAction    { amount: 3 }
  ]
```

```
// (=) Lost Devil — Combat Elite, thắng = guaranteed Rank III Spell
LostDevil:
  category: TradeOff
  actions: [
    ForceCombatAction {
      enemyType:             Elite,
      giveReward:            true,
      guaranteedRewardRank:  Rank.III,
      guaranteedRewardType:  Spell
    }
  ]
```

```
// (=) Force Trade — Next combat: all_res -15%, all_pot +15%
ForceTrade:
  category: TradeOff
  actions: [
    ApplyCombatModifierAction {
      modifiers: [
        { stat: AllRes, modType: Percent, value: -0.15 },
        { stat: AllPot, modType: Percent, value:  0.15 }
      ]
    }
  ]
```

```
// (−) Ambush — Combat Minion, no rewards
Ambush:
  category: Negative
  actions: [
    ForceCombatAction {
      enemyType:             Minion,
      giveReward:            false,
      guaranteedRewardRank:  null,
      guaranteedRewardType:  null
    }
  ]
```

```
// (−) Fading Curse — Next combat: all_res -10%
FadingCurse:
  category: Negative
  actions: [
    ApplyCombatModifierAction {
      modifiers: [
        { stat: AllRes, modType: Percent, value: -0.10 }
      ]
    }
  ]
```

```
// (−) Thief Gang — Mất 25% Gold
ThiefGang:
  category: Negative
  actions: [
    TakeGoldPercentAction { percent: 0.25 }
  ]
```

---

### `CombatModifier` _(Thêm mới v7)_

Struct mô tả modifier tạm thời áp dụng cho trận combat tiếp theo. Được tạo bởi Event (Force Trade, Fading Curse) và lưu trong `GameManager.pendingCombatModifier`.

```
CombatModifier {
    modifiers : StatModifier[]   // reuse StatModifier — áp dụng khi combat start
}
```

**Cách áp dụng trong combat:**

```
TurnManager.StartCombatAsync():
    if GameManager.pendingCombatModifier != null:
        for each mod in pendingCombatModifier.modifiers:
            // Áp vào player như Layer 3 tạm thời
            // mod.modType == Percent → nhân vào effective value
            player.ApplyTemporaryModifier(mod)
        
    // ... combat loop ...
    
    // Sau combat kết thúc:
    player.RemoveAllTemporaryModifiers()
    GameManager.pendingCombatModifier = null
```

> **Tại sao dùng `StatModifier[]`?** Force Trade (`AllRes -15%, AllPot +15%`) và Fading Curse (`AllRes -10%`) đều là percentage adjustments — chính xác là `StatModifier` với `ModType.Percent`. Reuse struct có sẵn, không cần kiểu mới.

> **Lưu trữ:** `CombatModifier?` nullable trong `GameManager` và `SaveData`. `null` = không có modifier pending.

---

### `SaveSystem` _(Thêm mới v2)_

**Trách nhiệm:** Serialize và deserialize trạng thái run tại các checkpoint.

**Checkpoint:** Xảy ra **sau khi player ra khỏi Node** (không save mid-combat). Các checkpoint: sau combat thắng (post-reward), sau Shop, sau Rest, sau Event.

**Không save mid-combat** vì: serialize toàn bộ EffectSystem, ArmorStack, spell cooldown, VisualQueue là phức tạp và dễ bị exploit (save-scum từng đòn).

**Dữ liệu được save:**

```
SaveData {
    currentArc          : int
    mapSeed             : int         // để regenerate map giống hệt
    visitedNodeIds      : string[]
    playerSnapshot      : PlayerSnapshot
    gold                : int
    pendingCombatModifier: CombatModifier?
}
```

**Trigger save:** `GameManager` subscribe vào `NodeRouter.OnNodeExit` → gọi `SaveSystem.Save()` sau mỗi Node.

---

## 13. Luồng dữ liệu chính

### Luồng: Player cast Spell lên Enemy (Spark làm ví dụ)

```
Player chọn Spark + tap Enemy A → SpellCaster.TryCast(Spark, selectedEnemy: A)
  │
  ├── [Validate] slot, cooldown, mp, phase
  │
  ├── [Hit/Dodge] SelectedEnemy check: HitDodgeResolver(player.AGI, A.AGI)
  │       └── Miss → Enqueue(PlayMissAnim(A)), return Success (MP và CD tiêu)
  │
  ├── Enqueue(PlaySpellAnimCommand(player, Spark))
  │
  ├── [Effect 0] DamageEffect { SelectedEnemy, Lightning × 0.7 }
  │       └── targets = [Enemy A]
  │       └── raw = player.GetEffectivePotency(Lightning) × 0.7
  │       └── DamageCalculator.Calculate → Enemy A.ArmorStack → Enemy A.currentHp
  │       └── Enqueue(PlayDamageAnimCommand(A), ShowDamageNumberCommand(A, dmg))
  │
  ├── [Effect 1] DamageEffect { SecondaryRandom, Lightning × 0.35 }
  │       └── targets = AliveEnemies.Except(A).RandomPick(1) = [Enemy B]
  │       └── Không check hit/dodge (SecondaryRandom = splash)
  │       └── DamageCalculator.Calculate → Enemy B
  │       └── Enqueue(ParallelCommand(PlayDamageAnim(B), ShowDamageNumber(B, dmg)))
  │
  ├── player.currentMp -= cost
  └── CooldownTracker.Set(Spark, 1)
```

### Luồng: Enemy Action Phase

```
TurnManager → PhaseHandler.RunActionPhase(enemy)
  ├── Nếu Frozen flag: skip, return
  ├── context = new CombatContext(player, enemy, roundNumber)
  ├── available = SpellSelector.GetAvailableSpells(enemy, context)
  ├── spellId = enemy.DecisionPolicy.SelectSpell(available, context)
  ├── Thực thi spell → DamageCalculator / EffectSystem / CombatResolver
  └── CooldownTracker.Set(spellId, baseCooldown)
```

### Luồng: Equip item mới

```
Player equip Staff_DragonFang → EquipmentSystem
  ├── equippedItems[StaffSlot] = Staff_DragonFang
  └── PlayerController.RecalculateBaseAttributes()
        ├── Bước 1: Thu thập allModifiers
        │     EquipmentSystem.GetAllModifiers() → [POT+12, FirePot+20, FirePot+15%]
        │     RuneSystem.GetAllStatModifiers()  → [...rune modifiers...]
        │
        ├── Bước 2: Tính Main Attributes
        │     POT = BASE_POT + sum(flat POT modifiers) = 10 + 12 = 22
        │
        ├── Bước 3: Tính storedFirePotency
        │     L0 = pot_scale × POT = 1.0 × 22 = 22.0
        │     L1 = 20.0  (flat FirePotency từ equipment)
        │     L2 = 0.15  (PercentAdd từ equipment)
        │     storedFirePotency = (22.0 + 20.0) × (1 + 0.15) = 48.3
        │
        └── Bước 5: Tính maxHp (VIT không đổi → maxHp không đổi)

Khi cast Fire spell lúc combat (Enrage đang active):
  effective_fire_potency
    = storedFirePotency × (1 + 0.15 Enrage)
    = 48.3 × 1.15
    = 55.545
```

```
TurnManager phát hiện all enemies currentHp <= 0
  ├── await VisualQueue (đợi animation chết xong)
  ├── GameManager.pendingCombatModifier = null
  ├── GoldLedger.Earn(goldReward)
  ├── RewardSystem.GenerateOffer → player chọn 1
  ├── SaveSystem.Save()
  └── NodeRouter.OnNodeComplete() → MapView
```

### Luồng: Rune bị Purge tại Shop

```
ShopSystem.PurgeRune(runeIndex)
  ├── rune = player.embeddedRunes[runeIndex]
  ├── rune.passive.OnPurge(player)        // gỡ toàn bộ hook, tránh memory leak
  ├── player.embeddedRunes[runeIndex] = null
  ├── GoldLedger.Spend(purgePrice)
  └── PlayerController.RecalculateBaseAttributes()
```

---

## 14. Ghi chú triển khai

|Mục|Ghi chú|
|---|---|
|**UniTask / DOTween**||
|UniTask thay toàn bộ Coroutine|`IEnumerator Execute()` → `UniTask ExecuteAsync(CancellationToken)`|
|DOTween bridge với UniTask|`tween.ToUniTask(ct)` — cần UniTask DOTween integration package hoặc extension tự viết|
|`ParallelCommand` cho animation đồng thời|`UniTask.WhenAll()` thay vì enqueue tuần tự|
|`CancellationToken` xuyên suốt async chain|Truyền từ `StartCombatAsync()` xuống mọi command — scene unload cancel sạch|
|**StateMachine / EventChannel**||
|StateMachine chỉ cho `GameManager` game-state|TurnManager phase loop là async sequential — `await` UniTask thẳng, không FSM|
|`IAsyncState` cho state cần async Enter|Extend `IState`, `StateMachineManager` gọi `EnterAsync()` — dùng cho scene loading state|
|`GameManagerDriver` MonoBehaviour|Gọi `_fsm.Tick()` trong `Update()`, khởi động `StartCombatAsync()` khi vào `InCombatState`|
|EventChannelSO chỉ cho cross-boundary|Logic layer giữ C# `event Action<T>`; `EventBridge` MonoBehaviour làm adapter mỏng|
|`EventBridge` không chứa business logic|Chỉ relay C# event → SO.RaiseEvent(), không quyết định gì|
|Reentrancy-safe trong EventChannelSO|`_isRunning` flag ngăn raise lồng nhau — SO event không trigger SO event cùng channel|
|**Spell Execution**||
|`SpellEffect` là typed subtypes|`[SerializeReference]` cho phép serialize polymorphic trong Inspector|
|`PotencyRef` thay `valueFormula` string|Type-safe, không cần runtime parser, `{ element, coefficient }`|
|`TargetResolver` per effect, không per spell|Cho phép Spark (SelectedEnemy + SecondaryRandom) và các spell lai targeting|
|`SecondaryRandom` không check hit/dodge|Splash damage — không phải đòn tấn công độc lập|
|SelectedEnemy miss → abort toàn bộ spell|MP và cooldown vẫn bị tiêu (cast hợp lệ, chỉ miss)|
|AOE hit/dodge check per enemy|`AllEnemies`/`RandomEnemies`/`LowestHpEnemies` — mỗi enemy tự dodge độc lập|
|`CombatResolver.CheckCombinations()` sau mỗi StatusEffect|Combo có thể trigger ngay sau từng effect apply, không chờ hết spell|
|`SpellCastContext` giữ reference selectedEnemy|`SecondaryRandom.Resolve()` cần biết primary target để exclude|
|Bubble apply Frozen trực tiếp|Không yêu cầu combo — Frozen là effect type, combo là cách khác để đạt nó|
|Tất cả coefficient `[TBD]`|Giá trị cụ thể cần balancing, lưu trong `SpellDefinition` SO|
|4 layer tách biệt|L0 formula → L1 flat → L2 percent → L3 combat effect, không trộn lẫn|
|`storedXValue` thay vì `baseXValue`|Đã tính qua L0+L1+L2; L3 tính realtime trong `GetEffective*()`|
|`StatModifier` struct dùng chung|Equipment và Rune `StatModifier` type dùng cùng struct, không duplicate|
|Percent chỉ ở Rank III Equipment|Rank I/II chỉ Flat — đơn giản, dễ đọc|
|`RecalculateBaseAttributes()` thứ tự cố định|Bước 1 collect → 2 main attr → 3 potency → 4 resist → 5 HP/MP → 6 recovery|
|**Combat**||
|`EffectSystem` dùng Dictionary keyed by EffectType|Combo check O(1)|
|`ArmorStack` dùng SortedList descending|Damage luôn lấy MaxKey, overflow tự lan xuống|
|`CombatResolver` là mediator duy nhất|`EffectSystem` và `DamageCalculator` không reference nhau|
|`Detonates` bypass Resistance và Armor|Flag `ignoreResistance`, `ignoreArmor` trong `ApplyRawDamage`|
|Crystalize flag bật bước 3, Burn bước 5|Crystalize apply trong lượt hiện tại không bảo vệ ngay — chỉ từ lượt sau|
|Combo tiêu hủy prerequisite|Enrage + Energized → remove cả hai, apply Overdrive. Tương tự các combo khác|
|**Rune / Equipment**||
|`IRunePassive.OnPurge()` bắt buộc|Phải unsubscribe mọi event đã đăng ký trong `OnEmbed()`|
|`AllPot` và `AllRes` shorthand|Apply cho cả 4 nguyên tố trong 1 modifier|
|**Meta / Save / Event**||
|`SaveSystem` chỉ save tại Node boundary|Không save mid-combat — tránh phức tạp và exploit|
|`GoldLedger` là single source of truth|Không có gì khác modify gold trực tiếp|
|`availableAttributePoints` tách earn/spend|RestNode và GiveAttributeAction chỉ `+= N`; player phân bổ qua `SpendAttributePoint()` + `AttributeAllocationView`|
|`GiveAttributeAction` chỉ có `amount`|Bỏ `playerChooses` — player luôn chọn attribute, flow thống nhất|
|`EventAction` là typed subtypes|`[SerializeReference]` — giống pattern SpellEffect, 8 subtypes|
|`EventCategory` phân loại event|`TradeOff` → player accept/decline; `Positive`/`Negative` → auto-resolve|
|`ForceCombatAction` và `OpenMiniShopAction` return sớm|Chuyển scene → các action sau trong list không fire|
|`CombatModifier` reuse `StatModifier[]`|Không cần kiểu mới — Force Trade / Fading Curse đều là percent modifiers|
|`CombatModifier` chỉ tồn tại 1 trận|Áp vào `StartCombatAsync()`, xóa sau khi combat kết thúc|
|`EventConfigSO` chứa toàn bộ pool|1 file `EventConfig.asset` duy nhất — `EventSystem` tạo shuffled queue per Arc|
|**AI**||
|`CombatContext` là snapshot bất biến|Tạo mới mỗi lần enemy action|
|`DecisionPolicy` là data-driven|Hành vi enemy do GD cấu hình trong `EnemyDefinition`|
|`GDD — Enemies` là dependency còn thiếu|`PriorityPolicy` và `ScriptedPolicy` cần nội dung từ GDD đó|
|**UI**||
|Logic không reference UI|UI subscribe event một chiều — không bao giờ ngược lại|
|Input không validate ở UI|UI gọi API → nhận `CastResult` / `PurchaseResult` → phản hồi|
|Unsubscribe trong `OnDestroy()`|Bắt buộc với mọi View|
|`pendingCombatModifier` cần icon trên MapView|Data đã có trong `GameManager`, UI đọc qua `PendingModifierChangedSO`|
|Enemy Intent là optional feature|Cần xác nhận với GD trước khi implement|
|Chi tiết widget từng screen|Thuộc UI Design doc riêng, ngoài scope Architecture doc|