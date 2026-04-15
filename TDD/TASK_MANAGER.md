___
**Game:** Wandering Wanderer 
**Author:** DukTofn 
**Last Updated:** 15/04/2026 
**Revision:** v4 — Architecture v8 (AvailableAttributePoints system)
___

# TDD — Task Manager

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

### TASK-001 — Khởi tạo project và folder structure `[x]`

**Complexity:** S | **Depends:** —

**AC:**

- [x] Unity project tạo thành công, Unity version commit vào `ProjectSettings/`
- [x] Toàn bộ folder structure cấp 1 và cấp 2 tồn tại trong `Assets/` theo Naming Convention doc
- [x] Assembly Definition tạo đúng 4 assembly: `Logic`, `Unity`, `Tests.EditMode`, `Tests.PlayMode`
- [x] `Logic.asmdef` không reference `UnityEngine` — compile thành công khi chạy batch compile
- [x] `Foundation.asmdef` tách biệt khỏi `Logic` và `Unity` (xem TASK-009)

---

### TASK-002 — Cài đặt packages: UniTask và DOTween `[x]`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [x] UniTask cài qua UPM (`com.cysharp.unitask`) — `using Cysharp.Threading.Tasks;` compile không lỗi
- [x] DOTween cài và setup xong (`DOTween.Init()` gọi trong Bootstrap scene)
- [x] UniTask DOTween integration enable: `using DG.Tweening;` + `.ToUniTask()` extension khả dụng trên `Tween`
- [x] Test sanity: `await DOTween.To(() => 0f, x => {}, 1f, 0.5f).ToUniTask();` compile và chạy không lỗi trong Play Mode

---

### TASK-003 — Tạo ScriptableObject: `CombatConfig` `[x]`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [x] `CombatConfig` SO tồn tại tại `Assets/Data/Config/CombatConfig.asset`
- [x] Có thể chỉnh `HIT_THRESHOLD`, `BASE_DODGE`, `MAX_DODGE`, `BASE_MP_RECOVERY`, `HP_CAP`, `HP_HALF`, `MP_COEFF` trong Inspector
- [x] Giá trị mặc định hợp lý được điền sẵn theo Combat Design doc

---

### TASK-004 — Tạo ScriptableObject: `ArcConfig`, `ShopPriceConfig`, `RewardRateConfig`, `EventConfig`, `WisdomSlotConfig` `[~]`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [x] 3 asset `ArcConfig_1/2/3` — mỗi asset có field: tổng Node, tỷ lệ NodeType, min path constraint, tỷ lệ Rank Shop
- [x] `WisdomSlotConfig` có mảng 5 phần tử WIS threshold
- [x] `RewardRateConfig` có bảng tỷ lệ Rank theo combatType × Arc (tổng mỗi dòng = 100%)
- [ ] `ShopPriceConfig` có giá cố định theo Rank (Equipment/Spell/Rune), giá dịch vụ (Enlighten/Embed/Purge), giá Rune Socket (lũy tiến)
- [ ] `EventConfig` có tỷ lệ từng Event, giá trị cụ thể (Gold amount, HP cost, Attribute bonus) cho mỗi Event trong pool
- [x] Không có serialization error trong Inspector

---

### TASK-005 — `StatModifier` struct và `StatType` enum `[x]`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [x] `StatModifier` struct nằm trong `Logic` assembly: `{ StatType stat, ModType modType, float value }`
- [x] `ModType` enum: `Flat`, `Percent`
- [x] `StatType` enum đủ tất cả stat có thể modify: `POT, SPI, WIS, VIT, AGI, FirePot, WaterPot, IcePot, LightningPot, FireRes, WaterRes, IceRes, LightningRes, MaxHp, MaxMp, MpRecovery, AllPot, AllRes`
- [x] Struct serialize được trong Unity Inspector (`[Serializable]`)
- [x] **Edit Mode Test**: tạo vài `StatModifier`, đọc lại field đúng giá trị

---

### TASK-006 — Spell data type system: `PotencyRef`, `TargetResolver`, `SpellEffect` subtypes _(Cập nhật v3)_

**Complexity:** M | **Depends:** TASK-001

Implement toàn bộ hệ thống data type cho spell trong `Logic` assembly. Không có MonoBehaviour, không có Unity-specific code.

**AC — `IEntity` interface:**

- [x] `IEntity` interface nằm trong `Logic` assembly: `CurrentHp`, `MaxHp`, `HasEffect(EffectType)`, `GetEffectivePotency(Element)`, `GetEffectiveResistance(Element)`, `GetEffectiveAGI()`
- [ ] Property mở rộng (`ArmorStack`, `EffectSystem`) bổ sung khi implement TASK-020, TASK-030

**AC — `EffectType` enum:**

- [x] `EffectType` enum nằm trong `Logic` assembly, đủ value: `None, Burn, Drenched, Chilled, Dazed, Enrage, Refreshing, Fortified, Energized, Overdrive, Crystalize, Frozen, Regen, Distracted`

**AC — `CombatState` class:**

- [x] `CombatState` nằm trong `Logic` assembly: `{ IEntity Player, IEntity[] AliveEnemies }`
- [x] Tạo mới mỗi lượt, read-only snapshot

**AC — `PotencyRef` struct:**

