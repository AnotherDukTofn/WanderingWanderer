___
**Game:** Wandering Wanderer 
**Author:** DukTofn 
**Last Updated:** 09/04/2026
___

# TDD — Task Manager

### Convention

|Ký hiệu|Nghĩa|
|---|---|
|`[ ]`|Chưa làm|
|`[~]`|Đang làm|
|`[x]`|Hoàn thành|
|**S / M / L**|Complexity ước tính (S = vài giờ, M = 1–2 ngày, L = 3+ ngày)|
|**Depends**|Task phải hoàn thành trước mới bắt đầu được task này|

**Acceptance Criteria (AC):** Mỗi AC là một điều kiện cụ thể có thể verify bằng test hoặc quan sát trực tiếp trong Unity. Task chỉ được đánh dấu `[x]` khi **tất cả** AC đều pass.

---

## Phase 0 — Project Setup

### TASK-001 — Khởi tạo project và folder structure

**Complexity:** S | **Depends:** —

Tạo Unity project và folder structure theo đúng Naming Convention doc.

**AC:**

- [x] Unity project tạo thành công, Unity version đã được commit vào `.gitignore` và `ProjectSettings/`
- [x] Toàn bộ folder structure cấp 1 và cấp 2 tồn tại trong `Assets/` theo đúng Naming Convention doc
- [x] Assembly Definition (`.asmdef`) được tạo cho `Logic`, `Tests` tách biệt khỏi `Unity`
- [x] `Logic.asmdef` không reference `UnityEngine` — compile thành công khi chạy batch compile

---

### TASK-002 — Tạo ScriptableObject: `CombatConfig`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] `CombatConfig` SO tồn tại tại `Assets/Data/Config/CombatConfig.asset`
- [ ] Có thể chỉnh `HIT_THRESHOLD`, `BASE_DODGE`, `MAX_DODGE`, `BASE_MP_RECOVERY`, `HP_CAP`, `HP_HALF`, `MP_COEFF` trực tiếp trong Inspector mà không cần code
- [ ] Giá trị mặc định hợp lý được điền sẵn (xem Combat Design doc)

---

### TASK-003 — Tạo ScriptableObject: `ArcConfig`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] 3 asset `ArcConfig_1`, `ArcConfig_2`, `ArcConfig_3` tồn tại tại `Assets/Data/Config/`
- [ ] Mỗi asset có field: tổng Node, tỷ lệ từng NodeType (%), min path constraint, tỷ lệ Rank item trong Shop
- [ ] Inspector hiển thị đúng giá trị, không bị serialization error

---

### TASK-004 — Tạo ScriptableObject: `ShopPriceConfig`, `RewardRateConfig`, `EventConfig`, `WisdomSlotConfig`

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] 4 asset tồn tại tại `Assets/Data/Config/`
- [ ] `WisdomSlotConfig` có mảng 5 phần tử, mỗi phần tử là WIS threshold để mở slot tương ứng
- [ ] `RewardRateConfig` có bảng tỷ lệ Rank theo combatType × Arc (tổng mỗi dòng = 100%)
- [ ] Không bị serialization error trong Inspector

---

### TASK-005 — Tạo `SpellDefinition` và data Rank I

**Complexity:** M | **Depends:** TASK-001

**AC:**

- [ ] `SpellDefinition` ScriptableObject class có đủ field: `id`, `displayName`, `rank`, `element`, `baseCost`, `baseCooldown`, `targetType`, `effects[]`
- [ ] `EffectApplication` là serializable class có `effectType`, `valueFormula` (string), `condition` (nullable)
- [ ] 12 asset Rank I Spell tồn tại tại `Assets/Data/Spells/RankI/`, mỗi file theo đúng naming convention
- [ ] Mỗi asset điền đủ data theo GDD — Spells (Rank I), không có field nào bỏ trống không hợp lý

---

### TASK-006 — Tạo `EnemyDefinition` schema

**Complexity:** S | **Depends:** TASK-005

**AC:**

- [ ] `EnemyDefinition` ScriptableObject class có đủ field theo Architecture doc
- [ ] `decisionPolicyConfig` serialize được các subtype (`RandomPolicyConfig`, `WeightedRandomPolicyConfig`, `PriorityPolicyConfig`, `ScriptedPolicyConfig`) bằng `[SerializeReference]`
- [ ] Tạo 1 enemy test mẫu (`TestMinion`) với `RandomPolicy`, 1 spell — không bị serialize error

---

