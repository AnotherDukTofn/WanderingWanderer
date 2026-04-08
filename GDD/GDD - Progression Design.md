___
**Game:** Wandering Wanderer
**Author:** DukTofn
**Last Updated:** 05/04/2026
___

# 1. Document Direction

Tài liệu này mô tả cấu trúc tiến trình của một run, bao gồm cách bản đồ được tạo ra, nội dung chi tiết của từng loại Node, cơ chế Magic Shop, phần thưởng sau combat và các Event ngẫu nhiên.

Không bao gồm cơ chế combat và hệ thống attribute — xem tại _GDD — Combat Design_.

---

# 2. Map Structure

## 2.1. Tổng quan

Một run gồm **3 Arc** đi theo thứ tự tuyến tính. Mỗi Arc là một đồ thị có hướng gồm nhiều Node nối với nhau. Người chơi bắt đầu từ Node đầu tiên và chỉ có thể di chuyển sang các Node kề phía trước — **không thể quay lại**.

```
[Arc 1] → [Arc 2] → [Arc 3]
```

## 2.2. Cấu trúc mỗi Arc

Mỗi Arc có tổng số Node **cố định** trên map nhưng **layout ngẫu nhiên** mỗi run. Người chơi chỉ đi qua một tập con các Node đó tùy đường chọn — phần lớn Node trên map sẽ không được visit trong một run, tạo replayability qua nhiều run.

||Arc 1|Arc 2|Arc 3|
|---|---|---|---|
|**Tổng số Node trên map**|50|75|100|
|**Node cuối (bắt buộc)**|Boss|Boss|Boss|
|**Số Node tối thiểu để đến Node cuối**|15|20|25|
|**Số Elite Node tối thiểu trên đường**|1|2|2|
|**Số Rest Node tối thiểu trên đường**|2|2|3|
|**Số Shop Node tối thiểu trên đường**|2|2|3|
|**Số Event Node tối thiểu trên đường**|2|3|3|
|**Số Optional Boss trên toàn map**|1|2|3|

> Số Node tối thiểu áp dụng cho **đường ngắn nhất có thể** — thuật toán sinh map phải đảm bảo mọi đường dẫn đến Boss cuối đều thoả mãn ràng buộc này.

## 2.3. Phân bổ Node trong Arc

Số Node mỗi loại cố định, layout ngẫu nhiên mỗi run.---
**Game:** Wandering Wanderer
**Author:** DukTofn
**Last Updated:** 08/04/2026
---

# GDD — Architecture

## Mục lục