- [x] `PotencyRef` nằm trong `Logic` assembly: `{ Element element, float coefficient }`
- [x] `[Serializable]` — serialize được trong Inspector
- [x] **Edit Mode Test**: `coefficient = 0.8`, `GetValue(potency: 50f)` → `40f`

**AC — `TargetResolver` struct:**

- [x] `TargetResolveType` enum đủ 6 value: `Caster`, `SelectedEnemy`, `AllEnemies`, `RandomEnemies`, `LowestHpEnemies`, `SecondaryRandom`
- [x] `TieBreaker` enum: `None`, `LowestResistance`
- [x] `TargetResolver` struct: `{ TargetResolveType type, int targetCount, TieBreaker tieBreaker, Element tieBreakerElement }`
- [x] `[Serializable]` — serialize được
- [x] `TargetResolver.Resolve(SpellCastContext, CombatState) → IEntity[]`:
    - `Caster` → `[ context.Caster ]`
    - `SelectedEnemy` → `[ context.SelectedEnemy ]`
    - `AllEnemies` → tất cả alive enemies
    - `RandomEnemies(n)` → n enemy ngẫu nhiên, không trùng
    - `LowestHpEnemies(n)` → n enemy HP thấp nhất; tie → sort theo resistance theo `tieBreakerElement`
    - `SecondaryRandom` → 1 enemy ngẫu nhiên **ngoại trừ** `context.SelectedEnemy`
- [x] **Edit Mode Test**:
    - `AllEnemies` với 3 enemies → trả về cả 3
    - `RandomEnemies(2)` với 4 enemies → trả về đúng 2, không trùng
    - `LowestHpEnemies(2)` với tie → tie-break theo resistance đúng
    - `SecondaryRandom` → không bao giờ trả về `context.SelectedEnemy`

**AC — `SpellEffect` subtypes:**

- [x] Abstract `SpellEffect` base class: `{ TargetResolver targetResolver, SpellCondition? condition }`
- [x] 4 concrete subtype, đều `[Serializable]`:
    - `DamageEffect : SpellEffect { PotencyRef potencyRef }`
    - `HealEffect : SpellEffect { PotencyRef potencyRef }`
    - `ArmorEffect : SpellEffect { PotencyRef potencyRef, int duration }`
    - `StatusEffect : SpellEffect { EffectType effectType }`
- [x] `[SerializeReference]` trên field `SpellEffect[]` trong `SpellDefinition` — Inspector cho phép chọn subtype cụ thể

**AC — `SpellCondition` subtypes:**

- [x] Abstract `SpellCondition` base class: `bool Evaluate(IEntity target, SpellCastContext context)`
- [x] 3 implementation: `TargetHasEffect { EffectType }`, `TargetHpBelow { float threshold }`, `CasterHasEffect { EffectType }`
- [x] **Edit Mode Test**: `TargetHasEffect(Burn).Evaluate(target)` đúng khi target có/không có Burn

**AC — `SpellCastContext` class:**

- [x] `SpellCastContext { IEntity Caster, IEntity? SelectedEnemy, ISpellDefinition Spell }`
- [x] `SelectedEnemy` có thể null (spell chỉ target Caster hoặc AllEnemies)

---

### TASK-007 — `SpellDefinition` class và data Rank I

**Complexity:** M | **Depends:** TASK-006

**AC — SO Class:**

- [x] `SpellDefinition : ScriptableObject` có field: `id`, `displayName`, `rank`, `element`, `baseCost`, `baseCooldown`, `minWisdomToImprint`
- [x] `effects: SpellEffect[]` dùng `[SerializeReference]` — Inspector hiển thị dropdown chọn subtype (`DamageEffect`, `HealEffect`, `ArmorEffect`, `StatusEffect`)
- [x] Tạo thành công `SpellDefinition` với `DamageEffect` + `StatusEffect` trong Inspector, không serialize error

**AC — 12 asset Rank I:**

- [x] 12 file asset tại `Assets/Data/Spells/RankI/`, naming đúng: `SP_Fireball.asset`, `SP_Ignite.asset`...
- [x] Mỗi asset điền đúng theo bảng sau (coefficient `[TBD]` ghi placeholder):

|Asset|Effects|Ghi chú|
|---|---|---|
|`SP_Fireball`|`DamageEffect(SelectedEnemy, Fire, c)` + `StatusEffect(SelectedEnemy, Burn)`||
|`SP_Ignite`|`StatusEffect(SelectedEnemy, Burn)`|Xem GDD ghi chú về "burn damage increase"|
|`SP_CandleGhost`|`DamageEffect(LowestHpEnemies n=3, tieBreaker=LowestResistance Fire, Fire, c)`||
|`SP_Heal`|`HealEffect(Caster, Water, c)`||
|`SP_Splash`|`DamageEffect(SelectedEnemy, Water, c)` + `StatusEffect(SelectedEnemy, Drenched)`||
|`SP_Bubble`|`StatusEffect(SelectedEnemy, Frozen)`|Apply Frozen trực tiếp|
|`SP_IceShard`|`DamageEffect(SelectedEnemy, Ice, c)` + `StatusEffect(SelectedEnemy, Chilled)`||
|`SP_IceShield`|`ArmorEffect(Caster, Ice, c, duration=3)`||
|`SP_ColdBreathe`|`DamageEffect(AllEnemies, Ice, c)`||
|`SP_Shock`|`DamageEffect(SelectedEnemy, Lightning, c)` + `StatusEffect(SelectedEnemy, Dazed)`||
|`SP_Spark`|`DamageEffect(SelectedEnemy, Lightning, c)` + `DamageEffect(SecondaryRandom, Lightning, c×0.5)`|Hai DamageEffect riêng|
|`SP_Pulse`|`DamageEffect(RandomEnemies n=3, Lightning, c)`||