### TASK-007 — Tạo `RuneDefinition` schema

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] `RuneDefinition` ScriptableObject class có đủ field: `id`, `displayName`, `rank`, `passiveType`, `passiveConfig`
- [ ] `passiveConfig` serialize được bằng `[SerializeReference]`
- [ ] Tạo 1 rune test mẫu (stat modifier đơn giản) — không bị serialize error

---

## Phase 1 — Core Math (Pure Logic, No MonoBehaviour)

### TASK-010 — `ArmorStack`: implement và unit test

**Complexity:** S | **Depends:** TASK-001

Implement `ArmorStack` class thuần C# (không MonoBehaviour).

**AC:**

- [ ] `ApplyArmor(value, duration)` tạo stack mới với `hurt_order` tăng dần
- [ ] `TakeDamage(damage)` trả về đúng overflow khi damage < tổng armor
- [ ] `TakeDamage(damage)` trả về đúng overflow khi damage > tổng armor (damage tràn sang HP)
- [ ] Damage tràn đúng thứ tự từ stack `hurt_order` cao nhất xuống thấp nhất
- [ ] `Tick()` giảm duration đúng 1 mỗi lần gọi
- [ ] `Tick()` xóa stack khi `duration == 0` dù còn `value`
- [ ] Khi stack bị xóa giữa chừng, stack tiếp theo nhận đúng damage tràn
- [ ] **Edit Mode Test pass** cho tất cả case trên (xem Testing Guide)

---

### TASK-011 — `DamageCalculator`: implement và unit test

**Complexity:** S | **Depends:** TASK-010

**AC:**

- [ ] `Calculate(rawDamage, resistance)` trả về đúng `rawDamage × 90 / (resistance + 90)`
- [ ] Tại resistance = 0: actual_damage = raw_damage (không giảm)
- [ ] Tại resistance = 90: actual_damage = 50% raw_damage
- [ ] Tại resistance = 180: actual_damage ≈ 33.3% raw_damage
- [ ] `crystalizeFlag = true` → actual_damage = 0
- [ ] `ApplyRawDamage(ignoreResistance=true)` bỏ qua resistance formula
- [ ] `ApplyRawDamage(ignoreArmor=true)` trừ thẳng vào HP, bỏ qua `ArmorStack`
- [ ] **Edit Mode Test pass** cho tất cả case trên

---

### TASK-012 — `HitDodgeResolver`: implement và unit test

**Complexity:** S | **Depends:** TASK-002

**AC:**

- [ ] `hit_delta >= HIT_THRESHOLD` → luôn trả về `true` (guaranteed hit)
- [ ] `hit_delta = 0` → `dodge_chance = BASE_DODGE`
- [ ] `hit_delta < -HIT_THRESHOLD * (1 - MAX_DODGE/BASE_DODGE)` → `dodge_chance = MAX_DODGE` (capped)
- [ ] `dodge_chance` không bao giờ âm hoặc vượt `MAX_DODGE`
- [ ] Với mock random seed cố định, kết quả hit/miss là deterministic và đúng với xác suất tính toán
- [ ] **Edit Mode Test pass** cho tất cả case trên

---

### TASK-013 — `PlayerController`: HP và MP scaling

**Complexity:** S | **Depends:** TASK-002

**AC:**

- [ ] VIT = 10 → `max_hp = 100` (đúng công thức `1000 × VIT / (VIT + 90)`)
- [ ] VIT = 90 → `max_hp = 500`
- [ ] SPI = 10 → `max_mp = 100`
- [ ] `turn_mp_recovery = BASE_MP_RECOVERY + SPI`
- [ ] `RecalculateBaseAttributes()` gọi xong → `max_hp` và `max_mp` cập nhật đúng
- [ ] **Edit Mode Test pass** cho tất cả case trên

---

### TASK-014 — `PlayerController`: Dynamic getters

**Complexity:** S | **Depends:** TASK-013

**AC:**

- [ ] Không có effect: `GetEffectivePotency()` = `baseXPotency`
- [ ] Có Enrage: `GetEffectivePotency()` = `base × 1.15`
- [ ] Có Drenched: `GetEffectivePotency()` = `base × 0.90`
- [ ] Có cả Enrage + Drenched: `base × 1.05` (stacking đúng)
- [ ] Có Overdrive: `GetEffectivePotency()` = `base × 1.30` (Enrage + Overdrive)
- [ ] Không có effect: `GetEffectiveAGI()` = `AGI`
- [ ] Overdrive: `GetEffectiveAGI()` = `float.PositiveInfinity`
- [ ] `GetEffectiveResistance()` cộng Fortified, trừ Dazed đúng
- [ ] **Edit Mode Test pass**

