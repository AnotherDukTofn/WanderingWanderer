___

**Game:** Wandering Wanderer 
**Author:** DukTofn 
**Last Updated:** 09/04/2026 
___

## Mục lục
- [Hướng dẫn đọc](#hướng-dẫn-đọc)
- [Phase 0 — Project Setup & Foundation](#phase-0--project-setup--foundation)
  - [TASK-001 — Khởi tạo project và folder structure](#task-001--khởi-tạo-project-và-folder-structure)
  - [TASK-002 — Cài đặt packages: UniTask và DOTween](#task-002--cài-đặt-packages-unitask-và-dotween)
  - [TASK-003 — Tạo ScriptableObject: CombatConfig](#task-003--tạo-scriptableobject-combatconfig)
  - [TASK-004 — Tạo ScriptableObject: ArcConfig...](#task-004--tạo-scriptableobject-arcconfig-shoppriceconfig-rewardrateconfig-eventconfig-wisdomslotconfig)
  - [TASK-005 — StatModifier struct và StatType enum](#task-005--statmodifier-struct-và-stattype-enum)
  - [TASK-006 — Tạo SpellDefinition và data Rank I](#task-006--tạo-spelldefinition-và-data-rank-i)
  - [TASK-007 — Tạo EnemyDefinition schema](#task-007--tạo-enemydefinition-schema)
  - [TASK-008 — Tạo EquipmentDefinition và RuneDefinition schema](#task-008--tạo-equipmentdefinition-và-runedefinition-schema)
  - [TASK-009 — Foundation Layer: EventChannelSO và StateMachineManager](#task-009--foundation-layer-eventchannelso-và-statemachinemanager)
  - [TASK-010 — EventChannelSO assets cho cross-boundary events](#task-010--eventchannelso-assets-cho-cross-boundary-events)
- [Phase 1 — Core Math (Pure Logic)](#phase-1--core-math-pure-logic)
  - [TASK-020 — ArmorStack: implement và unit test](#task-020--armorstack-implement-và-unit-test)
  - [TASK-021 — DamageCalculator: implement và unit test](#task-021--damagecalculator-implement-và-unit-test)
  - [TASK-022 — HitDodgeResolver: implement và unit test](#task-022--hitdodgeresolver-implement-và-unit-test)
  - [TASK-023 — PlayerController: HP/MP scaling và Stat Layering](#task-023--playercontroller-hpmp-scaling-và-stat-layering)
  - [TASK-024 — PlayerController: Stat Layering với equipment/rune modifier](#task-024--playercontroller-stat-layering-với-equipmentrune-modifier)
  - [TASK-025 — PlayerController: Dynamic getters (Layer 3)](#task-025--playercontroller-dynamic-getters-layer-3)
- [Phase 2 — Effect System](#phase-2--effect-system)
  - [TASK-030 — EffectSystem: apply và lookup](#task-030--effectsystem-apply-và-lookup)
  - [TASK-031 — EffectSystem: element interaction](#task-031--effectsystem-element-interaction)
  - [TASK-032 — EffectSystem: duration tick](#task-032--effectsystem-duration-tick)
  - [TASK-033 — CombatResolver: CheckCombinations](#task-033--combatresolver-checkcombinations)
  - [TASK-034 — CombatResolver: Detonates và Burn DoT](#task-034--combatresolver-detonates-và-burn-dot)
- [Phase 3 — Spell Layer](#phase-3--spell-layer)
  - [TASK-040 — SpellSlotManager: slot management](#task-040--spellslotmanager-slot-management)
  - [TASK-041 — CooldownTracker: cooldown logic](#task-041--cooldowntracker-cooldown-logic)
  - [TASK-042 — SpellCaster: validation pipeline](#task-042--spellcaster-validation-pipeline)
  - [TASK-043 — SpellCaster: thực thi effect theo spell data](#task-043--spellcaster-thực-thi-effect-theo-spell-data)
- [Phase 4 — Enemy AI](#phase-4--enemy-ai)
  - [TASK-050 — CombatContext snapshot](#task-050--combatcontext-snapshot)
  - [TASK-051 — SpellSelector và RandomPolicy](#task-051--spellselector-và-randompolicy)
  - [TASK-052 — PriorityPolicy, WeightedRandomPolicy, ScriptedPolicy](#task-052--prioritypolicy-weightedrandompolicy-scriptedpolicy)
- [Phase 5 — Turn System](#phase-5--turn-system)
  - [TASK-060 — TurnManager: async combat loop với UniTask](#task-060--turnmanager-async-combat-loop-với-unitask)
  - [TASK-061 — PhaseHandler: Start Phase resolve order](#task-061--phasehandler-start-phase-resolve-order)
  - [TASK-062 — PhaseHandler: End Phase và cooldown](#task-062--phasehandler-end-phase-và-cooldown)
- [Phase 6 — Presentation Layer (UniTask + DOTween)](#phase-6--presentation-layer-unitask--dotween)
  - [TASK-070 — VisualQueue: DrainAsync với UniTask](#task-070--visualqueue-drainasync-với-unitask)
  - [TASK-071 — TurnManager: await DrainAsync sau mỗi phase](#task-071--turnmanager-await-drainasync-sau-mỗi-phase)
  - [TASK-072 — IActionCommand và các command cơ bản với DOTween](#task-072--iactioncommand-và-các-command-cơ-bản-với-dotween)
  - [TASK-073 — ParallelCommand cho animation đồng thời](#task-073--parallelcommand-cho-animation-đồng-thời)
- [Phase 7 — Passive Layer](#phase-7--passive-layer)
  - [TASK-080 — EquipmentSystem: stat injection với StatModifier](#task-080--equipmentsystem-stat-injection-với-statmodifier)
  - [TASK-081 — RuneSystem: embed, purge, lifecycle + StatModifier](#task-081--runesystem-embed-purge-lifecycle--statmodifier)
- [Phase 8 — Foundation: StateMachine + EventBridge](#phase-8--foundation-statemachine--eventbridge)
  - [TASK-090 — GameManager states và StateMachineManager](#task-090--gamemanager-states-và-statemachinemanager)
  - [TASK-091 — GameManagerDriver MonoBehaviour](#task-091--gamemanagerdriver-monobehaviour)
  - [TASK-092 — EventBridge MonoBehaviours](#task-092--eventbridge-monobehaviours)
- [Phase 9 — Meta Layer](#phase-9--meta-layer)
  - [TASK-100 — GoldLedger](#task-100--goldledger)
  - [TASK-101 — RewardSystem, ShopSystem, EventSystem](#task-101--rewardsystem-shopsystem-eventsystem)
- [Phase 10 — Map Layer](#phase-10--map-layer)
  - [TASK-110 — MapSystem và NodeRouter](#task-110--mapsystem-và-noderouter)
- [Phase 11 — SaveSystem](#phase-11--savesystem)
  - [TASK-120 — GameManager state management và SaveSystem](#task-120--gamemanager-state-management-và-savesystem)
- [Phase 12 — UI Layer](#phase-12--ui-layer)
  - [TASK-130 — PlayerStatusView: HP/MP bar với DOTween](#task-130--playerstatusview-hpmp-bar-với-dotween)
  - [TASK-131 — EffectView, SpellBarView, TurnIndicatorView](#task-131--effectview-spellbarview-turnindicatorview)
  - [TASK-132 — MapGraphView và PendingModifierView](#task-132--mapgraphview-và-pendingmodifierview)
- [Phase 13 — Integration](#phase-13--integration)
  - [TASK-140 — Full combat loop: Player vs 1 Minion](#task-140--full-combat-loop-player-vs-1-minion)
  - [TASK-141 — Full combat loop: Element combos](#task-141--full-combat-loop-element-combos)

---

## Hướng dẫn đọc

|Ký hiệu|Nghĩa|
|---|---|
|`[ ]`|Chưa làm|
|`[~]`|Đang làm|
|`[x]`|Hoàn thành|
|**S / M / L**|Complexity ước tính (S = vài giờ, M = 1–2 ngày, L = 3+ ngày)|
|**Depends**|Task phải hoàn thành trước mới bắt đầu được task này|

**Acceptance Criteria (AC):** Mỗi AC là điều kiện cụ thể có thể verify bằng test hoặc quan sát trực tiếp trong Unity. Task chỉ được đánh dấu `[x]` khi **tất cả** AC đều pass.

---

## Phase 0 — Project Setup & Foundation

### TASK-001 — Khởi tạo project và folder structure

**Complexity:** S | **Depends:** —

**AC:**

- [x] Unity project tạo thành công, Unity version commit vào `ProjectSettings/`
- [x] Toàn bộ folder structure cấp 1 và cấp 2 tồn tại trong `Assets/` theo Naming Convention doc
- [x] Assembly Definition tạo đúng 4 assembly: `Logic`, `Unity`, `Tests.EditMode`, `Tests.PlayMode`
- [x] `Logic.asmdef` không reference `UnityEngine` — compile thành công khi chạy batch compile
- [x] `Foundation.asmdef` tách biệt khỏi `Logic` và `Unity` (xem TASK-009)

**Task Notes:**

| Subtask No. | Notes |
| ----------- | ----- |
| 1           | __    |
| 2           | __    |
| 3           | __    |
| 4           | __    |
| 5           | __    |

---
### TASK-002 — Cài đặt packages: UniTask và DOTween

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [x] UniTask cài qua UPM (`com.cysharp.unitask`) — `using Cysharp.Threading.Tasks;` compile không lỗi
- [x] DOTween cài và setup xong (`DOTween.Init()` gọi trong Bootstrap scene)
- [x] UniTask DOTween integration enable: `using DG.Tweening;` + `.ToUniTask()` extension khả dụng trên `Tween`
- [x] Test sanity: `await DOTween.To(() => 0f, x => {}, 1f, 0.5f).ToUniTask();` compile và chạy không lỗi trong Play Mode

**Task Notes:**

| Subtask No. | Notes                                                     |
| ----------- | --------------------------------------------------------- |
| 1           | __                                                        |
| 2           | Trong script DOTweenBoostrap.cs (Scripts/Unity/Bootstrap) |
| 3           | __                                                        |
| 4           | __                                                        |

---

### TASK-003 — Tạo ScriptableObject: `CombatConfig`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [x] `CombatConfig` SO tồn tại tại `Assets/Data/Config/CombatConfig.asset`
- [x] Có thể chỉnh `HIT_THRESHOLD`, `BASE_DODGE`, `MAX_DODGE`, `BASE_MP_RECOVERY`, `HP_CAP`, `HP_HALF`, `MP_COEFF` trong Inspector
- [x] Giá trị mặc định hợp lý được điền sẵn theo Combat Design doc

**Task Notes:**

| Subtask No. | Notes |
| ----------- | ----- |
| 1           | __    |
| 2           | __    |
| 3           | __    |


---

### TASK-004 — Tạo ScriptableObject: `ArcConfig`, `ShopPriceConfig`, `RewardRateConfig`, `EventConfig`, `WisdomSlotConfig`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [x] 3 asset `ArcConfig_1/2/3` — mỗi asset có field: tổng Node, tỷ lệ NodeType, min path constraint, tỷ lệ Rank Shop
- [x] `WisdomSlotConfig` có mảng 5 phần tử WIS threshold
- [x] `RewardRateConfig` có bảng tỷ lệ Rank theo combatType × Arc (tổng mỗi dòng = 100%)
- [x] Không có serialization error trong Inspector

---

### TASK-005 — `StatModifier` struct và `StatType` enum

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [x] `StatModifier` struct nằm trong `Logic` assembly: `{ StatType stat, ModType modType, float value }`
- [x] `ModType` enum: `Flat`, `PercentAdd`
- [x] `StatType` enum đủ tất cả stat có thể modify: `POT, SPI, WIS, VIT, AGI, FirePotency, WaterPotency, IcePotency, LightningPotency, FireRes, WaterRes, IceRes, LightningRes, MaxHp, MaxMp, MpRecovery, AllPotency, AllResistance`
- [x] Struct serialize được trong Unity Inspector (đánh dấu `[Serializable]`)
- [x] **Edit Mode Test**: tạo vài `StatModifier`, đọc lại field đúng giá trị

---

### TASK-006 — Tạo `SpellDefinition` và data Rank I

**Complexity:** M | **Depends:** TASK-001

**AC:**

- [ ] `SpellDefinition` ScriptableObject class có đủ field: `id`, `displayName`, `rank`, `element`, `baseCost`, `baseCooldown`, **`minWisdomToImprint`** (WIS tối thiểu để Imprint — GDD), `targetType`, `effects[]`
- [ ] `EffectApplication` serializable: `effectType`, `valueFormula` (string), `condition` (nullable, `[SerializeReference]`)
- [ ] 12 asset Rank I Spell tại `Assets/Data/Spells/RankI/`, naming đúng convention (`SP_Fireball.asset`...)
- [ ] Mỗi asset điền đủ data theo GDD — Spells (Rank I), không field nào bỏ trống không hợp lý

---

### TASK-007 — Tạo `EnemyDefinition` schema

**Complexity:** S | **Depends:** TASK-006

**AC:**

- [ ] `EnemyDefinition` ScriptableObject có đủ field theo Architecture doc
- [ ] `decisionPolicyConfig` serialize được các subtype bằng `[SerializeReference]`
- [ ] Tạo 1 enemy mẫu `EN_TestMinion` — `RandomPolicy`, 1 spell — không bị serialize error

---

### TASK-008 — Tạo `EquipmentDefinition` và `RuneDefinition` schema

**Complexity:** S | **Depends:** TASK-005

**AC:**

- [ ] `EquipmentDefinition` có field: `id`, `displayName`, `slot` (EquipmentSlot enum), `rank`, `modifiers: StatModifier[]`
- [ ] `RuneDefinition` có field: `id`, `displayName`, `rank`, `passiveType` (StatModifier/ConditionalTrigger/TurnHook), `statModifiers: StatModifier[]`, `passiveConfig` (`[SerializeReference]`)
- [ ] Tạo 1 equipment mẫu Rank I (1 modifier flat) — không serialize error
- [ ] Tạo 1 equipment mẫu Rank II (flat MaxHp + flat VIT) — `Garb_IronRobe` — không serialize error
- [ ] Tạo 1 rune mẫu `StatModifier` type — `Rune_FireEmbrace` — không serialize error

---

### TASK-009 — Foundation Layer: `EventChannelSO` và `StateMachineManager`

**Complexity:** S | **Depends:** TASK-001

Đưa code `EventChannelSO`, `EventListener`, `IState`, `StateMachineManager` vào project đúng namespace và assembly.

**AC:**

- [ ] `Core.Foundation.Events.SinglePayloadEvent` và `Core.Foundation.Events.TwoPayloadEvent` namespace tồn tại
- [ ] `Core.Foundation.StateMachine` namespace tồn tại với `IState`, `StateMachineManager`
- [ ] `Foundation.asmdef` không reference `Logic.asmdef` hoặc `Unity.asmdef` (utility thuần)
- [ ] `IAsyncState` interface tồn tại — extend `IState` với `UniTask EnterAsync()`
- [ ] `StateMachineManager.SetState()` detect `IAsyncState` và gọi `EnterAsync()` thay vì `Enter()`
- [ ] Compile không lỗi toàn bộ project

---

### TASK-010 — `EventChannelSO` assets cho cross-boundary events

**Complexity:** S | **Depends:** TASK-009

**AC:**

- [ ] 5 SO asset tại `Assets/Data/Events/`:
    - `CombatEndedSO.asset` — payload: `CombatResult`
    - `NodeEnteredSO.asset` — payload: `NodeType`
    - `GoldChangedSO.asset` — payload: `int`
    - `PendingModifierChangedSO.asset` — payload: `CombatModifier?`
    - `RewardReadySO.asset` — payload: `RewardOffer`
- [ ] Mỗi SO là concrete class extend đúng `EventChannelSO<T>` generic base
- [ ] `RaiseEvent()` và `AddListener()` / `RemoveListener()` gọi được từ Inspector và code

---

## Phase 1 — Core Math (Pure Logic)

### TASK-020 — `ArmorStack`: implement và unit test

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] `ApplyArmor(value, duration)` tạo stack mới với `hurt_order` tăng dần
- [ ] `TakeDamage(damage)` trả về đúng overflow khi damage < tổng armor
- [ ] `TakeDamage(damage)` trả về đúng overflow khi damage > tổng armor
- [ ] Damage tràn đúng thứ tự từ `hurt_order` cao nhất xuống thấp nhất
- [ ] `Tick()` giảm duration đúng 1; xóa stack khi `duration == 0` dù còn `value`
- [ ] Stack bị xóa giữa chừng → stack tiếp theo nhận đúng overflow
- [ ] **Edit Mode Test pass**

---

### TASK-021 — `DamageCalculator`: implement và unit test

**Complexity:** S | **Depends:** TASK-020

**AC:**

- [ ] `Calculate(rawDamage, resistance)` → đúng `rawDamage × 90 / (resistance + 90)`
- [ ] resistance = 0 → actual = raw; resistance = 90 → actual = 50%; resistance = 180 → actual ≈ 33.3%
- [ ] `crystalizeFlag = true` → actual = 0
- [ ] `ApplyRawDamage(ignoreResistance=true)` bỏ qua resistance formula
- [ ] `ApplyRawDamage(ignoreArmor=true)` trừ thẳng vào HP, bỏ qua `ArmorStack`
- [ ] **Edit Mode Test pass**

---

### TASK-022 — `HitDodgeResolver`: implement và unit test

**Complexity:** S | **Depends:** TASK-003

**AC:**

- [ ] `hit_delta >= HIT_THRESHOLD` → guaranteed hit
- [ ] `hit_delta = 0` → `dodge_chance = BASE_DODGE`
- [ ] `dodge_chance` không bao giờ vượt `MAX_DODGE`
- [ ] Random seed cố định → kết quả deterministic
- [ ] **Edit Mode Test pass**

---

### TASK-023 — `PlayerController`: HP/MP scaling và Stat Layering

**Complexity:** M | **Depends:** TASK-003, TASK-005

**AC:**

- [ ] VIT = 10 → `storedMaxHp = 100` (công thức `1000 × VIT / (VIT + 90)` — L0 thuần, không có L1 flat)
- [ ] VIT = 10, flat MaxHp modifier +50 → `storedMaxHp = 150` (L0 + L1)
- [ ] SPI = 10 → `storedMaxMp = 100`; `storedMpRecovery = BASE_MP_RECOVERY + 10`
- [ ] `RecalculateBaseAttributes()` thứ tự đúng: collect → main attr → potency (L0+L1+L2) → resist → HP/MP → recovery
- [ ] Sau `RecalculateBaseAttributes()`: `maxHp` và `maxMp` alias đúng `storedMaxHp`/`storedMaxMp`
- [ ] **Edit Mode Test pass**

---

### TASK-024 — `PlayerController`: Stat Layering với equipment/rune modifier

**Complexity:** M | **Depends:** TASK-023

**AC:**

- [ ] Equip 1 item `{ stat: POT, modType: Flat, value: 8 }` → `POT` tăng 8
- [ ] Equip item `{ stat: FirePotency, modType: Flat, value: 15 }` → `storedFirePotency = L0 + 15`
- [ ] Equip item `{ stat: FirePotency, modType: PercentAdd, value: 0.10 }` → `storedFirePotency = (L0 + L1) × 1.10`
- [ ] Stacking: flat +15 + percent +10% → `(L0 + 15) × 1.10` (đúng thứ tự L1 trước L2)
- [ ] `{ stat: AllPotency, modType: Flat, value: 10 }` → cả 4 `storedXPotency` đều tăng 10
- [ ] `{ stat: MaxHp, modType: Flat, value: 50 }` → `storedMaxHp = formulaResult + 50`
- [ ] Unequip item → `RecalculateBaseAttributes()` → về giá trị gốc
- [ ] **Edit Mode Test pass**

---

### TASK-025 — `PlayerController`: Dynamic getters (Layer 3)

**Complexity:** S | **Depends:** TASK-024

**AC:**

- [ ] Không có effect: `GetEffectivePotency()` = `storedXPotency`
- [ ] Enrage: `GetEffectivePotency()` = `stored × 1.15`
- [ ] Drenched: `GetEffectivePotency()` = `stored × 0.90`
- [ ] Enrage + Drenched: `stored × 1.05` (cộng multiplier, không nhân liên tiếp)
- [ ] Overdrive: `stored × 1.30` (Enrage + Overdrive stacking)
- [ ] `GetEffectiveAGI()`: Overdrive → `float.PositiveInfinity`; Energized → `AGI × 1.30`; Chilled → `AGI × 0.70`
- [ ] `GetEffectiveResistance()`: Fortified +15%, Dazed -15%
- [ ] L3 effect hoàn toàn tách biệt L0–L2 — thay đổi equipment không ảnh hưởng L3 và ngược lại
- [ ] **Edit Mode Test pass**

---

## Phase 2 — Effect System

### TASK-030 — `EffectSystem`: apply và lookup

**Complexity:** M | **Depends:** TASK-001

**AC:**

- [ ] `Apply(EffectType)` thêm effect vào `activeEffects` Dictionary
- [ ] `Has(EffectType)` → `true` khi active
- [ ] Apply cùng loại (non-stackable): Refresh duration, `Count` không tăng
- [ ] `Remove(EffectType)` → `Has()` trả `false`
- [ ] `OnEffectApplied` C# event phát khi apply thành công
- [ ] `OnEffectRemoved` C# event phát khi remove
- [ ] **Edit Mode Test pass**

---

### TASK-031 — `EffectSystem`: element interaction

**Complexity:** M | **Depends:** TASK-030

**AC:**

- [ ] Water > Fire: apply Burn khi có Drenched → Burn bị giải, Drenched giữ
- [ ] Water > Fire: apply Drenched khi có Burn → Burn bị giải, Drenched apply
- [ ] Cùng nguyên tố, khác loại (Enrage + Burn): Neutralize cả hai, `activeEffects` trống
- [ ] Cùng nguyên tố, cùng loại (Burn + Burn): Refresh duration, count vẫn 1
- [ ] `OnEffectApplied` **không** phát khi Abort
- [ ] **Edit Mode Test pass** cho tất cả case

---

### TASK-032 — `EffectSystem`: duration tick

**Complexity:** S | **Depends:** TASK-031

**AC:**

- [ ] `duration = 2` → sau 2 lần `Tick()` bị xóa
- [ ] `duration = -1` (sentinel ∞) → không bị xóa
- [ ] `OnEffectRemoved` phát khi expire do Tick
- [ ] `duration = 1` → xóa sau đúng 1 lần Tick
- [ ] **Edit Mode Test pass**

---

### TASK-033 — `CombatResolver`: CheckCombinations

**Complexity:** M | **Depends:** TASK-032

**AC:**

- [ ] Enrage + Energized → `EffectSystem.Apply(Overdrive)` gọi
- [ ] Refreshing + Fortified → `EffectSystem.Apply(Crystalize)` gọi
- [ ] Drenched + Chilled → `EffectSystem.Apply(Frozen)` gọi
- [ ] Burn + Dazed → `OnCombinationTriggered(Detonates)` event phát
- [ ] Chỉ 1 trong 2 effect → không trigger
- [ ] Idempotent: gọi `CheckCombinations` nhiều lần liên tiếp → combo chỉ trigger 1 lần
- [ ] **Edit Mode Test pass**

---

### TASK-034 — `CombatResolver`: Detonates và Burn DoT

**Complexity:** M | **Depends:** TASK-033, TASK-021

**AC:**

- [ ] Detonates: target nhận đúng `30% × maxHp`
- [ ] Detonates: ignoreResistance=true (không giảm bởi fire_res)
- [ ] Detonates: ignoreArmor=true (bypass ArmorStack, trừ thẳng HP)
- [ ] Detonates khi Crystalize flag: vẫn gây damage (Crystalize không block Detonates)
- [ ] Burn DoT: `10% × maxHp` giảm bởi fire_res
- [ ] Burn DoT khi Crystalize flag: damage = 0
- [ ] **Edit Mode Test pass**

---

## Phase 3 — Spell Layer

### TASK-040 — `SpellSlotManager`: slot management

**Complexity:** S | **Depends:** TASK-004

**AC:**

- [ ] Bắt đầu với `openSlots = 1`
- [ ] `Imprint(spell)` vào slot trống → `IsImprinted(spell) = true`
- [ ] `Forget(spell)` → slot trống, `IsImprinted = false`
- [ ] `Imprint` vào slot chưa mở → thất bại (`ImprintResult.SlotLocked` hoặc tương đương)
- [ ] `Imprint` khi `player.WIS < spell.minWisdomToImprint` → thất bại (`ImprintResult.NotEnoughWisdom`)
- [ ] `UnlockSlot()` → slot mới mở, có thể Imprint
- [ ] Số slot mở dựa đúng theo `WisdomSlotConfig` threshold (độc lập với ngưỡng Imprint từng spell)
- [ ] **Edit Mode Test pass**

---

### TASK-041 — `CooldownTracker`: cooldown logic

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] `GetCooldown(spellId)` trả 0 cho spell chưa cast
- [ ] `Set(spellId, 3)` → `GetCooldown` = 3; sau 3 lần `Tick()` → 0
- [ ] `OnCooldownChanged` event phát sau mỗi `Tick()` khi cooldown > 0
- [ ] Nhiều spell tracking độc lập
- [ ] **Edit Mode Test pass**

---

### TASK-042 — `SpellCaster`: validation pipeline

**Complexity:** M | **Depends:** TASK-040, TASK-041, TASK-025

**AC:**

- [ ] Không trong slot → `CastResult.SpellNotImprinted`
- [ ] Đang cooldown → `CastResult.SpellOnCooldown`
- [ ] Không đủ MP → `CastResult.NotEnoughMp`
- [ ] Không phải Action Phase → `CastResult.NotYourTurn`
- [ ] Tất cả thỏa → `CastResult.Success`
- [ ] Miss: MP không trừ, cooldown không set
- [ ] Success: MP trừ đúng cost, cooldown set đúng `baseCooldown`
- [ ] Distracted: `cost = base × 1.15` (làm tròn lên)
- [ ] **Edit Mode Test pass**

---

### TASK-043 — `SpellCaster`: thực thi effect theo spell data

**Complexity:** M | **Depends:** TASK-042, TASK-033

**AC:**

- [ ] Spell có damage: `DamageCalculator` gọi với đúng potency và element
- [ ] Spell buff lên Self: `EffectSystem(player).Apply()` gọi
- [ ] Spell debuff lên enemy: `EffectSystem(enemy).Apply()` gọi
- [ ] `targetType = AllEnemies`: tất cả enemy nhận damage/effect
- [ ] `targetType = Random(n)`: đúng n enemy ngẫu nhiên
- [ ] Sau apply effect → `CombatResolver.CheckCombinations()` gọi
- [ ] **Edit Mode Test pass** (dùng StubEntity)

---

## Phase 4 — Enemy AI

### TASK-050 — `CombatContext` snapshot

**Complexity:** S | **Depends:** TASK-023, TASK-032

**AC:**

- [ ] `CombatContext` tạo được từ PlayerController và EnemyController
- [ ] `playerHpPercent`, `selfHpPercent` tính đúng
- [ ] `playerActiveEffects` là read-only snapshot
- [ ] Thay đổi state sau khi tạo context → không ảnh hưởng giá trị trong context
- [ ] **Edit Mode Test pass**

---

### TASK-051 — `SpellSelector` và `RandomPolicy`

**Complexity:** S | **Depends:** TASK-041, TASK-050

**AC:**

- [ ] `SpellSelector`: spell cooldown = 0 → xuất hiện; cooldown > 0 → không xuất hiện
- [ ] `RandomPolicy.SelectSpell(available, context)` → trả 1 spell trong `available`
- [ ] Statistical test n=1000: tất cả spell trong list được chọn ít nhất 1 lần
- [ ] **Edit Mode Test pass**

---

### TASK-052 — `PriorityPolicy`, `WeightedRandomPolicy`, `ScriptedPolicy`

**Complexity:** M | **Depends:** TASK-051

**AC:**

- [ ] `PriorityPolicy`: `PlayerHas(Burn)` — chọn spell khi player bị Burn, bỏ qua khi không
- [ ] `PriorityPolicy`: `PlayerHpBelow(0.3f)` — chọn khi player HP < 30%
- [ ] `PriorityPolicy`: priority cao check trước; spell on cooldown → skip sang rule tiếp theo
- [ ] `PriorityPolicy`: không rule nào thỏa → fallback Random
- [ ] `WeightedRandomPolicy`: weight cao → xác suất cao (statistical n=1000); weight 0 → không bao giờ chọn
- [ ] `ScriptedPolicy`: lần lượt theo sequence; spell on cooldown → skip, không block
- [ ] **Edit Mode Test pass**

---

## Phase 5 — Turn System

### TASK-060 — `TurnManager`: async combat loop với UniTask

**Complexity:** M | **Depends:** TASK-042, TASK-052

**AC:**

- [ ] `StartCombatAsync(CancellationToken ct)` là `async UniTask`, không phải Coroutine
- [ ] Phase order: Player (Start→Action→End) → Enemy (Start→Action→End) → lặp
- [ ] Win: tất cả enemy HP ≤ 0 → loop kết thúc → `OnCombatEnded(Win)` C# event phát
- [ ] Lose: player HP ≤ 0 → loop kết thúc → `OnCombatEnded(Lose)` C# event phát
- [ ] `OnPhaseChanged` event phát tại đầu mỗi phase
- [ ] `ct.Cancel()` từ bên ngoài → loop dừng sạch, không throw unhandled exception
- [ ] Không có `IEnumerator` hay `StartCoroutine` trong `TurnManager`
- [ ] **Play Mode Test** trong TestCombatScene

---

### TASK-061 — `PhaseHandler`: Start Phase resolve order

**Complexity:** M | **Depends:** TASK-034, TASK-060

**AC:**

- [ ] MP Recovery xảy ra bước 1 (trước mọi damage)
- [ ] Frozen check bước 2: Action Phase bị skip đúng 1 lượt, Frozen giải
- [ ] Crystalize flag bật bước 3 (trước Burn bước 5)
- [ ] Regen (bước 4) xảy ra trước Burn (bước 5) — verify bằng HP log order
- [ ] Burn bỏ qua khi Crystalize flag đang bật
- [ ] Combination check (bước 7) xảy ra sau tất cả status resolve
- [ ] **Play Mode Test**: scenario cụ thể từng bước

---

### TASK-062 — `PhaseHandler`: End Phase và cooldown

**Complexity:** S | **Depends:** TASK-061

**AC:**

- [ ] Effect duration giảm 1, hết duration bị xóa sau End Phase
- [ ] Armor stack duration giảm 1, hết duration bị xóa sau End Phase
- [ ] Spell cooldown giảm 1, không giảm xuống dưới 0
- [ ] **Edit Mode Test + Play Mode Test**

---

## Phase 6 — Presentation Layer (UniTask + DOTween)

### TASK-070 — `VisualQueue`: DrainAsync với UniTask

**Complexity:** M | **Depends:** TASK-002, TASK-001

**AC:**

- [ ] `VisualQueue` là MonoBehaviour, không dùng Coroutine
- [ ] `Enqueue(IActionCommand)` thêm vào Queue
- [ ] `DrainAsync(CancellationToken)` là `async UniTask` — execute tuần tự đến khi queue rỗng
- [ ] Không có `OnQueueDrained` event — caller `await DrainAsync()` trực tiếp
- [ ] `ct.Cancel()` trong khi drain → command hiện tại bị cancel, drain dừng sạch
- [ ] **Play Mode Test** với mock command `WaitCommand(0.1f)`

---

### TASK-071 — `TurnManager`: await DrainAsync sau mỗi phase

**Complexity:** S | **Depends:** TASK-070, TASK-060

**AC:**

- [ ] Sau mỗi `RunPhaseAsync()` → `await visualQueue.DrainAsync(ct)` trước khi phase tiếp theo
- [ ] Phase chỉ chuyển SAU khi `DrainAsync` hoàn thành
- [ ] VisualQueue empty → `DrainAsync` return ngay (không đợi không cần thiết)
- [ ] **Play Mode Test**: verify timing bằng `Time.time` log

---

### TASK-072 — `IActionCommand` và các command cơ bản với DOTween

**Complexity:** M | **Depends:** TASK-070, TASK-002

**AC:**

- [ ] `IActionCommand` interface: `UniTask ExecuteAsync(CancellationToken ct = default)` — **không** có `IEnumerator`
- [ ] `ShowDamageNumberCommand`: floating text với số damage, DOTween fade out sau 1 giây — `await` xong mới return
- [ ] `ShowHpGainCommand`: floating text màu xanh, DOTween fade
- [ ] `PlayDamageAnimCommand`: `transform.DOShakePosition(...).ToUniTask(ct)` — đợi shake xong
- [ ] `PlayDeathAnimCommand`: `transform.DOScale(0, 0.3f).ToUniTask(ct)` rồi deactivate
- [ ] `PlayEffectApplyAnimCommand`: Instantiate VFX prefab, await duration, Destroy
- [ ] Mỗi command khi `ct` cancel → tween bị `.Kill()`, không để orphan tween
- [ ] **Play Mode Test**: sequence 3 command, verify thứ tự và timing

---

### TASK-073 — `ParallelCommand` cho animation đồng thời

**Complexity:** S | **Depends:** TASK-072

**AC:**

- [ ] `ParallelCommand(params IActionCommand[] cmds)` — `ExecuteAsync` dùng `UniTask.WhenAll()`
- [ ] Tất cả command trong parallel hoàn thành trước khi `ExecuteAsync` return
- [ ] Một command trong parallel cancel → tất cả còn lại cũng được cancel
- [ ] Dùng để: Detonates VFX + ShowDamageNumber cùng lúc
- [ ] **Play Mode Test** với 2 WaitCommand có thời gian khác nhau — verify kết thúc đúng lúc cái lâu hơn

---

## Phase 7 — Passive Layer

### TASK-080 — `EquipmentSystem`: stat injection với StatModifier

**Complexity:** S | **Depends:** TASK-024, TASK-008

**AC:**

- [ ] `EquipmentSystem.GetAllModifiers()` → `StatModifier[]` từ tất cả equipped item
- [ ] Equip item Rank II có 2 modifier → cả 2 apply vào `RecalculateBaseAttributes()`
- [ ] Equip/unequip → `RecalculateBaseAttributes()` tự gọi
- [ ] Equip vào slot đã có → item cũ replace, stat cập nhật đúng
- [ ] `{ stat: AllPotency }` modifier → cả 4 nguyên tố potency tăng
- [ ] **Edit Mode Test pass**

---

### TASK-081 — `RuneSystem`: embed, purge, lifecycle + StatModifier

**Complexity:** M | **Depends:** TASK-080, TASK-032

**AC:**

- [ ] `Embed(rune, socketIndex)` → `IRunePassive.OnEmbed(player)` gọi
- [ ] `Purge(socketIndex)` → `IRunePassive.OnPurge(player)` gọi **trước** khi xóa rune
- [ ] Rune `passiveType = StatModifier`: `RuneSystem.GetAllStatModifiers()` trả modifier đúng → vào `RecalculateBaseAttributes()`
- [ ] Rune `passiveType = ConditionalTrigger`: sau Embed → callback đăng ký trong `EffectSystem.OnEffectApplied`
- [ ] Rune `passiveType = ConditionalTrigger`: sau Purge → callback bị xóa
- [ ] Verify no memory leak: `EffectSystem.OnEffectApplied.GetInvocationList()` không còn rune callback sau Purge
- [ ] Embed vào socket đã có rune → thất bại
- [ ] **Edit Mode Test pass**

---

## Phase 8 — Foundation: StateMachine + EventBridge

### TASK-090 — `GameManager` states và `StateMachineManager`

**Complexity:** M | **Depends:** TASK-009

**AC:**

- [ ] 6 state class implement `IState`: `InMapState`, `InCombatState`, `InShopState`, `InRestState`, `InEventState`, `InRewardState`
- [ ] `InCombatState.Enter()` gọi `TurnManager.StartCombatAsync(ct)` bằng `UniTask.Void` (fire-and-forget, không block Tick)
- [ ] `GameManager.BuildStateMachine()` setup đủ transitions: `NodeEnteredSO` → đúng state
- [ ] `StateMachineManager.Tick()` check transition đúng, gọi `SetState` khi predicate thỏa
- [ ] `AddAnyTransition` từ bất kỳ state → `InMapState` khi node exit
- [ ] **Edit Mode Test**: mock predicate, verify state transition đúng

---

### TASK-091 — `GameManagerDriver` MonoBehaviour

**Complexity:** S | **Depends:** TASK-090

**AC:**

- [ ] `GameManagerDriver : MonoBehaviour` tồn tại trong `Scripts/Unity/Core/`
- [ ] `Awake()` tạo `GameManager`, gọi `BuildStateMachine()`
- [ ] `Update()` gọi `_fsm.Tick()`
- [ ] `FixedUpdate()` gọi `_fsm.FixedTick()`
- [ ] `StartRun()` được gọi đúng chỗ khi game bắt đầu
- [ ] **Play Mode Test**: verify Tick() được gọi mỗi frame

---

### TASK-092 — `EventBridge` MonoBehaviours

**Complexity:** S | **Depends:** TASK-010, TASK-060, TASK-091

Tạo bridge adapter cho từng cross-boundary event.

**AC:**

- [ ] `CombatEndedBridge`: subscribe `TurnManager.OnCombatEnded` → raise `CombatEndedSO`; unsubscribe trong `OnDestroy()`
- [ ] `NodeEnteredBridge`: `NodeRouter` phát C# event → raise `NodeEnteredSO`
- [ ] `GoldChangedBridge`: `GoldLedger.OnBalanceChanged` → raise `GoldChangedSO`
- [ ] Mỗi Bridge không chứa logic — chỉ relay
- [ ] Verify: sau `OnDestroy()` của Bridge, SO không còn nhận event cũ
- [ ] **Play Mode Test**: raise C# event → verify SO.RaiseEvent() được gọi đúng payload

---

## Phase 9 — Meta Layer

### TASK-100 — `GoldLedger`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] `Earn(100)` → `balance = 100`
- [ ] `Spend(60)` khi đủ → `balance = 40`, return `true`
- [ ] `Spend(60)` khi thiếu → không đổi, return `false`
- [ ] `OnBalanceChanged` C# event phát sau mỗi Earn/Spend thành công
- [ ] **Edit Mode Test pass**

---

### TASK-101 — `RewardSystem`, `ShopSystem`, `EventSystem`

**Complexity:** M | **Depends:** TASK-006, TASK-008, TASK-100

**AC:**

- [ ] `RewardSystem.GenerateOffer(Minion, Arc1)` → 1 Equipment + 1 Spell + 1 Rune; Rank phân bổ đúng xác suất (n=1000)
- [ ] `ShopSystem.GenerateInventory(Arc1)` → 5 Equipment + 5 Spell + 5 Rune; Rank đúng tỷ lệ
- [ ] `ShopSystem.TryPurchase()`: đủ tiền → GoldLedger.Spend(); thiếu tiền → `NotEnoughGold`
- [ ] `EventSystem.InitArc()` → shuffle queue; không lặp Event trong Arc
- [ ] `Force Trade` event → `GameManager.pendingCombatModifier` set đúng
- [ ] **Edit Mode Test pass**

---

## Phase 10 — Map Layer

### TASK-110 — `MapSystem` và `NodeRouter`

**Complexity:** L | **Depends:** TASK-004

**AC:**

- [ ] `GenerateArc(arcConfig)` không throw exception với 100 random seed khác nhau
- [ ] Tất cả path đến Boss đều thỏa ràng buộc Elite/Rest/Shop/Event tối thiểu — verify DFS/BFS
- [ ] Mỗi Node có 1–3 cạnh ra, không Node bị cô lập
- [ ] `NodeRouter`: Enter đúng node type → dispatch đúng handler
- [ ] Enter node đã visited → bị chặn
- [ ] **Edit Mode Test pass** (không cần visual)

---

## Phase 11 — SaveSystem

### TASK-120 — `GameManager` state management và `SaveSystem`

**Complexity:** M | **Depends:** TASK-091, TASK-100

**AC:**

- [ ] `StartRun()` khởi tạo player 10 điểm mỗi attribute, inventory trống
- [ ] `pendingCombatModifier` cleared sau combat kết thúc
- [ ] `SaveSystem.Save()` tạo JSON đủ: `currentArc`, `mapSeed`, `visitedNodeIds[]`, `playerSnapshot`, `gold`, `pendingCombatModifier`
- [ ] `SaveSystem.Load()` khôi phục đúng state; map giống hệt khi dùng cùng `mapSeed`
- [ ] Save không xảy ra trong combat — `Assert.IsFalse(SaveSystem.isSaving)` khi TurnManager đang loop
- [ ] **Edit Mode Test pass**

---

## Phase 12 — UI Layer

### TASK-130 — `PlayerStatusView`: HP/MP bar với DOTween

**Complexity:** S | **Depends:** TASK-023, TASK-070

**AC:**

- [ ] HP bar animate bằng DOTween khi `OnHpChanged` event phát (không snap)
- [ ] MP bar cập nhật khi `OnMpChanged` phát
- [ ] View không reference trực tiếp `PlayerController` — chỉ subscribe C# event
- [ ] `OnDestroy()` unsubscribe tất cả — verify invocation count về 0
- [ ] **Play Mode Test**

---

### TASK-131 — `EffectView`, `SpellBarView`, `TurnIndicatorView`

**Complexity:** M | **Depends:** TASK-032, TASK-041, TASK-060

**AC:**

- [ ] `EffectView`: Apply Burn → icon xuất hiện; expire → biến mất; Neutralize → cả hai mất đồng thời
- [ ] `SpellBarView`: cooldown overlay đúng; ngoài Action Phase → button disable; tap → `SpellCaster.TryCast()` → `CastResult` → feedback DOTween (shake nếu fail)
- [ ] `TurnIndicatorView`: subscribe `TurnManager.OnPhaseChanged` → cập nhật đúng
- [ ] Tất cả View unsubscribe trong `OnDestroy()`
- [ ] **Play Mode Test**

---

### TASK-132 — `MapGraphView` và `PendingModifierView`

**Complexity:** M | **Depends:** TASK-110, TASK-092

**AC:**

- [ ] Node đúng icon theo type; visited → dim; chỉ node kề phía trước tap được
- [ ] `PendingModifierView`: subscribe `PendingModifierChangedSO` → xuất hiện/ẩn đúng
- [ ] Tap node → `NodeRouter.EnterNode(nodeId)` gọi
- [ ] `OnDestroy()` unsubscribe SO listener
- [ ] **Play Mode Test**

---

## Phase 13 — Integration

### TASK-140 — Full combat loop: Player vs 1 Minion

**Complexity:** L | **Depends:** TASK-071, TASK-072, TASK-131

**AC:**

- [ ] Bắt đầu combat: SpellBar active, TurnIndicator "Player Turn"
- [ ] Player cast Fireball → Minion nhận damage đúng số, DOTween shake animation, floating damage number
- [ ] Enemy turn: enemy cast → Player nhận damage, animation chạy
- [ ] Minion HP = 0 → DOTween death anim → Reward screen → `CombatEndedSO.RaiseEvent(Win)`
- [ ] Player HP = 0 → Game Over, `CombatEndedSO.RaiseEvent(Lose)`
- [ ] Không có null reference, không orphan tween sau combat kết thúc
- [ ] **Manual test**

---

### TASK-141 — Full combat loop: Element combos

**Complexity:** M | **Depends:** TASK-140

**AC:**

- [ ] Burn + Dazed → Detonates: 30% maxHp instant, bypass armor — `ParallelCommand` VFX + number đồng thời
- [ ] Refreshing + Fortified → Crystalize: lượt sau miễn damage
- [ ] Enrage + Energized → Overdrive: guaranteed hit, ×1.30 damage
- [ ] Drenched + Chilled → Frozen: enemy skip Action Phase 1 lượt
- [ ] **Manual test**

---

### TASK-142 — Full game flow: Map → Combat → Shop → Rest → Map

**Complexity:** L | **Depends:** TASK-140, TASK-132, TASK-101

**AC:**

- [ ] `GameManagerDriver.Tick()` chuyển state đúng theo event SO
- [ ] Map → Minion Combat → Win → Reward → Map (node marked visited)
- [ ] Map → Shop → mua item → Gold giảm → Map
- [ ] Map → Rest → chọn attribute → RecalculateBaseAttributes() → Map
- [ ] `PendingModifierView` xuất hiện sau Force Trade event, biến mất sau combat
- [ ] Không có memory leak, không orphan UniTask, không orphan DOTween
- [ ] **Manual test end-to-end**

---

### TASK-143 — Save và Load giữa session

**Complexity:** M | **Depends:** TASK-142, TASK-120

**AC:**

- [ ] Thắng combat, Stop Play Mode (simulate quit)
- [ ] Play lại → load đúng: Arc, map layout, inventory, gold
- [ ] Không mất item đã nhận
- [ ] **Manual test**

---

## Ghi chú

- **Edit Mode Test** → `Assets/Tests/EditMode/`; **Play Mode Test** → `Assets/Tests/PlayMode/`
- Phase 0–4 ưu tiên Edit Mode Test trước khi integrate vào scene
- Task `[~]` chỉ một người nhận tại một thời điểm
- **Không dùng `StartCoroutine` hoặc `IEnumerator`** trong bất kỳ code mới nào từ Phase 5 trở đi
- **DOTween tween phải được `.Kill()` khi cancel** — không để orphan tween sau scene unload
- Khi task xong, cập nhật status và ghi ngày hoàn thành