- [x] Không asset nào có field bỏ trống không hợp lý (coefficient TBD OK, type phải đúng)

---

### TASK-008 — Tạo `EnemyDefinition`, `EquipmentDefinition`, `RuneDefinition` schema

**Complexity:** S | **Depends:** TASK-005, TASK-007

**AC:**

- [ ] `EnemyDefinition` ScriptableObject có đủ field theo Architecture doc; `decisionPolicyConfig` serialize bằng `[SerializeReference]`
- [ ] `spells: SpellDefinition[]` (direct reference, không string ID)
- [ ] Tạo `EN_TestMinion` — `RandomPolicy`, gán `SP_Fireball` asset vào `spells` array — không bị serialize error
- [ ] `EquipmentDefinition` có field: `id`, `displayName`, `slot` (`EquipmentSlot` enum), `rank`, `modifiers: StatModifier[]`
- [ ] `RuneDefinition` có field: `id`, `displayName`, `rank`, `passiveType`, `statModifiers: StatModifier[]`, `passiveConfig` (`[SerializeReference]`)
- [ ] Tạo `EQ_FlameWand_R1` (1 flat POT), `EQ_IronRobe_R2` (flat VIT + flat MaxHp) — không serialize error
- [ ] Tạo `RU_FireEmbrace_R1` (StatModifier type, flat FirePotency) — không serialize error

---

### TASK-009 — Foundation Layer: `EventChannelSO` và `StateMachineManager`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] `Core.Foundation.Events.SinglePayloadEvent` và `TwoPayloadEvent` namespace tồn tại
- [ ] `Core.Foundation.StateMachine` namespace tồn tại với `IState`, `StateMachineManager`
- [ ] `Foundation.asmdef` không reference `Logic.asmdef` hoặc `Unity.asmdef`
- [ ] `IAsyncState` interface tồn tại — extend `IState` với `UniTask EnterAsync()`
- [ ] `StateMachineManager.SetState()` detect `IAsyncState` → gọi `EnterAsync()` thay vì `Enter()`
- [ ] Compile không lỗi toàn bộ project

---

### TASK-010 — `EventChannelSO` assets cho cross-boundary events

**Complexity:** S | **Depends:** TASK-009

**AC:**

- [ ] 5 SO asset tại `Assets/Data/Events/`: `CombatEndedSO`, `NodeEnteredSO`, `GoldChangedSO`, `PendingModifierChangedSO`, `RewardReadySO`
- [ ] Mỗi SO là concrete class extend đúng `EventChannelSO<T>` generic base
- [ ] `RaiseEvent()`, `AddListener()`, `RemoveListener()` gọi được từ code

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

- [ ] VIT = 10 → `storedMaxHp = 100`; VIT = 10 + flat MaxHp +50 → `storedMaxHp = 150`
- [ ] SPI = 10 → `storedMaxMp = 100`; `storedMpRecovery = BASE_MP_RECOVERY + 10`
- [ ] `RecalculateBaseAttributes()` thứ tự đúng: collect → main attr → potency (L0+L1+L2) → resist → HP/MP → recovery
- [ ] `maxHp` và `maxMp` alias đúng `storedMaxHp`/`storedMaxMp`
- [ ] `availableAttributePoints` khởi tạo = 0; `SpendAttributePoint(POT)` → POT tăng 1, `availableAttributePoints` giảm 1, `RecalculateBaseAttributes()` tự gọi
- [ ] `SpendAttributePoint()` khi `availableAttributePoints == 0` → return false, không thay đổi
- [ ] `SpendAttributePoint()` với stat không phải Main Attribute (VD: `FirePot`) → return false
- [ ] `HasAvailablePoints()` trả đúng theo `availableAttributePoints`
- [ ] **Edit Mode Test pass**

---

### TASK-024 — `PlayerController`: Stat Layering với equipment/rune modifier

**Complexity:** M | **Depends:** TASK-023

**AC:**

- [ ] Flat POT +8 → `POT` tăng 8; flat FirePot +15 → `storedFirePotency = L0 + 15`
- [ ] Percent FirePot +10% → `storedFirePotency = (L0 + L1) × 1.10`
- [ ] Stacking flat +15 + percent +10% → `(L0 + 15) × 1.10` (L1 trước L2)
- [ ] `AllPot` modifier → cả 4 nguyên tố tăng
- [ ] `MaxHp` flat → `storedMaxHp = L0_formula + flat`
- [ ] Unequip → về giá trị gốc
- [ ] **Edit Mode Test pass**

---

### TASK-025 — `PlayerController`: Dynamic getters (Layer 3)

**Complexity:** S | **Depends:** TASK-024

**AC:**