---

## Phase 2 — Effect System

### TASK-020 — `EffectSystem`: apply và lookup

**Complexity:** M | **Depends:** TASK-001

**AC:**

- [ ] `Apply(EffectType)` thêm effect vào `activeEffects`
- [ ] `Has(EffectType)` trả về `true` khi effect đang active
- [ ] Apply effect đã có (non-stackable, cùng loại): không tạo instance mới, chỉ refresh — `activeEffects.Count` không tăng
- [ ] `Remove(EffectType)` xóa effect, `Has()` trả về `false` sau đó
- [ ] `OnEffectApplied` event phát khi apply thành công
- [ ] `OnEffectRemoved` event phát khi remove
- [ ] **Edit Mode Test pass**

---

### TASK-021 — `EffectSystem`: element interaction

**Complexity:** M | **Depends:** TASK-020

**AC:**

- [ ] Apply Burn (Fire) khi có Drenched (Water) active: Water > Fire → Burn bị giải, Drenched giữ nguyên
- [ ] Apply Drenched (Water) khi có Burn (Fire) active: Water > Fire → Burn bị giải, Drenched được apply
- [ ] Apply Burn (Fire) khi đã có Enrage (Fire Buff): cùng nguyên tố, khác loại → **Neutralize** cả hai, `activeEffects` trống
- [ ] Apply Burn (Fire) khi đã có Burn (Fire Debuff): cùng nguyên tố, cùng loại → **Refresh**, count vẫn = 1
- [ ] `OnEffectApplied` **không** phát khi Abort (bị khắc hoặc neutralize)
- [ ] **Edit Mode Test pass** cho tất cả case trên

---

### TASK-022 — `EffectSystem`: duration tick

**Complexity:** S | **Depends:** TASK-021

**AC:**

- [ ] Effect có `duration = 2`: sau 2 lần `Tick()`, effect bị xóa
- [ ] Effect có `duration = ∞` (giá trị sentinel `-1`): không bị xóa sau `Tick()`
- [ ] `OnEffectRemoved` phát khi effect expire do Tick
- [ ] Effect với `duration = 1` bị xóa sau đúng 1 lần Tick, không sớm hơn
- [ ] **Edit Mode Test pass**

---

### TASK-023 — `CombatResolver`: CheckCombinations

**Complexity:** M | **Depends:** TASK-022

**AC:**

- [ ] Enrage + Energized → `EffectSystem.Apply(Overdrive)` được gọi
- [ ] Refreshing + Fortified → `EffectSystem.Apply(Crystalize)` được gọi
- [ ] Drenched + Chilled → `EffectSystem.Apply(Frozen)` được gọi
- [ ] Burn + Dazed → `OnCombinationTriggered(Detonates)` event phát
- [ ] Chỉ 1 trong 2 effect → không trigger combo
- [ ] Mỗi combo chỉ trigger 1 lần dù `CheckCombinations` gọi nhiều lần liên tiếp (idempotent)
- [ ] **Edit Mode Test pass**

---

### TASK-024 — `CombatResolver`: Detonates và Burn DoT

**Complexity:** M | **Depends:** TASK-023, TASK-011

**AC:**

- [ ] Detonates: target nhận đúng `30% × maxHp` damage
- [ ] Detonates: damage không bị giảm bởi resistance (ignoreResistance=true)
- [ ] Detonates: damage bypass Armor stack (ignoreArmor=true), trừ thẳng vào HP
- [ ] Detonates khi có Crystalize flag: flag không block Detonates (Crystalize chỉ block damage thường)
- [ ] Burn DoT: target nhận `10% × maxHp` damage, bị giảm bởi fire_res
- [ ] Burn DoT khi Crystalize flag bật: damage = 0
- [ ] **Edit Mode Test pass**

---

## Phase 3 — Entity và Spell Layer

### TASK-030 — `SpellSlotManager`: slot management

**Complexity:** S | **Depends:** TASK-004

**AC:**

- [ ] Player bắt đầu với `openSlots = 1`
- [ ] `Imprint(spell)` vào slot trống: thành công, `IsImprinted(spell)` trả về `true`
- [ ] `Imprint(spell)` vào slot đã có spell khác: thất bại (hoặc replace tùy design)
- [ ] `Forget(spell)`: slot trở về trống, `IsImprinted` trả về `false`
- [ ] `Imprint` vào slot chưa mở khóa: thất bại
- [ ] `UnlockSlot()`: slot mới mở khóa, có thể Imprint vào đó
- [ ] Số slot mở dựa đúng theo `WisdomSlotConfig` threshold
- [ ] **Edit Mode Test pass**