1. [Tổng quan kiến trúc](#1-tổng-quan-kiến-trúc)
2. [Entry Point](#2-entry-point)
3. [Map Layer](#3-map-layer)
4. [Combat Layer](#4-combat-layer)
5. [Entity Layer](#5-entity-layer)
6. [Spell Layer](#6-spell-layer)
7. [Passive Layer](#7-passive-layer)
8. [Meta Layer](#8-meta-layer)
9. [Data / Config Layer](#9-data--config-layer)
10. [Luồng dữ liệu chính](#10-luồng-dữ-liệu-chính)
11. [Ghi chú triển khai](#11-ghi-chú-triển-khai)

---

## 1. Tổng quan kiến trúc

Game được chia thành **8 layer** theo trách nhiệm. Các layer cấp cao phụ thuộc vào layer cấp thấp hơn — không có dependency ngược chiều.

```
[Entry Point]
    └── [Map Layer]
            ├── [Combat Layer]
            │       ├── [Entity Layer]
            │       └── [Spell Layer]
            │               └── [Passive Layer]
            └── [Meta Layer]

[Data / Config Layer]  ← được tham chiếu bởi mọi layer, không phụ thuộc vào ai
```

Nguyên tắc thiết kế:
- **ScriptableObject-driven config:** Mọi hằng số balancing đều nằm trong ScriptableObject, không hard-code trong logic.
- **Data tách khỏi behavior:** `EnemyDefinitions`, `SpellDefinitions`, `RuneDefinitions` là pure data — không chứa logic xử lý.
- **Effect là trung tâm combat:** Mọi tương tác giữa Spell, Rune, Equipment và Enemy đều đi qua `EffectSystem`.

---

## 2. Entry Point

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

| Dữ liệu | Mô tả |
|---|---|
| `currentArc` | Arc hiện tại (1, 2, 3) |
| `playerSnapshot` | Toàn bộ trạng thái player (attributes, inventory, equipped items, runes) |
| `gold` | Số vàng hiện có |
| `visitedNodes` | Tập các Node đã đi qua (để tránh quay lại) |
| `pendingCombatModifier` | Modifier tạm thời từ Event (Force Trade, Fading Curse) — áp dụng cho trận tiếp theo |

**Lưu ý:** `pendingCombatModifier` là nullable. Khi `CombatScene` khởi động, nó đọc giá trị này, áp dụng vào combat, rồi xóa đi sau khi trận kết thúc — tránh tạo persistent modifier system giữa các Node.

---

## 3. Map Layer

### `MapSystem`

**Trách nhiệm:** Sinh đồ thị Node cho từng Arc theo các ràng buộc thiết kế, lưu kết quả để render lên màn hình bản đồ.

**Input:** Cấu hình Arc (tổng số Node, tỷ lệ từng loại, ràng buộc đường đi tối thiểu) — đọc từ ScriptableObject.

**Output:** Một đồ thị có hướng `Graph<Node>` với các cạnh đã được xác định.

**Ràng buộc sinh map (áp dụng cho mọi đường dẫn đến Boss cuối):**

| Arc | Tổng Node | Min path | Elite tối thiểu | Rest tối thiểu | Shop tối thiểu | Event tối thiểu |
|---|---|---|---|---|---|---|
| Arc 1 | 50 | 15 | 1 | 2 | 2 | 2 |
| Arc 2 | 75 | 20 | 2 | 2 | 2 | 3 |
| Arc 3 | 100 | 25 | 2 | 3 | 3 | 3 |

**Thuật toán sinh (tham khảo — chi tiết trong Tech Design — Map Generation):**
1. Chia Node thành các "hàng" (row) theo chiều sâu.
2. Phân bổ loại Node theo tỷ lệ cấu hình vào từng hàng, đảm bảo ràng buộc tối thiểu.
3. Tạo cạnh ngẫu nhiên giữa các hàng kề nhau (mỗi Node có 1–3 cạnh ra).
4. Validate tất cả đường dẫn có thể đến Boss cuối đều thỏa ràng buộc — nếu không, regenerate.

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

## 4. Combat Layer

### `TurnManager`

**Trách nhiệm:** Điều phối vòng lặp lượt giữa Player và Enemy. Đảm bảo thứ tự Phase được thực thi đúng.

**Vòng lặp:**

```
Player Turn:
  StartPhase(Player) → ActionPhase(Player) → EndPhase(Player)

Enemy Turn:
  StartPhase(Enemy) → ActionPhase(Enemy) → EndPhase(Enemy)

→ Lặp lại cho đến khi có bên HP = 0
```

**Điều kiện kết thúc combat:**
- **Win:** Tất cả Enemy `currentHp <= 0`.
- **Lose:** Player `currentHp <= 0`.

Sau khi kết thúc, `TurnManager` báo kết quả về `GameManager`.

---

### `PhaseHandler`

**Trách nhiệm:** Thực thi từng Phase theo đúng resolve order đã định nghĩa trong GDD — Combat Design.

**Start Phase — resolve order:**

| Thứ tự | Hành động | Lý do |
|---|---|---|
| 1 | MP Recovery | Hồi mana trước mọi thứ |
| 2 | Frozen check | Nếu Frozen → set flag skip Action Phase, giải Frozen |
| 3 | Crystalize check | Nếu Crystalize active → bật flag miễn nhiễm damage + debuff |
| 4 | Regen | Hồi HP trước DoT |
| 5 | Burn (DoT) | Gây damage. Bỏ qua nếu Crystalize flag đang bật |
| 6 | Các status còn lại | Enrage, Drenched, Chilled, Dazed, Fortified, Energized... |
| 7 | Combination check | Kiểm tra Overdrive / Detonates / Frozen / Crystalize trigger |

**End Phase — resolve order:**

| Thứ tự | Hành động |
|---|---|
| 1 | Giảm duration tất cả Effect |
| 2 | Xóa Effect hết duration |
| 3 | Giảm duration Armor stack |
| 4 | Xóa Armor stack hết duration |
| 5 | Giảm cooldown tất cả Spell |

---

### `EffectSystem`

**Trách nhiệm:** Quản lý toàn bộ buff và debuff trên một entity. Xử lý apply, giải trừ, neutralize, và kích hoạt combo.

**Cấu trúc nội bộ:**

```
EffectSystem(entity) {
    activeEffects: Dictionary<EffectType, EffectInstance>
}
```

Dùng `Dictionary` thay vì `List` để check combo và lookup hiệu quả O(1).

**Khi apply một Effect mới:**

1. Kiểm tra **element interaction** với các Effect đang active:
   - Nếu Effect mới bị khắc bởi Effect cũ → Effect mới bị giải, Effect cũ giữ nguyên.
   - Nếu Effect mới khắc Effect cũ → Effect cũ bị giải, apply Effect mới.
   - Nếu cùng nguyên tố với Effect đang active → **Neutralize** cả hai.
2. Apply Effect vào `activeEffects`.
3. Gọi **Combination Check**:
   - `Enrage` + `Energized` → trigger `Overdrive`
   - `Refreshing` + `Fortified` → trigger `Crystalize`
   - `Burn` + `Dazed` → trigger `Detonates`
   - `Drenched` + `Chilled` → trigger `Frozen`

**Bảng tương khắc nguyên tố:**

```
Nước > Lửa > Băng > Sét > Nước
```

| Effect cũ (B) | Effect mới (A) | Kết quả |
|---|---|---|
| B > A | — | A bị giải, B giữ nguyên |
| A > B | — | B bị giải, A được apply |
| A và B cùng nguyên tố | — | Cả hai bị Neutralize |

**Bảng các Effect và thuộc tính:**

| Effect | Loại | Stackable | Duration | Trigger |
|---|---|---|---|---|
| Enrage | Buff | Non-self | ∞ | Dùng phép Lửa lên bản thân |
| Refreshing | Buff | Non-self | ∞ | Dùng phép Nước lên bản thân |
| Fortified | Buff | Non-self | ∞ | Dùng phép Băng lên bản thân |
| Energized | Buff | Non-self | ∞ | Dùng phép Sét lên bản thân |
| Overdrive | Buff | Non-self | This turn | Enrage + Energized |
| Crystalize | Buff | Non-self | Next turn | Refreshing + Fortified |
| Regen | Buff | Self-stackable | Depends | Spell / Item |
| Burn | Debuff | Non-self | ∞ | Trúng phép Lửa |
| Drenched | Debuff | Non-self | ∞ | Trúng phép Nước |
| Chilled | Debuff | Non-self | ∞ | Trúng phép Băng |
| Dazed | Debuff | Non-self | ∞ | Trúng phép Sét |
| Detonates | Debuff | — | Instant | Burn + Dazed |
| Frozen | Debuff | Non-self | Next turn | Drenched + Chilled |
| Distracted | Debuff | Depends | Depends | Spell / Item |

---

### `DamageCalculator`

**Trách nhiệm:** Tính toán và áp dụng sát thương cuối cùng lên target, sau khi đã xét Resistance và Armor.

**Pipeline xử lý một đòn tấn công:**

```
1. Tính raw_damage từ spell (base_value × potency)
2. Tra nguyên tố đòn tấn công → lấy resistance tương ứng của target
3. Áp dụng công thức giảm sát thương:
       damage_reduction = R / (R + 90)
       actual_damage    = raw_damage × (1 - damage_reduction)
4. Kiểm tra Crystalize flag trên target:
       nếu active → actual_damage = 0, bỏ qua bước tiếp theo
5. Gọi ArmorStack.TakeDamage(actual_damage):
       → damage đi qua chuỗi hurt_order từ cao xuống thấp
       → phần tràn sang HP cuối cùng
```

**Lưu ý — Detonates:** Gây 30% `max_hp` damage **không thể giảm bởi Resistance** và **bỏ qua Armor** (instant damage đặc biệt). `DamageCalculator` xử lý riêng trường hợp này bằng flag `ignoreResistance` và `ignoreArmor`.

---

### `HitDodgeResolver`

**Trách nhiệm:** Xác định đòn tấn công có trúng không, dựa trên AGI của hai bên.

**Công thức:**

```
hit_delta = AGI(attacker) - AGI(defender)

if hit_delta >= HIT_THRESHOLD:
    → guaranteed hit

else:
    dodge_chance = clamp(BASE_DODGE × (1 - hit_delta / HIT_THRESHOLD), 0, MAX_DODGE)
    → roll random, nếu < dodge_chance thì miss
```

Các hằng số `HIT_THRESHOLD`, `BASE_DODGE`, `MAX_DODGE` đọc từ ScriptableObject.

**Lưu ý Overdrive:** Khi entity có Overdrive active → `AGI = ∞` trong công thức, đảm bảo `hit_delta >= HIT_THRESHOLD` luôn đúng.

---

## 5. Entity Layer

### `PlayerController`

**Trách nhiệm:** Lưu trữ và cung cấp toàn bộ trạng thái hiện tại của player trong combat.

**Dữ liệu quản lý:**

```
PlayerController {
    // Main Attributes (base + bonus từ equipment)
    POT, SPI, WIS, VIT, AGI : int

    // Sub Attributes (tính từ main attributes + modifiers từ Rune/Equipment)
    fire_potency, water_potency, ice_potency, lightning_potency : float
    fire_res, water_res, ice_res, lightning_res                 : float

    // Resources
    currentHp, maxHp   : int
    currentMp, maxMp   : int

    // Combat state
    effectSystem       : EffectSystem
    armorStack         : ArmorStack
    spellSlots         : SpellSlotManager
    equippedItems      : Equipment[5]
    embeddedRunes      : Rune[4]
}
```

Sub attribute được tính lại mỗi khi có thay đổi equipment hoặc rune (không tính real-time trong combat để tránh overhead).

**Công thức HP:**
```
max_hp = 1000 × VIT / (VIT + 90)
```

**Công thức MP:**
```
max_mp           = 10 × SPI
turn_mp_recovery = BASE_MP_RECOVERY + SPI
```

---

### `EnemyController`

**Trách nhiệm:** Lưu trạng thái của một enemy instance trong combat và thực thi behavior pattern.

**Dữ liệu quản lý:**

```
EnemyController {
    // Định nghĩa từ EnemyDefinition (immutable trong combat)
    maxHp              : int
    fire_potency, water_potency, ice_potency, lightning_potency : float
    fire_res, water_res, ice_res, lightning_res                 : float
    spells             : Spell[]
    behaviorPattern    : BehaviorPattern

    // Trạng thái runtime (mutable)
    currentHp          : int
    effectSystem       : EffectSystem
    armorStack         : ArmorStack
    spellCooldowns     : Dictionary<SpellID, int>
}
```

**Behavior Pattern** là interface tách biệt khỏi data — cho phép thêm pattern mới không cần sửa `EnemyDefinition`:

```
interface BehaviorPattern {
    Spell SelectNextSpell(EnemyController self, CombatContext context)
}

// Các implementation:
RandomPattern      → chọn ngẫu nhiên từ spell pool
CyclePattern       → lần lượt theo thứ tự cố định
PriorityPattern    → chọn spell có priority cao nhất thỏa điều kiện (HP threshold, debuff check...)
```

---

### `ArmorStack`

**Trách nhiệm:** Quản lý chuỗi Armor stack theo `hurt_order`, xử lý damage tràn giữa các stack.

**Cấu trúc:**

```
ArmorStack {
    stacks: SortedList<int, ArmorStackInstance>  // key = hurt_order, descending
}

ArmorStackInstance {
    value      : int   // HP còn lại của stack
    duration   : int   // Lượt còn lại
    hurt_order : int
}
```

**Khi nhận damage:**

```
TakeDamage(damage):
    remaining = damage
    for each stack in stacks (từ hurt_order cao nhất):
        if remaining <= 0: break
        absorbed = min(stack.value, remaining)
        stack.value -= absorbed
        remaining -= absorbed
        if stack.value == 0: remove stack
    return remaining  // phần damage tràn sang HP thật
```

**Khi apply Armor mới:**

```
ApplyArmor(value, duration):
    newOrder = (stacks.MaxKey ?? 0) + 1
    stacks.Add(newOrder, new ArmorStackInstance(value, duration, newOrder))
```

**End Phase tick:**

```
Tick():
    for each stack: stack.duration -= 1
    remove all stacks where duration == 0
```

---

## 6. Spell Layer

### `SpellCaster`

**Trách nhiệm:** Điều phối toàn bộ quá trình cast một Spell — từ validation MP/cooldown đến thực thi effect.

**Pipeline cast một Spell:**

```
1. Kiểm tra spell có trong SpellSlot không
2. Kiểm tra cooldown == 0
3. Tính mana_cost (base_cost × Distracted modifier nếu đang bị)
4. Kiểm tra currentMp >= mana_cost
5. Xác định target(s)
6. Gọi HitDodgeResolver → nếu miss thì dừng
7. Thực thi effect của spell:
       a. DamageCalculator nếu spell gây damage
       b. EffectSystem.Apply() nếu spell apply buff/debuff
       c. Heal / Armor apply trực tiếp lên PlayerController
8. Trừ MP
9. Set cooldown = spell.baseCooldown
10. Notify UI
```

---

### `SpellSlotManager`

**Trách nhiệm:** Quản lý số Spell Slot khả dụng và danh sách spell đang được imprint.

**Quy tắc:**
- Số slot mở khóa phụ thuộc vào `WIS` của player (tối đa 5 slot khi max WIS).
- Một Spell chỉ có thể được cast khi đang ở trong một slot đã mở.
- `Imprint` và `Forget` chỉ thực hiện được ngoài combat.

**Cấu trúc:**

```
SpellSlotManager {
    slots: SpellSlot[5]  // max 5 slots
    openSlots: int       // số slot đang mở khóa
}

SpellSlot {
    isUnlocked    : bool
    imprinted     : Spell?  // null nếu trống
}
```

Logic WIS threshold cho từng slot nằm trong `WisdomSlotConfig` ScriptableObject.

---

### `CooldownTracker`

**Trách nhiệm:** Theo dõi cooldown của tất cả spell đang trong slot.

**Hoạt động:**
- Khi `SpellCaster` thực thi Spell thành công → set `cooldown[spellId] = spell.baseCooldown`.
- Trong End Phase → `TurnManager` gọi `CooldownTracker.Tick()` → giảm tất cả cooldown đang > 0 xuống 1.
- `SpellCaster` check `cooldown[spellId] == 0` trước khi cho phép cast.

---

## 7. Passive Layer

### `EquipmentSystem`

**Trách nhiệm:** Quản lý 5 equipment slot, tính tổng stat bonus từ tất cả equipment đang equipped, inject vào `PlayerController`.

**5 slot:**

| Slot | Equipment | Focus Stat |
|---|---|---|
| Staff Slot | Staff | POT |
| Ring Slot | Ring | SPI |
| Book Slot | Book | WIS |
| Garb Slot | Garb | VIT |
| Boot Slot | Boots | AGI |

**Khi equip / unequip bất kỳ item nào:**
1. Recalculate tổng stat bonus từ tất cả equipment.
2. Gọi `PlayerController.RecalculateSubAttributes()` để cập nhật lại potencies và resistances.

**Rank và stat cung cấp:**

| Rank | Số stat cung cấp |
|---|---|
| Rank I | 1 stat |
| Rank II | 2 stat (hoặc ít hơn với giá trị cao hơn) |
| Rank III | 3 stat (hoặc ít hơn với giá trị cao hơn) |

---

### `RuneSystem`

**Trách nhiệm:** Quản lý Rune Socket và các passive effect của Rune đang embedded. Một số Rune hook vào `EffectSystem` để trigger passive theo điều kiện.

**Socket:**
- Người chơi bắt đầu với 0 Socket. Mở thêm tại Magic Shop (tối đa 4).
- Mỗi Socket chứa tối đa 1 Rune.
- `Embed` / `Purge` chỉ thực hiện được tại Magic Shop.

**Phân loại passive theo cách hoạt động:**

| Loại passive | Cách implement |
|---|---|
| Stat modifier đơn giản (VD: +10% fire_potency) | Inject trực tiếp vào `PlayerController.RecalculateSubAttributes()` |
| Trigger theo điều kiện combat (VD: Khi bị Burn, hồi 5% HP) | Đăng ký callback vào `EffectSystem` event |
| Modifier theo lượt (VD: Spell đầu tiên mỗi lượt miễn phí) | Hook vào `TurnManager` Start Phase |

Rune Rank III thường ảnh hưởng đến nhiều cơ chế đồng thời → có thể đăng ký nhiều hook cùng lúc.

---

## 8. Meta Layer

### `ShopSystem`

**Trách nhiệm:** Sinh inventory ngẫu nhiên cho Magic Shop và xử lý các giao dịch mua bán, dịch vụ.

**Inventory mỗi lần vào Shop:**

```
ShopInventory {
    equipmentSection : Equipment[5]   // 5 equipment ngẫu nhiên
    spellSection     : Spell[5]       // 5 spell ngẫu nhiên
    runeSection      : Rune[5]        // 5 rune ngẫu nhiên
}
```

Tỷ lệ Rank khi sinh item phụ thuộc vào Arc hiện tại — đọc từ ScriptableObject (Arc 1 ít Rank III, Arc 3 nhiều Rank III).

**Các dịch vụ:**

| Dịch vụ | Điều kiện | Tác động |
|---|---|---|
| Enlighten | Đủ WIS threshold cho slot tiếp theo | Mở thêm 1 Spell Slot |
| Embed | Có Rune + Socket trống | Gắn Rune vào Socket |
| Purge | Có Rune đang embedded | Tháo Rune khỏi Socket |
| Rune Socket | Chưa đạt max 4 Socket | Mở thêm 1 Socket (giá tăng lũy tiến) |

Giá cố định theo Rank — đọc từ `ShopPriceConfig` ScriptableObject.

---

### `RewardSystem`

**Trách nhiệm:** Sau khi player thắng combat, offer 1 Equipment + 1 Spell + 1 Rune để player chọn 1.

**Pipeline:**

```
1. Xác định loại combat (Minion / Elite / Boss Optional / Boss Bắt buộc)
2. Xác định Arc hiện tại
3. Lấy bảng tỷ lệ Rank tương ứng từ RewardRateConfig ScriptableObject
4. Roll Rank cho từng loại item (Equipment, Spell, Rune) độc lập
5. Sample ngẫu nhiên 1 item của đúng Rank từ pool tương ứng
6. Hiển thị 3 lựa chọn cho player
7. Áp dụng item được chọn vào inventory
```

**Tỷ lệ Rank reward:**

_Minion & Elite:_

| Rank | Arc 1 | Arc 2 | Arc 3 |
|---|---|---|---|
| Rank I | 55% | 40% | 25% |
| Rank II | 35% | 40% | 40% |
| Rank III | 10% | 20% | 35% |

_Boss Node:_

| Rank | Arc 1 | Arc 2 | Arc 3 |
|---|---|---|---|
| Rank I | 20% | 10% | 0% |
| Rank II | 50% | 40% | 30% |
| Rank III | 30% | 50% | 70% |

---

### `EventSystem`

**Trách nhiệm:** Chọn ngẫu nhiên Event từ pool khi player vào Event Node và thực thi kết quả.

**Quy tắc lấy Event:**
- Mỗi Arc dùng một **shuffled queue** riêng từ pool — đảm bảo không lặp trong cùng Arc (nếu đủ Event trong pool).
- Queue được shuffle lại ở đầu mỗi Arc mới.

**Phân loại Event:**

| Ký hiệu | Loại | Tỷ lệ khuyến nghị |
|---|---|---|
| (+) | Tích cực | ~50% |
| (=) | Đánh đổi | ~30% |
| (−) | Tiêu cực | ~20% |

**Xử lý Event ảnh hưởng combat** (`Force Trade`, `Fading Curse`): Ghi modifier vào `GameManager.pendingCombatModifier`. Khi combat bắt đầu, `PlayerController` đọc và áp modifier này. Sau khi combat kết thúc, `pendingCombatModifier` được xóa.

**Event pool hiện tại (10 Event):**

| Event | Loại | Mô tả |
|---|---|---|
| Windfall | (+) | Nhận Gold ngẫu nhiên |
| Ancient Shrine | (+) | +1 Main Attribute tùy chọn |
| Wandering Merchant | (+) | Mini-shop 3 item, giá giảm 25% |
| Hidden Cache | (+) | Nhận miễn phí 1 Rune ngẫu nhiên |
| Cursed Altar | (=) | Mất % HP hiện tại để nhận +3 Attribute |
| Lost Devil | (=) | Chiến với Elite, nếu thắng offer Spell Rank III |
| Force Trade | (=) | Trận tiếp theo: all_res -15%, all_potencies +15% |
| Ambush | (−) | Chiến với Minion ngay, không nhận reward nếu thắng |
| Fading Curse | (−) | Trận tiếp theo: all_res -10% |
| Thief Gang | (−) | Mất 25% Gold hiện có |

---

### `RestNode`

**Trách nhiệm:** Xử lý logic khi player chọn Rest Node.

**Hiệu ứng:**
1. Hồi toàn bộ HP (`currentHp = maxHp`).
2. Cho phép player chọn +1 điểm vào một Main Attribute tùy ý.
3. Sau khi chọn, gọi `PlayerController.RecalculateSubAttributes()`.

---

### `GoldLedger`

**Trách nhiệm:** Quản lý số Gold của player trong một run — cung cấp API thu/chi thống nhất.

```
GoldLedger {
    balance: int

    Earn(amount: int)
    Spend(amount: int) → bool    // trả false nếu không đủ tiền
    CanAfford(amount: int) → bool
}
```

Tất cả giao dịch (`ShopSystem`, `RewardSystem`, `EventSystem`) đều đi qua `GoldLedger` — không ai trực tiếp modify `GameManager.gold`.

---

## 9. Data / Config Layer

### `ScriptableObjects`

Toàn bộ hằng số balancing được tách ra thành ScriptableObject riêng biệt, nhóm theo chủ đề:

| ScriptableObject | Nội dung |
|---|---|
| `CombatConfig` | `HIT_THRESHOLD`, `BASE_DODGE`, `MAX_DODGE`, `BASE_MP_RECOVERY`, `HP_CAP`, `HP_HALF`, `MP_COEFF` |
| `ArcConfig[3]` | Tổng Node, tỷ lệ từng loại Node, tỷ lệ Rank item trong Shop, Gold reward theo loại combat |
| `ShopPriceConfig` | Giá theo Rank, giá dịch vụ Enlighten/Embed/Purge, giá Socket lũy tiến |
| `RewardRateConfig` | Bảng tỷ lệ Rank reward theo loại combat × Arc |
| `EventConfig` | Tỷ lệ từng Event, giá trị cụ thể (Gold Windfall range, HP% Cursed Altar...) |
| `WisdomSlotConfig` | WIS threshold để mở từng Spell Slot |

---

### `EnemyDefinitions`

Pool tất cả enemy trong game. Mỗi entry là pure data:

```
EnemyDefinition {
    id                 : string
    displayName        : string
    enemyType          : Minion | Elite | Boss
    maxHp              : int
    fire_potency, water_potency, ice_potency, lightning_potency : float
    fire_res, water_res, ice_res, lightning_res                 : float
    spells             : SpellID[]
    behaviorPatternType: BehaviorPatternType   // Random | Cycle | Priority
    behaviorConfig     : object               // tham số riêng của từng pattern
}
```

`EnemyController` nhận `EnemyDefinition` khi được khởi tạo, tạo instance runtime từ đó.

---

### `SpellDefinitions`

Pool tất cả Spell trong game:

```
SpellDefinition {
    id           : string
    displayName  : string
    rank         : Rank (I | II | III)
    element      : Fire | Water | Ice | Lightning
    baseCost     : int
    baseCooldown : int
    targetType   : Single | AllEnemies | Self | Random(n)
    effects      : EffectApplication[]
}

EffectApplication {
    effectType   : EffectType
    valueFormula : string     // VD: "0.8 × fire_potency" — evaluate lúc cast
    condition    : Condition? // nullable — một số spell có điều kiện kích hoạt
}
```

---

### `RuneDefinitions`

Pool tất cả Rune trong game:

```
RuneDefinition {
    id           : string
    displayName  : string
    rank         : Rank (I | II | III)
    passiveType  : StatModifier | ConditionalTrigger | TurnHook | ...
    passiveConfig: object  // tham số tùy theo passiveType
}
```

---

## 10. Luồng dữ liệu chính

### Luồng: Player cast Spell lên Enemy

```
Player chọn Spell → SpellCaster
  │
  ├── [Validate] SpellSlotManager.IsImprinted(spell)
  ├── [Validate] CooldownTracker.GetCooldown(spell) == 0
  ├── [Validate] PlayerController.currentMp >= cost (sau Distracted modifier)
  │
  ├── HitDodgeResolver.Resolve(player.AGI, enemy.AGI)
  │       └── [Miss] → abort, không trừ MP, không set cooldown
  │
  ├── DamageCalculator.Calculate(spell, player.fire_potency, enemy.fire_res)
  │       └── ArmorStack.TakeDamage(actual_damage)
  │               └── EnemyController.currentHp -= overflow
  │
  ├── EffectSystem(enemy).Apply(Burn)
  │       └── Combination Check → trigger Detonates nếu Dazed đã active
  │
  ├── PlayerController.currentMp -= cost
  └── CooldownTracker.Set(spell, spell.baseCooldown)
```

### Luồng: Kết thúc một combat Node

```
TurnManager phát hiện all enemies currentHp <= 0
  │
  ├── GameManager.pendingCombatModifier = null  // xóa modifier tạm từ Event
  ├── GoldLedger.Earn(goldReward theo ArcConfig)
  ├── RewardSystem.GenerateOffer(combatType, currentArc)
  │       └── Player chọn 1 trong 3 → PlayerController.AddToInventory(item)
  └── NodeRouter.OnNodeComplete() → trở về MapView
```

### Luồng: Player vào Event Node (Force Trade)

```
NodeRouter → EventSystem.TriggerEvent()
  │
  ├── Draw event từ shuffled queue của Arc hiện tại
  ├── Event = Force Trade
  │       └── GameManager.pendingCombatModifier = { all_res: -15%, all_potencies: +15% }
  └── Trở về MapView

Lần sau player vào Combat Node:
  ├── CombatScene đọc GameManager.pendingCombatModifier
  ├── PlayerController áp modifier trong suốt combat đó
  └── Sau combat → GameManager.pendingCombatModifier = null
```

---

## 11. Ghi chú triển khai

| Mục                                                   | Ghi chú                                                                  |
| ----------------------------------------------------- | ------------------------------------------------------------------------ |
| `EffectSystem` dùng Dictionary keyed by EffectType    | Combo check O(1) thay vì scan List                                       |
| `ArmorStack` dùng SortedList descending by hurt_order | Damage luôn lấy `.Last()`, overflow tự lan xuống                         |
| `pendingCombatModifier` là nullable                   | Không cần persistent modifier system giữa các Node                       |
| `BehaviorPattern` là interface                        | Thêm pattern mới không sửa `EnemyDefinition`                             |
| Sub-attribute recalculate theo event, không real-time | Tránh overhead trong mỗi frame combat                                    |
| Tất cả hằng số balancing trong ScriptableObject       | GD chỉnh không cần rebuild                                               |
| `GoldLedger` là single source of truth cho Gold       | Không có hệ thống nào khác modify trực tiếp                              |
| Event pool dùng shuffled queue per Arc                | Không lặp Event trong cùng Arc nếu pool đủ lớn                           |
| `Detonates` bypass cả Resistance lẫn Armor            | Cần flag riêng `ignoreResistance`, `ignoreArmor` trong DamageCalculator  |
| Crystalize flag bật ở bước 3, Burn xử lý ở bước 5     | Crystalize apply trong lượt hiện tại không bảo vệ ngay — chỉ từ lượt sau |

|Loại Node|Arc 1|Arc 2|Arc 3|
|---|---|---|---|
|Combat — Minion|[TBD]%|[TBD]%|[TBD]%|
|Combat — Elite|[TBD]%|[TBD]%|[TBD]%|
|Combat — Boss (Optional)|[TBD]%|[TBD]%|[TBD]%|
|Combat — Boss (Bắt buộc, Node cuối)|1 Node|1 Node|1 Node|
|Shop Node|[TBD]%|[TBD]%|[TBD]%|
|Rest Node|[TBD]%|[TBD]%|[TBD]%|
|Event Node|[TBD]%|[TBD]%|[TBD]%|

> **Nguyên tắc thiết kế:** Arc sau nên có tỷ lệ Combat cao hơn và ít Rest hơn để tăng áp lực dần. Tỷ lệ cụ thể cần cân chỉnh qua playtesting.

## 2.4. Branching

- Mỗi Node có thể nối tới **1–3 Node** ở hàng kế tiếp.
- Người chơi thấy loại Node phía trước trước khi chọn đường đi.
- Chi tiết thuật toán sinh layout xem tại tài liệu **Tech Design — Map Generation**.

---

# 3. Item Ranks

Tất cả các loại vật phẩm có thể nhận được (Equipment, Spell, Rune) đều chia làm **3 Rank**. Rank cao hơn đồng nghĩa với sức mạnh cao hơn và độ hiếm cao hơn.

## 3.1. Equipment

|Rank|Số chỉ số cung cấp|
|---|---|
|Rank I|1 chỉ số|
|Rank II|2 chỉ số (hoặc ít hơn nhưng giá trị cao hơn)|
|Rank III|3 chỉ số (hoặc ít hơn nhưng giá trị cao hơn)|

## 3.2. Spell

|Rank|Mô tả|
|---|---|
|Rank I|Spell cơ bản — hiệu ứng đơn giản, damage/effect thấp|
|Rank II|Spell nâng cao — hiệu ứng mạnh hơn hoặc có thêm điều kiện kích hoạt|
|Rank III|Spell hiếm — damage/effect cao nhất, thường có cơ chế đặc biệt|

## 3.3. Rune

|Rank|Mô tả|
|---|---|
|Rank I|Passive đơn giản, hiệu quả thấp|
|Rank II|Passive mạnh hơn hoặc có điều kiện kích hoạt|
|Rank III|Passive mạnh, thường ảnh hưởng đến nhiều cơ chế cùng lúc|

---

# 4. Magic Shop

## 4.1. Inventory

Mỗi lần vào Shop, hệ thống tạo ra **15 mặt hàng** chia đều thành **3 gian hàng**, mỗi gian có **5 mặt hàng** của một loại:

|Gian hàng|Nội dung|
|---|---|
|Equipment|5 Equipment ngẫu nhiên|
|Spell|5 Spell ngẫu nhiên|
|Rune|5 Rune ngẫu nhiên|

**Tỷ lệ Rank của từng loại khi xuất hiện trong Shop (theo Arc):**

|Rank|Arc 1|Arc 2|Arc 3|
|---|---|---|---|
|Rank I|[TBD]%|[TBD]%|[TBD]%|
|Rank II|[TBD]%|[TBD]%|[TBD]%|
|Rank III|[TBD]%|[TBD]%|[TBD]%|

> Tỷ lệ Rank áp dụng giống nhau cho cả Equipment, Spell và Rune trong Shop. Rank III nên hiếm ở Arc 1 và phổ biến hơn ở Arc 3.

## 4.2. Bảng giá cố định

Tất cả giá tính bằng **Gold**, cố định theo Rank (không phân biệt loại mặt hàng).

|Rank|Giá|
|---|---|
|Rank I|[TBD] G|
|Rank II|[TBD] G|
|Rank III|[TBD] G|

## 4.3. Dịch vụ

Ngoài mua đồ, Shop còn cung cấp các dịch vụ sau:

|Dịch vụ|Mô tả|Giá|
|---|---|---|
|**Enlighten**|Mở thêm 1 Spell Slot (yêu cầu đủ `WIS`)|[TBD] G|
|**Embed**|Khảm 1 Rune vào Rune Socket|[TBD] G|
|**Purge**|Tháo 1 Rune khỏi Socket|[TBD] G|
|**Rune Socket**|Mở thêm 1 Rune Socket (tối đa 4)|Xem bảng bên dưới|

**Giá mua Rune Socket (tăng lũy tiến theo số Socket đã có):**

|Socket thứ|Giá|
|---|---|
|1|[TBD] G|
|2|[TBD] G|
|3|[TBD] G|
|4|[TBD] G|

> Giá Rune Socket tăng lũy tiến để tạo quyết định đánh đổi — mở Socket hay mua đồ.

---

# 5. Combat Rewards

Sau khi thắng một trận combat, người chơi nhận được:

1. **Gold** (cố định theo loại kẻ địch)
2. **Chọn 1 trong 3 phần thưởng** — hệ thống offer đúng 1 Equipment + 1 Spell + 1 Rune, người chơi chọn 1

## 5.1. Gold Reward

|Loại combat|Gold nhận được|
|---|---|
|Minion Node|[TBD] G|
|Elite Node|[TBD] G|
|Boss Node (Optional)|[TBD] G|
|Boss Node (Bắt buộc, Node cuối Arc)|[TBD] G|

## 5.2. Item Reward

Hệ thống offer **1 Equipment + 1 Spell + 1 Rune**, người chơi chọn **đúng 1** trong 3.

**Tỷ lệ Rank của reward (theo loại combat và Arc):**

_Minion & Elite Node:_

|Rank|Arc 1|Arc 2|Arc 3|
|---|---|---|---|
|Rank I|55%|40%|25%|
|Rank II|35%|40%|40%|
|Rank III|10%|20%|35%|

_Boss Node (Optional & Bắt buộc):_

|Rank|Arc 1|Arc 2|Arc 3|
|---|---|---|---|
|Rank I|20%|10%|0%|
|Rank II|50%|40%|30%|
|Rank III|30%|50%|70%|

> Boss Node luôn drop Rank cao hơn đáng kể so với cùng Arc để xứng đáng với rủi ro.

---

# 6. Event Details

## 6.1. Cơ chế

Khi vào Event Node, hệ thống chọn **ngẫu nhiên 1 Event** từ pool. Event không lặp lại trong cùng một Arc (nếu pool đủ lớn).

**Phân loại Event:**

|Ký hiệu|Loại|Mô tả|
|---|---|---|
|(+)|Tích cực|Chỉ cho lợi ích|
|(=)|Đánh đổi|Có cả lợi và hại, người chơi quyết định|
|(−)|Tiêu cực|Chỉ gây bất lợi|

> Khuyến nghị tỷ lệ phân bổ: ~50% tích cực / ~30% đánh đổi / ~20% tiêu cực để tránh Event Node cảm giác quá an toàn.

## 6.2. Event Pool

|Event|Loại|Mô tả|Tỷ lệ|
|---|---|---|---|
|**Windfall**|(+)|Nhận một lượng Gold ngẫu nhiên (`[TBD]–[TBD] G`).|[TBD]%|
|**Ancient Shrine**|(+)|Nhận +1 điểm Main Attribute tùy chọn.|[TBD]%|
|**Wandering Merchant**|(+)|Mở một mini-shop với 3 mặt hàng ngẫu nhiên, giá giảm 25%.|[TBD]%|
|**Hidden Cache**|(+)|Nhận miễn phí 1 Rune ngẫu nhiên.|[TBD]%|
|**Cursed Altar**|(=)|Mất `[TBD]%` HP hiện tại để nhận +3 điểm Main Attribute tùy chọn.|[TBD]%|
|**Lost Devil**|(=)|Chiến đấu với 1 Elite — nếu thắng, reward chắc chắn offer Spell Rank III.|[TBD]%|
|**Force Trade**|(=)|Trong trận combat ngay tiếp theo: `all_res -= 15%`, `all_potencies += 15%`.|[TBD]%|
|**Ambush**|(−)|Phải chiến đấu ngay với 1 Minion — không nhận Combat Rewards nếu thắng.|[TBD]%|
|**Fading Curse**|(−)|Trong trận combat ngay tiếp theo: `all_res -= 10%`.|[TBD]%|
|**Thief Gang**|(−)|Mất 25% Gold hiện có.|[TBD]%|

> **Lưu ý — Event ảnh hưởng combat:** _Force Trade_ và _Fading Curse_ áp dụng modifier **chỉ cho trận combat ngay tiếp theo** (không kéo dài nhiều Node), tránh cần định nghĩa thêm persistent modifier system giữa các Node.

---

# 7. Ghi chú & Mục cần hoàn thiện

|Mục|Trạng thái|Ghi chú|
|---|---|---|
|Tỷ lệ phân bổ Node mỗi loại trong Arc|⏳ Balancing|Cần playtesting để cân thời lượng và độ khó một run|
|Thuật toán sinh layout Map|📄 Tech Design|Cần đảm bảo ràng buộc số Node tối thiểu mọi đường|
|Giá tất cả mặt hàng và dịch vụ Shop|⏳ Balancing|Cần Gold economy cơ bản trước|
|Gold Reward từng loại combat|⏳ Balancing|Liên quan trực tiếp đến Gold economy|
|Tỷ lệ Rank trong Shop và Reward|⏳ Balancing|Liên quan đến power curve của run|
|Tỷ lệ và giá trị cụ thể của từng Event|⏳ Balancing|—|
|Rank system của Spell và Rune (nội dung cụ thể)|📄 Tài liệu riêng|Spell Design / Rune Design|
|Bổ sung Event pool nếu cần|🔜 Mở rộng|Hiện có 10 Event|
|Enemy Design|📄 Tài liệu riêng|—|