- [ ] Không effect: `GetEffectivePotency()` = `storedXPotency`
- [ ] Enrage: `stored × 1.15`; Drenched: `stored × 0.90`; Enrage + Drenched: `stored × 1.05`
- [ ] Overdrive: `stored × 1.50`
- [ ] `GetEffectiveAGI()`: Overdrive → `float.PositiveInfinity`; Energized × 1.30; Chilled × 0.70
- [ ] `GetEffectiveResistance()`: Fortified +15%, Dazed -15%
- [ ] L3 tách biệt hoàn toàn L0–L2
- [ ] **Edit Mode Test pass**

---

## Phase 2 — Effect System

### TASK-030 — `EffectSystem`: apply và lookup

**Complexity:** M | **Depends:** TASK-001

**AC:**

- [ ] `Apply(EffectType)` thêm vào `activeEffects` Dictionary
- [ ] `Has(EffectType)` → `true` khi active; `Remove()` → `false`
- [ ] Apply cùng loại (non-stackable): Refresh duration, `Count` không tăng
- [ ] `OnEffectApplied` và `OnEffectRemoved` C# event phát đúng lúc
- [ ] **Edit Mode Test pass**

---

### TASK-031 — `EffectSystem`: element interaction

**Complexity:** M | **Depends:** TASK-030

**AC:**

- [ ] Water > Fire: apply Burn khi có Drenched → Burn giải, Drenched giữ
- [ ] Water > Fire: apply Drenched khi có Burn → Burn giải, Drenched apply
- [ ] Cùng nguyên tố, khác loại (Enrage + Burn): Neutralize cả hai
- [ ] Cùng nguyên tố, cùng loại (Burn + Burn): Refresh, count = 1
- [ ] `OnEffectApplied` **không** phát khi Abort
- [ ] **Edit Mode Test pass**

---

### TASK-032 — `EffectSystem`: duration tick

**Complexity:** S | **Depends:** TASK-031

**AC:**

- [ ] `duration = 2` → sau 2 lần `Tick()` bị xóa; sentinel `duration = -1` → không bị xóa
- [ ] `OnEffectRemoved` phát khi expire do Tick
- [ ] **Edit Mode Test pass**

---

### TASK-033 — `CombatResolver`: CheckCombinations

**Complexity:** M | **Depends:** TASK-032

**AC:**

- [ ] Enrage + Energized → `Apply(Overdrive)`, remove Enrage + Energized; Refreshing + Fortified → `Apply(Crystalize)`, remove Refreshing + Fortified
- [ ] Drenched + Chilled → `Apply(Frozen)`, remove Drenched + Chilled; Burn + Dazed → `OnCombinationTriggered(Detonates)`, remove Burn + Dazed
- [ ] Chỉ 1 trong 2 effect → không trigger
- [ ] Idempotent: gọi nhiều lần liên tiếp → trigger chỉ 1 lần
- [ ] **Edit Mode Test pass**

---

### TASK-034 — `CombatResolver`: Detonates và Burn DoT

**Complexity:** M | **Depends:** TASK-033, TASK-021

**AC:**

- [ ] Detonates: `30% × maxHp`, ignoreResistance, ignoreArmor
- [ ] Detonates khi Crystalize flag: vẫn gây damage
- [ ] Burn DoT: `10% × maxHp` giảm bởi fire_res; = 0 khi Crystalize flag
- [ ] **Edit Mode Test pass**

---

## Phase 3 — Spell Layer

### TASK-040 — `SpellSlotManager`: slot management

**Complexity:** S | **Depends:** TASK-004

**AC:**

- [ ] Bắt đầu `openSlots = 1`; `Imprint` vào slot trống → `IsImprinted = true`
- [ ] `Imprint` khi `player.WIS < spell.minWisdomToImprint` → thất bại
- [ ] `Forget` → slot trống; `Imprint` vào slot chưa mở → thất bại
- [ ] `UnlockSlot()` → slot mới mở; số slot dựa đúng `WisdomSlotConfig`
- [ ] **Edit Mode Test pass**

---

### TASK-041 — `CooldownTracker`: cooldown logic

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] `Set(spellId, 3)` → `GetCooldown` = 3; sau 3 lần `Tick()` → 0
- [ ] `OnCooldownChanged` event phát sau mỗi `Tick()` khi cooldown > 0
- [ ] Nhiều spell tracking độc lập
- [ ] **Edit Mode Test pass**

---

### TASK-042 — `SpellCaster`: validation pipeline và `SpellCastContext` _(Cập nhật v3)_

**Complexity:** M | **Depends:** TASK-040, TASK-041, TASK-025, TASK-006

**AC:**

- [ ] Không trong slot → `CastResult.SpellNotImprinted`
- [ ] Đang cooldown → `CastResult.SpellOnCooldown`
- [ ] Không đủ MP → `CastResult.NotEnoughMp`
- [ ] Không phải Action Phase → `CastResult.NotYourTurn`
- [ ] Spell có `SelectedEnemy` effect nhưng `selectedEnemy = null` → `CastResult.NoTargetSelected`
- [ ] Tất cả thỏa → `CastResult.Success`
- [ ] Distracted: `cost = base × 1.15` (làm tròn lên)
- [ ] `SpellCastContext` được tạo đúng với `Caster`, `SelectedEnemy`, `Spell`
- [ ] **Edit Mode Test pass** cho tất cả path

---

### TASK-043 — `SpellCaster`: hit/dodge theo target type _(Cập nhật v3)_

**Complexity:** M | **Depends:** TASK-042, TASK-022