---

### TASK-031 — `CooldownTracker`: cooldown logic

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] `GetCooldown(spellId)` trả về 0 cho spell chưa cast
- [ ] `Set(spellId, 3)` → `GetCooldown` trả về 3
- [ ] `Tick()` một lần → `GetCooldown` trả về 2
- [ ] Sau 3 lần `Tick()` → `GetCooldown` trả về 0
- [ ] `OnCooldownChanged` event phát sau mỗi `Tick()` khi cooldown > 0
- [ ] Nhiều spell tracking độc lập, không ảnh hưởng lẫn nhau
- [ ] **Edit Mode Test pass**

---

### TASK-032 — `SpellCaster`: validation pipeline

**Complexity:** M | **Depends:** TASK-030, TASK-031, TASK-014

**AC:**

- [ ] Spell không trong slot → `CastResult.SpellNotImprinted`
- [ ] Spell đang cooldown → `CastResult.SpellOnCooldown`
- [ ] MP không đủ → `CastResult.NotEnoughMp`
- [ ] Gọi khi không phải Action Phase → `CastResult.NotYourTurn`
- [ ] Tất cả điều kiện thỏa → `CastResult.Success`
- [ ] Khi Miss (HitDodgeResolver trả về false): MP không bị trừ, cooldown không được set
- [ ] Khi Success: MP bị trừ đúng cost, cooldown được set đúng `baseCooldown`
- [ ] Distracted effect: `mana_cost = base × 1.15`, làm tròn lên
- [ ] **Edit Mode Test pass** cho tất cả path

---

### TASK-033 — `SpellCaster`: thực thi effect theo spell data

**Complexity:** M | **Depends:** TASK-032, TASK-023

**AC:**

- [ ] Spell có damage effect: `DamageCalculator.Calculate()` được gọi với đúng potency và element
- [ ] Spell apply buff lên bản thân (target = Self): `EffectSystem(player).Apply()` được gọi
- [ ] Spell apply debuff lên enemy: `EffectSystem(enemy).Apply()` được gọi
- [ ] Spell có `targetType = AllEnemies`: tất cả enemy nhận damage/effect
- [ ] Spell có `targetType = Random(n)`: đúng n enemy ngẫu nhiên nhận effect
- [ ] Sau khi apply effect → `CombatResolver.CheckCombinations()` được gọi
- [ ] **Edit Mode Test pass** (dùng mock enemy để verify)

---

## Phase 4 — Enemy AI

### TASK-040 — `CombatContext`: tạo snapshot

**Complexity:** S | **Depends:** TASK-013, TASK-022

**AC:**

- [ ] `CombatContext` tạo được từ `PlayerController` và `EnemyController`
- [ ] `playerHpPercent`, `selfHpPercent` tính đúng
- [ ] `playerActiveEffects` là array read-only của effect đang active trên player
- [ ] Thay đổi state sau khi tạo context không ảnh hưởng giá trị trong context (true snapshot)
- [ ] **Edit Mode Test pass**

---

### TASK-041 — `SpellSelector`: lọc spell available

**Complexity:** S | **Depends:** TASK-031, TASK-040

**AC:**

- [ ] Spell có cooldown = 0 → xuất hiện trong kết quả
- [ ] Spell có cooldown > 0 → không xuất hiện
- [ ] Tất cả spell của enemy đều off cooldown → trả về đúng list đầy đủ
- [ ] Tất cả spell on cooldown → trả về list rỗng
- [ ] **Edit Mode Test pass**

---

### TASK-042 — `RandomPolicy`: implement và test

**Complexity:** S | **Depends:** TASK-041

**AC:**

- [ ] `SelectSpell(available, context)` trả về 1 spell trong `available`
- [ ] Với list 1 spell → luôn trả về spell đó
- [ ] Với list nhiều spell và nhiều lần gọi → tất cả spell đều được chọn ít nhất 1 lần (statistical test với n=1000)
- [ ] **Edit Mode Test pass**

---

### TASK-043 — `PriorityPolicy`: implement và test

**Complexity:** M | **Depends:** TASK-042

**AC:**

- [ ] Rule có `condition = PlayerHas(Burn)`: chọn spell đó khi player bị Burn, bỏ qua khi không
- [ ] Rule có `condition = PlayerHpBelow(0.3f)`: chọn khi player HP < 30%
- [ ] Rule có `condition = SelfHpBelow(0.5f)`: chọn khi enemy HP < 50%
- [ ] Rule có `condition = Always`: luôn chọn (dùng làm fallback cuối)
- [ ] Priority cao hơn được check trước, thỏa thì chọn ngay
- [ ] Không có rule nào thỏa → fallback về Random từ `available`
- [ ] Rule spell on cooldown → skip sang rule tiếp theo
- [ ] **Edit Mode Test pass** cho từng Condition type

---

### TASK-044 — `WeightedRandomPolicy` và `ScriptedPolicy`

**Complexity:** M | **Depends:** TASK-042

**AC:**

- [ ] `WeightedRandomPolicy`: spell có weight cao hơn được chọn với xác suất cao hơn (verify statistical với n=1000)
- [ ] `WeightedRandomPolicy`: spell weight = 0 không bao giờ được chọn
- [ ] `ScriptedPolicy`: gọi lần 1 → spell[0], lần 2 → spell[1], lần 3 → wrap về spell[0]
- [ ] `ScriptedPolicy`: nếu spell tiếp theo trong sequence đang on cooldown → skip sang spell kế, không block
- [ ] **Edit Mode Test pass**

---

## Phase 5 — Turn System

### TASK-050 — `TurnManager`: vòng lặp cơ bản

**Complexity:** M | **Depends:** TASK-032, TASK-044

**AC:**

- [ ] Combat bắt đầu: Player Turn trước, Enemy Turn sau
- [ ] Sau End Phase của enemy → bắt đầu Player Turn mới (lặp đúng thứ tự)
- [ ] Win condition: tất cả enemy HP ≤ 0 → combat kết thúc, `OnCombatEnded(Win)` phát
- [ ] Lose condition: player HP ≤ 0 → combat kết thúc, `OnCombatEnded(Lose)` phát
- [ ] `OnPhaseChanged` event phát khi chuyển Phase (Start/Action/End, Player/Enemy)
- [ ] **Play Mode Test** trong scene đơn giản với stub enemy và stub animation

---

### TASK-051 — `PhaseHandler`: Start Phase resolve order

**Complexity:** M | **Depends:** TASK-024, TASK-050

**AC:**

- [ ] MP Recovery xảy ra ở bước 1 (trước mọi damage)
- [ ] Frozen check ở bước 2: nếu Frozen → Action Phase bị skip trong lượt đó, Frozen bị giải
- [ ] Crystalize check ở bước 3: flag được bật trước Burn ở bước 5
- [ ] Regen (bước 4) xảy ra trước Burn (bước 5) — player hồi HP trước khi nhận DoT
- [ ] Burn (bước 5) bị bỏ qua khi Crystalize flag đang bật
- [ ] Combination check (bước 7) xảy ra sau tất cả status
- [ ] **Play Mode Test**: tạo scenario cụ thể cho từng bước, verify bằng Debug.Log order

---

### TASK-052 — `PhaseHandler`: End Phase và cooldown

**Complexity:** S | **Depends:** TASK-051

**AC:**

- [ ] Effect duration giảm 1 sau End Phase
- [ ] Effect hết duration bị xóa sau End Phase, không tồn tại sang lượt sau
- [ ] Armor stack duration giảm 1 sau End Phase
- [ ] Armor stack hết duration bị xóa sau End Phase, dù còn value
- [ ] Spell cooldown giảm 1 sau End Phase
- [ ] Spell cooldown không giảm xuống dưới 0
- [ ] **Edit Mode Test + Play Mode Test**

---

## Phase 6 — Presentation Layer

### TASK-060 — `VisualQueue`: enqueue và drain

**Complexity:** M | **Depends:** TASK-001

**AC:**

- [ ] `Enqueue(command)` khi queue rỗng → tự động bắt đầu execute ngay
- [ ] `Enqueue(command)` khi đang execute → command được thêm vào queue, thực thi sau khi command hiện tại xong
- [ ] Các command thực thi đúng thứ tự FIFO
- [ ] Khi queue drain → `OnQueueDrained` event phát
- [ ] `isPlaying = false` sau khi drain
- [ ] **Play Mode Test** với mock command có `yield return new WaitForSeconds(0.1f)`

---

### TASK-061 — `TurnManager`: await VisualQueue

**Complexity:** S | **Depends:** TASK-060, TASK-050

**AC:**

- [ ] `TurnManager` không chuyển Phase cho đến khi `OnQueueDrained` phát
- [ ] Nếu VisualQueue drain ngay (empty command): Phase chuyển trong cùng frame
- [ ] Nếu VisualQueue có animation 1 giây: Phase chỉ chuyển sau ~1 giây
- [ ] **Play Mode Test**: verify timing bằng `Time.time` log