**AC:**

- [ ] Spell có bất kỳ `DamageEffect { SelectedEnemy }` → check hit/dodge **1 lần duy nhất** trước khi execute effects
- [ ] Miss với SelectedEnemy → abort toàn bộ spell, MP và cooldown vẫn tiêu, `PlayMissAnim` enqueue
- [ ] `DamageEffect { AllEnemies }` → check hit/dodge **per enemy**; enemy dodge được bỏ qua, không abort toàn bộ
- [ ] `DamageEffect { RandomEnemies(n) }` → check hit/dodge per enemy; enemy dodge được skip, target pool không bị refill
- [ ] `DamageEffect { LowestHpEnemies(n) }` → check hit/dodge per enemy
- [ ] `DamageEffect { SecondaryRandom }` → **không** check hit/dodge (splash, không phải đòn riêng)
- [ ] `HealEffect`, `ArmorEffect`, `StatusEffect` → không check hit/dodge bất kể target type
- [ ] **Edit Mode Test pass** cho tất cả rule

---

### TASK-044 — `SpellCaster`: thực thi typed SpellEffect _(Thêm mới v3)_

**Complexity:** M | **Depends:** TASK-043, TASK-033, TASK-006

**AC:**

- [ ] `DamageEffect`: `raw = caster.GetEffectivePotency(potencyRef.element) × potencyRef.coefficient` → `DamageCalculator.Calculate(raw, target, element)` → armor chain → HP
- [ ] `HealEffect`: `amount = caster.GetEffectivePotency(element) × coefficient` → `target.currentHp += amount` (capped tại `StoredMaxHp`)
- [ ] `ArmorEffect`: `value = caster.GetEffectivePotency(element) × coefficient` → `target.armorStack.ApplyArmor(value, duration)`
- [ ] `StatusEffect`: `target.effectSystem.Apply(effectType)` → `CombatResolver.CheckCombinations(target)` ngay sau đó
- [ ] `StatusEffect { Frozen }` (từ Bubble): apply bình thường qua `EffectSystem`, không yêu cầu combo prerequisite
- [ ] `effect.condition != null` và `condition.Evaluate(target, context) = false` → skip effect, không execute
- [ ] Mỗi effect fire visual command tương ứng vào `VisualQueue` (xem Phase 6)
- [ ] **Edit Mode Test pass** với StubEntity cho tất cả subtype

---

### TASK-045 — `SpellCaster`: verify end-to-end với Rank I spell data _(Thêm mới v3)_

**Complexity:** S | **Depends:** TASK-044, TASK-007

**AC — Fireball (DamageEffect + StatusEffect, SelectedEnemy):**

- [ ] Cast Fireball lên enemy → enemy nhận damage đúng `potency × coefficient`, enemy có Burn
- [ ] Miss → không damage, không Burn, MP tiêu

**AC — Spark (DamageEffect SelectedEnemy + DamageEffect SecondaryRandom):**

- [ ] Cast Spark lên Enemy A khi có Enemy B → A nhận damage đúng, B nhận damage 50% của A
- [ ] B không check hit/dodge
- [ ] Chỉ có 1 enemy → `SecondaryRandom` resolve → empty list → không có secondary damage

**AC — CandleGhost (LowestHpEnemies n=3, tieBreaker LowestResistance Fire):**

- [ ] 3 enemies, 2 HP bằng nhau → cả 3 nhận damage; khi tie → chọn đúng theo fire_res thấp hơn
- [ ] Chỉ có 2 enemies → chỉ 2 target, không error

**AC — IceShield (ArmorEffect, Caster):**

- [ ] Cast IceShield → player có Armor stack mới với đúng value và duration 3

**AC — Bubble (StatusEffect Frozen, SelectedEnemy):**

- [ ] Cast Bubble lên enemy → enemy có Frozen; không yêu cầu Drenched + Chilled trước
- [ ] Enemy có Frozen → lượt tiếp theo skip Action Phase

**AC — Heal (HealEffect, Caster):**

- [ ] Cast Heal khi HP thiếu → HP tăng đúng amount
    
- [ ] HP đã đầy → HP không vượt `StoredMaxHp`
    
- [ ] **Edit Mode Test pass** cho tất cả case
    

---

## Phase 4 — Enemy AI

### TASK-050 — `CombatContext` snapshot (AI) và `SpellCastContext` resolve

**Complexity:** S | **Depends:** TASK-023, TASK-032, TASK-006

**AC:**

- [ ] `CombatContext` (AI): tạo từ PlayerController và EnemyController; `playerHpPercent`, `selfHpPercent` đúng; snapshot bất biến
- [ ] `SpellCastContext`: `Caster`, `SelectedEnemy`, `Spell` đúng khi tạo
- [ ] `TargetResolver.Resolve(SpellCastContext, CombatState)` đã test ở TASK-006 — verify integration ở đây với real entity
- [ ] **Edit Mode Test pass**

---

### TASK-051 — `SpellSelector` và `RandomPolicy`

**Complexity:** S | **Depends:** TASK-041, TASK-050

**AC:**

- [ ] `SpellSelector`: spell cooldown = 0 → available; cooldown > 0 → không
- [ ] `RandomPolicy`: trả 1 spell trong available; statistical n=1000: tất cả spell được chọn
- [ ] **Edit Mode Test pass**

---

### TASK-052 — `PriorityPolicy`, `WeightedRandomPolicy`, `ScriptedPolicy`

**Complexity:** M | **Depends:** TASK-051

**AC:**

- [ ] `PriorityPolicy`: `PlayerHas(Burn)`, `PlayerHpBelow(0.3f)` — chọn đúng theo condition
- [ ] Priority cao check trước; spell on cooldown → skip sang rule kế; không rule thỏa → fallback Random
- [ ] `WeightedRandomPolicy`: weight cao → xác suất cao (n=1000); weight 0 → không bao giờ chọn
- [ ] `ScriptedPolicy`: lần lượt sequence; spell on cooldown → skip, không block
- [ ] **Edit Mode Test pass**

---

## Phase 5 — Turn System

### TASK-060 — `TurnManager`: async combat loop với UniTask

**Complexity:** M | **Depends:** TASK-042, TASK-052

**AC:**

- [ ] `StartCombatAsync(CancellationToken ct)` là `async UniTask`, không Coroutine
- [ ] Phase order: Player (Start→Action→End) → Enemy (Start→Action→End) → lặp
- [ ] Win/Lose: `OnCombatEnded` C# event phát đúng
- [ ] `ct.Cancel()` → loop dừng sạch
- [ ] Không có `IEnumerator` hay `StartCoroutine` trong `TurnManager`
- [ ] **Play Mode Test**

---

### TASK-061 — `PhaseHandler`: Start Phase và End Phase resolve order

**Complexity:** M | **Depends:** TASK-034, TASK-060

**AC:**

- [ ] MP Recovery bước 1; Frozen check bước 2 (skip Action Phase đúng 1 lượt); Crystalize flag bật bước 3
- [ ] Regen bước 4 trước Burn bước 5; Burn bỏ qua khi Crystalize flag
- [ ] Combination check bước 7 sau tất cả status
- [ ] End Phase: effect duration -1 → xóa; armor duration -1 → xóa; spell cooldown -1 (không âm)
- [ ] **Play Mode Test**: scenario cụ thể từng bước

---

## Phase 6 — Presentation Layer (UniTask + DOTween)

### TASK-070 — `VisualQueue`: DrainAsync với UniTask

**Complexity:** M | **Depends:** TASK-002

**AC:**

- [ ] `VisualQueue` là MonoBehaviour; `DrainAsync(ct)` là `async UniTask`
- [ ] Không có `OnQueueDrained` event — caller `await DrainAsync()` trực tiếp
- [ ] `ct.Cancel()` → drain dừng sạch
- [ ] **Play Mode Test** với `WaitCommand(0.1f)`

---

### TASK-071 — `TurnManager`: await DrainAsync sau mỗi phase

**Complexity:** S | **Depends:** TASK-070, TASK-060

**AC:**

- [ ] Phase chỉ chuyển SAU khi `DrainAsync` hoàn thành; queue empty → return ngay
- [ ] **Play Mode Test**: verify timing

---

### TASK-072 — `IActionCommand` và các command cơ bản với DOTween

**Complexity:** M | **Depends:** TASK-070, TASK-002

**AC:**

- [ ] `IActionCommand`: `UniTask ExecuteAsync(CancellationToken ct = default)` — không `IEnumerator`
- [ ] `ShowDamageNumberCommand`: floating text, DOTween fade — `await` xong mới return
- [ ] `ShowHpGainCommand`: màu xanh, DOTween fade
- [ ] `PlayDamageAnimCommand`: `DOShakePosition.ToUniTask(ct)`
- [ ] `PlayDeathAnimCommand`: `DOScale(0).ToUniTask(ct)` → deactivate
- [ ] `PlayEffectApplyAnimCommand`: Instantiate VFX, await duration, Destroy
- [ ] `PlayArmorAnimCommand`: VFX shield apply
- [ ] `PlayMissAnimCommand`: miss indicator
- [ ] Tất cả: `ct` cancel → tween `.Kill()`, không orphan
- [ ] **Play Mode Test**: sequence 3 command, verify thứ tự và timing

---

### TASK-073 — `ParallelCommand`

**Complexity:** S | **Depends:** TASK-072

**AC:**

- [ ] `ParallelCommand(params IActionCommand[])` — `ExecuteAsync` dùng `UniTask.WhenAll()`
- [ ] Tất cả hoàn thành trước khi return; 1 cancel → tất cả cancel
- [ ] **Play Mode Test**: 2 WaitCommand thời gian khác nhau — kết thúc đúng lúc cái lâu hơn

---

## Phase 7 — Passive Layer

### TASK-080 — `EquipmentSystem`: stat injection với StatModifier

**Complexity:** S | **Depends:** TASK-024, TASK-008

**AC:**

- [ ] `GetAllModifiers()` → `StatModifier[]` từ tất cả equipped item
- [ ] Equip/unequip → `RecalculateBaseAttributes()` tự gọi; `AllPotency` → cả 4 nguyên tố tăng
- [ ] **Edit Mode Test pass**

---

### TASK-081 — `RuneSystem`: embed, purge, lifecycle

**Complexity:** M | **Depends:** TASK-080, TASK-032

**AC:**