---

### TASK-062 — Implement các `ActionCommand` cơ bản

**Complexity:** M | **Depends:** TASK-060

Implement các command không cần animation asset thật (dùng placeholder).

**AC:**

- [ ] `ShowDamageNumber`: floating text với số damage xuất hiện trên screen, tự fade sau 1 giây
- [ ] `ShowHpGain`: floating text màu xanh
- [ ] `PlayDeathAnim`: target GameObject deactivate sau delay
- [ ] Mỗi command implement đúng `IEnumerator Execute()`, yield cho đến khi animation xong
- [ ] Queue drain đúng sau khi tất cả command chạy xong
- [ ] **Play Mode Test** với sequence 3 command, verify thứ tự và timing

---

## Phase 7 — Passive Layer

### TASK-070 — `EquipmentSystem`: stat injection

**Complexity:** S | **Depends:** TASK-013

**AC:**

- [ ] Equip Staff +5 POT → `player.POT` tăng đúng 5
- [ ] Unequip Staff → `player.POT` về giá trị gốc
- [ ] Equip 2 items cùng stat → cộng dồn đúng
- [ ] Equip vào slot đã có item → item cũ bị replace, stat cập nhật đúng
- [ ] Sau equip/unequip → `RecalculateBaseAttributes()` được gọi tự động
- [ ] **Edit Mode Test pass**

---

### TASK-071 — `RuneSystem`: embed, purge, lifecycle

**Complexity:** M | **Depends:** TASK-070, TASK-022

**AC:**

- [ ] `Embed(rune, socketIndex)` → `IRunePassive.OnEmbed(player)` được gọi
- [ ] `Purge(socketIndex)` → `IRunePassive.OnPurge(player)` được gọi **trước** khi xóa rune
- [ ] Rune stat modifier: `Embed` stat tăng, `Purge` stat về bình thường
- [ ] Rune event hook: sau `Embed` → callback được đăng ký trong `EffectSystem`; sau `Purge` → callback bị xóa
- [ ] Verify no memory leak: sau `Purge`, `EffectSystem.OnEffectApplied.GetInvocationList()` không còn rune callback
- [ ] Embed vào socket đã có rune → thất bại (phải Purge trước)
- [ ] **Edit Mode Test pass** với mock rune

---

## Phase 8 — Meta Layer

### TASK-080 — `GoldLedger`: thu chi

**Complexity:** S | **Depends:** TASK-001

**AC:**

- [ ] `Earn(100)` → `balance = 100`
- [ ] `Spend(60)` khi đủ tiền → `balance = 40`, trả về `true`
- [ ] `Spend(60)` khi thiếu tiền → `balance` không đổi, trả về `false`
- [ ] `CanAfford(amount)` đúng trong mọi trường hợp
- [ ] `OnBalanceChanged` event phát sau mỗi Earn/Spend thành công
- [ ] **Edit Mode Test pass**

---

### TASK-081 — `RewardSystem`: generate offer

**Complexity:** S | **Depends:** TASK-005, TASK-007, TASK-080

**AC:**

- [ ] `GenerateOffer(Minion, Arc1)` → trả về 1 Equipment + 1 Spell + 1 Rune
- [ ] Rank của từng item phân bổ đúng xác suất theo `RewardRateConfig` (verify statistical n=1000)
- [ ] Boss Node: tỷ lệ Rank III cao hơn Minion Node rõ rệt
- [ ] `SelectReward(0)` → player nhận Equipment, không nhận Spell và Rune
- [ ] **Edit Mode Test pass**

---

### TASK-082 — `ShopSystem`: sinh inventory

**Complexity:** S | **Depends:** TASK-081

**AC:**

- [ ] `GenerateInventory(Arc1)` → đúng 5 Equipment + 5 Spell + 5 Rune
- [ ] Rank phân bổ đúng theo `ArcConfig` tỷ lệ Shop
- [ ] `TryPurchase(itemId)` khi đủ tiền → `GoldLedger.Spend()` được gọi, item vào inventory
- [ ] `TryPurchase(itemId)` khi không đủ tiền → `PurchaseResult.NotEnoughGold`
- [ ] Enlighten khi không đủ WIS → `PurchaseResult.ServiceUnavailable`
- [ ] **Edit Mode Test pass**

---

### TASK-083 — `EventSystem`: draw và resolve

**Complexity:** M | **Depends:** TASK-082

**AC:**