- [ ] `Embed` → `OnEmbed(player)` gọi; `Purge` → `OnPurge(player)` gọi **trước** khi xóa
- [ ] `StatModifier` rune: `GetAllStatModifiers()` → vào `RecalculateBaseAttributes()`
- [ ] `ConditionalTrigger` rune: sau Purge → callback bị xóa; verify no memory leak
- [ ] `UnlockSocket()` → socket mới mở; tối đa 4 socket; giá lũy tiến theo `ShopPriceConfig`
- [ ] Bắt đầu với 0 socket; `Embed` khi không có socket trống → thất bại
- [ ] **Edit Mode Test pass**

---

## Phase 8 — Foundation: StateMachine + EventBridge

### TASK-090 — `GameManager` states và `StateMachineManager`

**Complexity:** M | **Depends:** TASK-009

**AC:**

- [ ] 6 state class implement `IState`: `InMapState`, `InCombatState`, `InShopState`, `InRestState`, `InEventState`, `InRewardState`
- [ ] `InCombatState.Enter()` gọi `TurnManager.StartCombatAsync(ct).Forget()`
- [ ] `BuildStateMachine()` setup đủ transitions; `Tick()` check và chuyển state đúng
- [ ] **Edit Mode Test**: mock predicate, verify transition

---

### TASK-091 — `GameManagerDriver` và `EventBridge` MonoBehaviours

**Complexity:** S | **Depends:** TASK-090, TASK-010, TASK-060

**AC:**

- [ ] `GameManagerDriver`: `Awake()` tạo GameManager + FSM; `Update()` gọi `Tick()`
- [ ] `CombatEndedBridge`: relay `TurnManager.OnCombatEnded` → `CombatEndedSO.RaiseEvent()`; unsubscribe trong `OnDestroy()`
- [ ] `NodeEnteredBridge`, `GoldChangedBridge`: relay tương tự; không chứa logic
- [ ] **Play Mode Test**: raise C# event → verify SO.RaiseEvent() đúng payload

---

## Phase 9 — Meta Layer

### TASK-100 — `GoldLedger`, `RewardSystem`, `ShopSystem`, `EventSystem`

**Complexity:** M | **Depends:** TASK-007, TASK-008

**AC:**

- [ ] `GoldLedger`: Earn/Spend đúng; `OnBalanceChanged` phát
- [ ] `RewardSystem.GenerateOffer(Minion, Arc1)` → 1 Equipment + 1 Spell + 1 Rune; Rank đúng xác suất (n=1000)
- [ ] `ShopSystem.GenerateInventory(Arc1)` → 5+5+5; `TryPurchase()` đúng
- [ ] `EventSystem.InitArc()` → shuffle; không lặp trong Arc; `Force Trade` → `pendingCombatModifier` set đúng
- [ ] `GiveAttributeAction { amount: 3 }` → `player.availableAttributePoints += 3`; không gọi `RecalculateBaseAttributes()` ngay
- [ ] **Edit Mode Test pass**

---

## Phase 10 — Map Layer

### TASK-110 — `MapSystem` và `NodeRouter`

**Complexity:** L | **Depends:** TASK-004

**AC:**

- [ ] `GenerateArc()` không throw với 100 seed khác nhau; tất cả path thỏa ràng buộc (DFS/BFS); mỗi Node 1–3 cạnh
- [ ] Boss Node ngoài Node cuối cùng → là Optional Boss (cùng `NodeType.Boss`, phân biệt bằng vị trí trong graph)
- [ ] Số Optional Boss trên toàn map theo ArcConfig (Arc 1: 1, Arc 2: 2, Arc 3: 3)
- [ ] Không có 2 Node cùng loại liên tiếp trên cùng một path, trừ Combat Minion Node
- [ ] `NodeRouter`: đúng node type → dispatch đúng handler; visited Node bị chặn
- [ ] **Edit Mode Test pass**

---

## Phase 11 — SaveSystem

### TASK-120 — `GameManager` state management và `SaveSystem`

**Complexity:** M | **Depends:** TASK-091, TASK-100

**AC:**

- [ ] `StartRun()` khởi tạo player đúng; `pendingCombatModifier` cleared sau combat
- [ ] `Save()` tạo JSON đủ fields; `Load()` khôi phục đúng; map giống hệt theo `mapSeed`
- [ ] Save không xảy ra trong combat
- [ ] **Edit Mode Test pass**

---

## Phase 12 — UI Layer

### TASK-130 — `PlayerStatusView`, `EnemyStatusView`, `EffectView`

**Complexity:** S | **Depends:** TASK-023, TASK-070

**AC:**

- [ ] HP/MP bar animate DOTween khi event phát; không reference trực tiếp Controller
- [ ] `EffectView`: Apply Burn → icon xuất hiện; expire → biến mất; Neutralize → cả hai mất đồng thời
- [ ] Tất cả View `OnDestroy()` unsubscribe — verify invocation count về 0
- [ ] **Play Mode Test**

---

### TASK-131 — `SpellBarView` và `TurnIndicatorView`

**Complexity:** M | **Depends:** TASK-041, TASK-060, TASK-044

**AC:**

- [ ] Cooldown overlay đúng; ngoài Action Phase → button disable
- [ ] Tap spell → `SpellCaster.TryCast()` → `CastResult`:
    - `Success` → spell animation bắt đầu
    - `NotEnoughMp` → button shake DOTween
    - `NoTargetSelected` → highlight target selection prompt