- [ ] `InitArc(arcNumber)` → shuffle queue từ pool
- [ ] Gọi `DrawEvent()` liên tiếp: không event nào lặp lại trong cùng Arc (verify với toàn bộ pool)
- [ ] Sang Arc mới → queue được shuffle lại, có thể nhận event đã nhận ở Arc trước
- [ ] `Force Trade` → `GameManager.pendingCombatModifier` được set đúng `{ all_res: -0.15, all_potencies: +0.15 }`
- [ ] `Thief Gang` → `GoldLedger.Spend(balance × 0.25f)` được gọi
- [ ] **Edit Mode Test pass**

---

## Phase 9 — Map Layer

### TASK-090 — `MapSystem`: sinh graph hợp lệ

**Complexity:** L | **Depends:** TASK-003

**AC:**

- [ ] `GenerateArc(arcConfig)` không throw exception
- [ ] Graph có đúng số Node tổng theo config
- [ ] Node cuối là Boss
- [ ] Tất cả đường đi từ Node đầu đến Node cuối đều có ít nhất: Elite tối thiểu, Rest tối thiểu, Shop tối thiểu, Event tối thiểu (theo config từng Arc)
- [ ] Mỗi Node có 1–3 cạnh ra (không có Node bị cô lập)
- [ ] Verify bằng DFS/BFS tất cả path — 100% pass với 100 lần generate khác nhau (random seed)
- [ ] **Edit Mode Test pass** (không cần visual)

---

### TASK-091 — `NodeRouter`: điều hướng đúng

**Complexity:** S | **Depends:** TASK-090, TASK-050, TASK-082, TASK-083

**AC:**

- [ ] Enter Minion Node → `TurnManager.StartCombat(minionEnemy)` được gọi
- [ ] Enter Shop Node → `ShopSystem.Open()` được gọi
- [ ] Enter Rest Node → `RestNode.Apply()` được gọi
- [ ] Enter Event Node → `EventSystem.DrawEvent()` được gọi
- [ ] Enter đã visited Node → bị chặn (không thể đi lại)
- [ ] **Edit Mode Test pass** với mock handlers

---

## Phase 10 — GameManager và SaveSystem

### TASK-100 — `GameManager`: state management

**Complexity:** M | **Depends:** TASK-091, TASK-080

**AC:**

- [ ] `StartRun()` khởi tạo player với 10 điểm mỗi attribute, inventory trống
- [ ] `currentArc` đúng ở mỗi Arc
- [ ] `pendingCombatModifier` được clear sau combat kết thúc
- [ ] Player snapshot serialize/deserialize đúng (không mất data)
- [ ] **Edit Mode Test pass**

---

### TASK-101 — `SaveSystem`: checkpoint save và load

**Complexity:** M | **Depends:** TASK-100

**AC:**

- [ ] `Save()` tạo file JSON tại đường dẫn hợp lệ
- [ ] File chứa đủ: `currentArc`, `mapSeed`, `visitedNodeIds[]`, `playerSnapshot`, `gold`, `pendingCombatModifier`
- [ ] `Load()` đọc file, khôi phục đúng state trong `GameManager`
- [ ] Simulate thoát game giữa Arc 2: load lại → map giống hệt (dùng cùng `mapSeed`), player state đúng
- [ ] Save không xảy ra trong combat (verify bằng `Assert.IsFalse(SaveSystem.isSaving)` khi ở giữa TurnManager loop)
- [ ] **Edit Mode Test pass**

---

## Phase 11 — UI Layer

### TASK-110 — `PlayerStatusView`: HP và MP bar

**Complexity:** S | **Depends:** TASK-013, TASK-060

**AC:**

- [ ] HP bar fill = `currentHp / maxHp` sau mỗi `OnHpChanged` event
- [ ] MP bar fill = `currentMp / maxMp` sau mỗi `OnMpChanged` event
- [ ] HP giảm: bar animate dần xuống (không snap ngay)
- [ ] View không có reference trực tiếp đến `PlayerController` (chỉ subscribe event)
- [ ] `OnDestroy()` unsubscribe tất cả event — verify bằng `GetInvocationList().Length` giảm về 0
- [ ] **Play Mode Test**

---

### TASK-111 — `EffectView`: hiển thị buff/debuff icon

**Complexity:** S | **Depends:** TASK-022

**AC:**

- [ ] Apply Burn → icon Burn xuất hiện
- [ ] Burn expire (Tick đến 0) → icon Burn biến mất
- [ ] Neutralize → cả hai icon biến mất đồng thời
- [ ] Combo trigger Overdrive → icon Overdrive xuất hiện
- [ ] **Play Mode Test**

---

### TASK-112 — `SpellBarView`: cast và feedback

**Complexity:** M | **Depends:** TASK-032, TASK-110

**AC:**

- [ ] Spell đang cooldown: button hiển thị countdown số, không thể tap
- [ ] Không phải Action Phase: tất cả button disable
- [ ] Tap spell khi đủ điều kiện → `SpellCaster.TryCast()` được gọi
- [ ] `CastResult.NotEnoughMp` → button shake animation
- [ ] `CastResult.Success` → spell animation bắt đầu (visual queue)
- [ ] Cooldown hiển thị cập nhật realtime theo `CooldownTracker.OnCooldownChanged`
- [ ] **Play Mode Test**

---

### TASK-113 — `MapGraphView`: hiển thị và navigation

**Complexity:** M | **Depends:** TASK-091

**AC:**

- [ ] Mỗi Node hiển thị đúng icon theo NodeType (Minion/Elite/Boss/Shop/Rest/Event)
- [ ] Node đã visited hiển thị khác (dim/greyed out)
- [ ] Chỉ Node kề phía trước của node hiện tại mới có thể tap
- [ ] `PendingModifierView` xuất hiện khi `pendingCombatModifier != null`, biến mất khi null
- [ ] Tap Node → `NodeRouter.EnterNode(nodeId)` được gọi
- [ ] **Play Mode Test**

---

## Phase 12 — Integration

### TASK-120 — Full combat loop: Player vs 1 Minion

**Complexity:** L | **Depends:** TASK-061, TASK-062, TASK-112

**AC:**

- [ ] Bắt đầu combat: Player Turn, Action Phase, SpellBar active
- [ ] Player cast Fireball → Minion nhận damage (số đúng), animation chạy
- [ ] Enemy turn: enemy cast spell → Player nhận damage, animation chạy
- [ ] Minion HP = 0 → death animation → combat kết thúc → Reward screen
- [ ] Player HP = 0 → Game Over screen
- [ ] Toàn bộ chạy không có null reference exception hay log error
- [ ] **Manual test trong Scene đơn giản**

---

### TASK-121 — Full combat loop: Element combos

**Complexity:** M | **Depends:** TASK-120

**AC:**

- [ ] Cast Fire spell lên enemy → enemy nhận Burn icon
- [ ] Cast Lightning spell lên cùng enemy đang có Burn → Detonates trigger: 30% maxHp damage instant, bypass armor
- [ ] Cast Water spell lên bản thân → Refreshing icon
- [ ] Cast Ice spell lên bản thân khi đang có Refreshing → Crystalize icon, Refreshing biến mất
- [ ] Lượt tiếp theo: enemy cast damage → player nhận 0 damage (Crystalize active), sau đó Crystalize expire
- [ ] Cast Fire + Lightning lên bản thân → Overdrive: lượt đó guaranteed hit, damage × 1.30
- [ ] **Manual test**

---

### TASK-122 — Full run: Map → Combat → Shop → Map

**Complexity:** L | **Depends:** TASK-120, TASK-113, TASK-082

**AC:**

- [ ] Vào Map Node → Map hiển thị đúng
- [ ] Chọn Minion Node → combat bắt đầu
- [ ] Thắng combat → Reward screen → chọn item → Map
- [ ] Chọn Shop Node → Shop mở → mua item → Gold giảm → Map
- [ ] Node đã đi bị mark, không vào lại được
- [ ] Không có scene loading error, không có memory allocation spike bất thường
- [ ] **Manual test end-to-end**

---

### TASK-123 — Save và Load giữa session

**Complexity:** M | **Depends:** TASK-122, TASK-101

**AC:**

- [ ] Sau khi thắng 1 combat, quit game (Application.Quit trong editor: Stop play mode)
- [ ] Play lại → game load đúng state: đúng Arc, đúng map, đúng inventory, đúng gold
- [ ] Player không mất item đã nhận trước khi quit
- [ ] **Manual test**

---

## Ghi chú

- Tất cả **Edit Mode Test** viết trong `Assets/Tests/EditMode/`
- Tất cả **Play Mode Test** viết trong `Assets/Tests/PlayMode/`
- Task Phase 0–4 ưu tiên viết Edit Mode Test trước khi integrate vào scene
- Task `[~]` chỉ một người nhận tại một thời điểm
- Khi một task xong, người thực hiện cập nhật status và ghi ngày hoàn thành vào cột bên cạnh ID