- [ ] `TurnIndicatorView`: subscribe `OnPhaseChanged` → cập nhật đúng Player/Enemy, Start/Action/End
- [ ] **Play Mode Test**

---

### TASK-132 — `MapGraphView` và `PendingModifierView`

**Complexity:** M | **Depends:** TASK-110, TASK-091

**AC:**

- [ ] Node đúng icon; visited → dim; chỉ node kề phía trước tap được
- [ ] `PendingModifierView`: subscribe `PendingModifierChangedSO` → xuất hiện/ẩn đúng
- [ ] **Play Mode Test**

---

## Phase 13 — Integration

### TASK-140 — Full combat loop: Player vs 1 Minion

**Complexity:** L | **Depends:** TASK-071, TASK-072, TASK-131

**AC:**

- [ ] Player cast Fireball: enemy nhận damage đúng, Burn icon xuất hiện, floating number animate
- [ ] Enemy turn: cast, player nhận damage, animation chạy
- [ ] Minion HP = 0 → death anim → Reward screen → `CombatEndedSO.RaiseEvent(Win)`
- [ ] Player HP = 0 → `CombatEndedSO.RaiseEvent(Lose)`
- [ ] Không có null reference, không orphan tween
- [ ] **Manual test**

---

### TASK-141 — Full combat loop: Multi-target spells

**Complexity:** M | **Depends:** TASK-140

**AC — ColdBreathe (AllEnemies):**

- [ ] Tất cả enemy nhận damage; mỗi enemy có dodge check độc lập; enemy dodge không abort spell

**AC — Spark (SelectedEnemy + SecondaryRandom):**

- [ ] Enemy A nhận full damage, Enemy B nhận 50%; B không có dodge check
- [ ] Miss Enemy A → cả A và B đều không nhận damage

**AC — CandleGhost (LowestHpEnemies + tieBreaker):**

- [ ] 3 enemies với 2 bằng HP → tie-break đúng theo fire_res; chỉ có 2 enemy → 2 target, không error

**AC — Bubble (Frozen trực tiếp):**

- [ ] Enemy bị Frozen không cần Drenched+Chilled trước; lượt sau skip Action Phase
    
- [ ] **Manual test**
    

---

### TASK-142 — Full combat loop: Element combos

**Complexity:** M | **Depends:** TASK-141

**AC:**

- [ ] Burn + Dazed → Detonates: 30% maxHp, bypass armor, `ParallelCommand` VFX + number
- [ ] Refreshing + Fortified → Crystalize: lượt sau miễn damage và debuff
- [ ] Enrage + Energized → Overdrive: guaranteed hit, ×1.30 damage
- [ ] Drenched + Chilled → Frozen: skip Action Phase
- [ ] **Manual test**

---

### TASK-143 — Full game flow: Map → Combat → Shop → Rest → Map

**Complexity:** L | **Depends:** TASK-140, TASK-132, TASK-100

**AC:**

- [ ] GameManagerDriver chuyển state đúng theo event SO
- [ ] Map → Combat → Win → Reward → Map; Map → Shop → mua → Gold giảm; Map → Rest → `availableAttributePoints += 1` → `AttributeAllocationView` hiển → player phân bổ → stat tăng
- [ ] Map → Event (Ancient Shrine) → `availableAttributePoints += 1` → allocation → đúng
- [ ] Map → Event (Cursed Altar, Accept) → HP giảm + `availableAttributePoints += 3` → allocation 3 điểm
- [ ] Không memory leak, không orphan UniTask, không orphan DOTween
- [ ] **Manual test end-to-end**

---

### TASK-144 — Save và Load giữa session

**Complexity:** M | **Depends:** TASK-143, TASK-120

**AC:**

- [ ] Stop Play Mode sau combat, Play lại → load đúng Arc, map, inventory, gold
- [ ] **Manual test**

---

## Ghi chú

- **Edit Mode Test** → `Assets/Tests/EditMode/`; **Play Mode Test** → `Assets/Tests/PlayMode/`
- Phase 0–4 ưu tiên Edit Mode Test trước khi integrate vào scene
- Task `[~]` chỉ một người nhận tại một thời điểm
- **Không dùng `StartCoroutine` hoặc `IEnumerator`** trong bất kỳ code mới nào từ Phase 5 trở đi
- **DOTween tween phải `.Kill()` khi cancel** — không để orphan tween
- Khi task xong, cập nhật status và ghi ngày hoàn thành

## Changelog

|Version|Thay đổi|
|---|---|
|v4|Thêm AC cho `availableAttributePoints` và `SpendAttributePoint()` trong TASK-023; TASK-100 thêm AC verify `GiveAttributeAction` cộng `availableAttributePoints`; TASK-143 thêm AC cho Rest và Event attribute allocation flow|
|v3|TASK-001–005 mark done; TASK-006 rewrite cho typed SpellEffect system; TASK-007 rewrite với 12 Rank I asset schema; TASK-008 merge EnemyDefinition+Equipment+Rune; TASK-042 thêm `SpellCastContext` + `NoTargetSelected`; TASK-043 rewrite hit/dodge per resolver type; TASK-044 mới (typed effect execution); TASK-045 mới (end-to-end spell verify); TASK-141 mới (multi-target spell test)|
|v2|Foundation Layer, UniTask, DOTween, EventChannelSO, StateMachine|
|v1|Initial|