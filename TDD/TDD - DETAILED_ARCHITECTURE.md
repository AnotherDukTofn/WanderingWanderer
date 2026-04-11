___
**Game:** Wandering Wanderer 
**Author:** DukTofn 
**Last Updated:** 11/04/2026 
___ 
## Mục lục

1. [Tổng quan kiến trúc](#1-tổng-quan-kiến-trúc)
2. [UI Layer](#2-ui-layer)
   - [Nguyên tắc](#nguyên-tắc)
   - [`CastResult`](#castresult)
   - [Events do Logic Layer phát](#events-do-logic-layer-phát)
   - [Views theo Scene](#views-theo-scene)
     - [CombatScene Views](#combatscene-views)
     - [MapView Views](#mapview-views)
     - [ShopScene Views](#shopscene-views)
     - [RewardScreen View](#rewardscreen-view)
     - [EventScene View](#eventscene-view)
     - [RestScene View](#restscene-view)
   - [Unsubscribe Pattern](#unsubscribe-pattern-bắt-buộc)
3. [Entry Point](#3-entry-point)
   - [`GameManager`](#gamemanager)
4. [Map Layer](#4-map-layer)
   - [`MapSystem`](#mapsystem)
   - [`NodeRouter`](#noderouter)
5. [Combat Layer](#5-combat-layer)
   - [`TurnManager`](#turnmanager)
   - [`PhaseHandler`](#phasehandler)
   - [`CombatResolver`](#combatresolver)
   - [`EffectSystem`](#effectsystem)
   - [`DamageCalculator`](#damagecalculator)
   - [`HitDodgeResolver`](#hitdodgeresolver)
6. [Presentation Layer](#6-presentation-layer)
   - [`VisualQueue`](#visualqueue)
   - [`ActionCommand`](#actioncommand)
7. [Entity Layer](#7-entity-layer)
   - [Stat Layering Model](#stat-layering-model-thêm-mới-v4)
   - [`StatModifier`](#statmodifier-thêm-mới-v4)
   - [`PlayerController`](#playercontroller)
   - [`RecalculateBaseAttributes()`](#recalculatebaseattributes--thứ-tự-tính-theo-layer)
8. [Spell Layer](#8-spell-layer)
9. [Passive Layer](#9-passive-layer)
10. [Meta Layer](#10-meta-layer)
11. [Data / Config Layer](#11-data--config-layer)
12. [Luồng dữ liệu chính](#12-luồng-dữ-liệu-chính)
13. [Ghi chú triển khai](#13-ghi-chú-triển-khai)

---

## 1. Tổng quan kiến trúc

Game được chia thành **10 layer** theo trách nhiệm. Các layer cấp cao phụ thuộc vào layer cấp thấp hơn — không có dependency ngược chiều.

```
[UI Layer]  ← subscribe event từ mọi layer bên dưới, không bị ai phụ thuộc ngược lại
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

[Data / Config Layer]  ← được tham chiếu bởi mọi layer, không phụ thuộc vào ai
```

**Nguyên tắc thiết kế:**

- **ScriptableObject-driven config:** Mọi hằng số balancing đều nằm trong ScriptableObject, không hard-code trong logic.
    
- **Data tách khỏi behavior:** `EnemyDefinitions`, `SpellDefinitions`, `RuneDefinitions` là pure data — không chứa logic xử lý.
    
- **Effect là trung tâm combat:** Mọi tương tác giữa Spell, Rune, Equipment và Enemy đều đi qua `EffectSystem`.
    
- **Logic trước, Visual sau:** Combat Layer tính toán và đẩy Command vào `VisualQueue`. Presentation Layer tiêu thụ Queue — hai bên không block nhau trực tiếp.
    
- **CombatResolver làm mediator:** Mọi tương tác yêu cầu cả `EffectSystem` lẫn `DamageCalculator` đều đi qua `CombatResolver`, tránh circular dependency.
    
- **Enemy AI là data-driven:** `DecisionPolicy` được cấu hình trong `EnemyDefinition`, không hard-code theo từng enemy.
    
- **UI Layer nằm ngoài cùng:** UI subscribe event từ Logic Layer, không bao giờ bị Logic Layer phụ thuộc ngược lại. Logic không biết UI tồn tại.
    
- **Input không validate ở UI:** UI chỉ gọi API của Logic Layer và nhận `Result`. Mọi validation nằm trong Logic.
    

---

## 2. UI Layer

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

```C#
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

## 3. Entry Point

### `GameManager`

**Trách nhiệm:** Quản lý trạng thái toàn bộ một run và điều phối việc chuyển đổi giữa các scene.

**State machine của một run:**

```folder
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

## 4. Map Layer

### `MapSystem`

**Trách nhiệm:** Sinh đồ thị Node cho từng Arc theo các ràng buộc thiết kế, lưu kết quả để render lên màn hình bản đồ.

**Input:** Cấu hình Arc (tổng số Node, tỷ lệ từng loại, ràng buộc đường đi tối thiểu) — đọc từ ScriptableObject.

**Output:** Một đồ thị có hướng `Graph<Node>` với các cạnh đã được xác định.

**Ràng buộc sinh map (áp dụng cho mọi đường dẫn đến Boss cuối):**

|Arc|Tổng Node|Min path|Elite tối thiểu|Rest tối thiểu|Shop tối thiểu|Event tối thiểu|
|---|---|---|---|---|---|---|
|Arc 1|50|15|1|2|2|2|
|Arc 2|75|20|2|2|2|3|
|Arc 3|100|25|2|3|3|3|

**Thuật toán sinh (tham khảo — chi tiết trong Tech Design — Map Generation):**

1. Chia Node thành các "hàng" (row) theo chiều sâu.
2. Phân bổ loại Node theo tỷ lệ cấu hình vào từng hàng, đảm bảo ràng buộc tối thiểu.
3. Tạo cạnh ngẫu nhiên giữa các hàng kề nhau (mỗi Node có 1–3 cạnh ra).
4. Validate tất cả đường dẫn có thể đến Boss cuối đều thỏa ràng buộc — nếu không, regenerate.

### `NodeRouter`

**Trách nhiệm:** Khi player chọn đi vào một Node, `NodeRouter` nhận loại Node và gọi đúng hệ thống tương ứng.

```flow
player chọn Node
  ├── NodeType.MINION / ELITE / BOSS  ──► khởi động CombatLayer (truyền EnemyDefinition)
  ├── NodeType.SHOP                   ──► khởi động ShopSystem
  ├── NodeType.REST                   ──► khởi động RestNode handler
  └── NodeType.EVENT                  ──► khởi động EventSystem
```

---

## 5. Combat Layer

### `TurnManager`

**Trách nhiệm:** Điều phối vòng lặp lượt giữa Player và Enemy. Chỉ chuyển Phase sau khi `VisualQueue` đã drain hết.

**Vòng lặp:**

```flow
Player Turn:
  StartPhase(Player) → [await VisualQueue]
  → ActionPhase(Player) → [await VisualQueue]
  → EndPhase(Player) → [await VisualQueue]

Enemy Turn:
  StartPhase(Enemy) → [await VisualQueue]
  → ActionPhase(Enemy) → [await VisualQueue]
  → EndPhase(Enemy) → [await VisualQueue]

→ Lặp lại cho đến khi có bên HP = 0
```

`[await VisualQueue]` là barrier đồng bộ: `TurnManager` dừng lại cho đến khi `VisualQueue` báo đã execute xong toàn bộ command đang pending. Điều này đảm bảo animation của Phase trước luôn hoàn thành trước khi Phase sau bắt đầu.

**Điều kiện kết thúc combat:**

- **Win:** Tất cả Enemy `currentHp <= 0`.
- **Lose:** Player `currentHp <= 0`.

Sau khi kết thúc, `TurnManager` báo kết quả về `GameManager`.

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
  ├── Đọc entity.EffectSystem.activeEffects (read-only)
  ├── Enrage + Energized → EffectSystem.Apply(Overdrive)
  ├── Refreshing + Fortified → EffectSystem.Apply(Crystalize)
  ├── Burn + Dazed → trigger Detonates (xử lý ở trên)
  └── Drenched + Chilled → EffectSystem.Apply(Frozen)
```

---

### `EffectSystem`

**Trách nhiệm:** Quản lý toàn bộ buff và debuff trên một entity. Không tự gọi `DamageCalculator` — mọi damage đi qua `CombatResolver`.

**Cấu trúc nội bộ:**

```pseudo
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

```pseudo
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

## 6. Presentation Layer

**Trách nhiệm:** Nhận các `ActionCommand` từ Combat Layer và thực thi chúng tuần tự — animation, VFX, floating text, sound. Báo về `TurnManager` khi Queue đã drain.

### `VisualQueue`

```
VisualQueue {
    queue    : Queue<ActionCommand>
    isPlaying: bool

    Enqueue(command: ActionCommand)
    OnQueueDrained: event Action    // TurnManager subscribe vào đây
}
```

Hoạt động: Khi `isPlaying == false` và có command mới được Enqueue → tự động bắt đầu execute. Mỗi command chạy xong mới lấy command tiếp theo. Khi Queue rỗng → phát `OnQueueDrained`.

### `ActionCommand`

Interface mà mọi command visual phải implement:

```
interface ActionCommand {
    Execute(): IEnumerator   // Unity Coroutine — chạy xong mới return
}
```

**Các command hiện tại:**

|Command|Mô tả|
|---|---|
|`PlaySpellAnim(caster, spell)`|Animation cast spell của caster|
|`PlayDamageAnim(target)`|Target nhận đòn|
|`ShowDamageNumber(target, amount)`|Floating text số damage|
|`ShowHpGain(target, amount)`|Floating text hồi HP|
|`ShowMpGain(target, amount)`|Floating text hồi MP|
|`PlayEffectApplyAnim(target, effectType)`|VFX khi buff/debuff được apply|
|`PlayEffectExpireAnim(target, effectType)`|VFX khi effect hết hạn|
|`PlayBurnAnim(target)`|VFX Burn DoT|
|`PlayFrozenThawAnim(target)`|Animation tan băng|
|`PlayCrystalizeShieldAnim(target)`|VFX khiên Crystalize|
|`PlayDetonatesAnim(target)`|VFX nổ Detonates|
|`PlayDeathAnim(target)`|Animation chết|

**Quan hệ với Combat Layer:**

Combat Layer (PhaseHandler, SpellCaster, CombatResolver) không biết về animation hay timing. Nó chỉ gọi `VisualQueue.Enqueue(command)`. `TurnManager` await `OnQueueDrained` trước khi chuyển Phase.

---

## 7. Entity Layer

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
    modType: ModType   // Flat | PercentAdd
    value  : float
}

enum ModType {
    Flat,        // Cộng thẳng vào Layer 1
    PercentAdd   // Cộng vào Layer 2 (% additive)
}
```

**`StatType` enum — toàn bộ stat có thể modify:**

```
enum StatType {
    // Main Attributes
    POT, SPI, WIS, VIT, AGI,

    // Sub Attributes — Potency (flat)
    FirePotency, WaterPotency, IcePotency, LightningPotency,

    // Sub Attributes — Resistance (flat)
    FireRes, WaterRes, IceRes, LightningRes,

    // Resources (flat)
    MaxHp, MaxMp,

    // Derived
    MpRecovery,       // flat bonus cộng thêm vào turn_mp_recovery
    AllPotency,       // shorthand: apply cho cả 4 nguyên tố cùng lúc
    AllResistance     // shorthand: apply cho cả 4 nguyên tố cùng lúc
}
```

**Ví dụ Equipment Rank III:**

```
Staff of Wildfire (Rank III):
  modifiers: [
    { stat: POT,         modType: Flat,       value: 8  },
    { stat: FirePotency, modType: Flat,       value: 15 },
    { stat: FirePotency, modType: PercentAdd, value: 0.10 }  // +10%
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
       + sum(flat AllPotency modifiers)
    L2 = sum(percent potency modifiers cho element này)
       + sum(percent AllPotency modifiers)
    storedXPotency = (L0 + L1) × (1 + L2)

  // Bước 4: Tính stored resistance
  for each element in [Fire, Water, Ice, Lightning]:
    L0 = res_scale × VIT
    L1 = sum(flat res modifiers cho element này)
       + sum(flat AllResistance modifiers)
    L2 = sum(percent res modifiers cho element này)
       + sum(percent AllResistance modifiers)
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
    if effectSystem.Has(Overdrive): multiplier += 0.15
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

## 8. Spell Layer

### `SpellCaster`

**Trách nhiệm:** Điều phối toàn bộ quá trình cast một Spell — validate, resolve, thực thi, đẩy visual command.

**Pipeline cast Spell (Player):**

```
1. SpellSlotManager.IsImprinted(spell) → abort nếu false
2. CooldownTracker.GetCooldown(spell) == 0 → abort nếu còn CD
3. cost = spell.baseCost × DistractedMultiplier(player.effectSystem)
4. player.currentMp >= cost → abort nếu không đủ
5. HitDodgeResolver.Resolve(player.GetEffectiveAGI(), enemy.GetEffectiveAGI())
   → Miss: abort. VisualQueue.Enqueue(PlayMissAnim)
6. VisualQueue.Enqueue(PlaySpellAnim(player, spell))
7. Thực thi effects của spell:
   a. Nếu có damage → DamageCalculator → VisualQueue.Enqueue(ShowDamageNumber)
   b. Nếu apply effect → EffectSystem.Apply() → CombatResolver.CheckCombinations()
   c. Nếu heal → player.currentHp += → VisualQueue.Enqueue(ShowHpGain)
   d. Nếu apply Armor → ArmorStack.ApplyArmor() → VisualQueue.Enqueue(PlayArmorAnim)
8. player.currentMp -= cost
9. CooldownTracker.Set(spell, spell.baseCooldown)
```

**Pipeline cast Spell (Enemy):**

```
1. SpellSelector.GetAvailableSpells(enemy, context)
2. DecisionPolicy.SelectSpell(available, context) → spellId
3. Thực thi spell (tương tự Player từ bước 5 trở đi, nhưng không có hit/dodge với single player)
4. CooldownTracker.Set(spell, spell.baseCooldown)
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

enum ImprintResult {
    Success,
    SlotLocked,           // slot chưa mở (WisdomSlotConfig)
    SlotOccupied,         // ô đã có spell — Imprint cần ô trống hoặc flow Forget trước
    NotEnoughWisdom       // player.WIS < spell.minWisdomToImprint (GDD — Spells)
}
```

`Imprint` / `Forget` chỉ thực hiện ngoài combat.

- **Mở slot:** `openSlots` / `isUnlocked` theo WIS và `WisdomSlotConfig` (đồng bộ với dịch vụ `Enlighten` tại Shop).
- **Imprint:** ngoài slot đã mở và ô trống, bắt buộc `player.WIS >= spellDefinition.minWisdomToImprint` (field trên `SpellDefinition` — xem GDD _Spells_). `Forget` không kiểm tra WIS.

---

### `CooldownTracker`

**Trách nhiệm:** Theo dõi cooldown của spell đang trong slot (Player) hoặc spell pool (Enemy).

- Khi spell được cast thành công → `Set(spellId, baseCooldown)`.
- End Phase → `Tick()` giảm tất cả cooldown > 0 xuống 1.
- `SpellCaster` / `SpellSelector` check `GetCooldown(spellId) == 0` trước khi cho phép cast.

---

## 9. Passive Layer

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
|Rank III|3|Có thể có PercentAdd modifier|

> PercentAdd modifier chỉ xuất hiện ở Rank III để giữ Rank I và II đơn giản và dễ đọc.

---

### `RuneSystem`

**Trách nhiệm:** Quản lý Rune Socket và lifecycle của passive Rune. Rune `StatModifier` type dùng chung cơ chế `StatModifier[]` với Equipment — không có logic riêng.

**Socket:** 0 mặc định, tối đa 4, mở tại Magic Shop. `Embed`/`Purge` chỉ tại Magic Shop.

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

## 10. Meta Layer

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
|Enlighten|Đủ WIS theo **ngưỡng mở slot** tiếp theo (`WisdomSlotConfig`)|Mở thêm 1 Spell Slot (không thay đổi quy tắc **Imprint** — Imprint vẫn cần `WIS >= spell.minWisdomToImprint`)|
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

**Trách nhiệm:** Chọn ngẫu nhiên Event từ pool khi vào Event Node.

- Mỗi Arc dùng shuffled queue riêng — không lặp Event trong Arc nếu pool đủ.
- Queue shuffle lại đầu Arc mới.
- Event combat modifier (`Force Trade`, `Fading Curse`) → ghi vào `GameManager.pendingCombatModifier`.

---

### `RestNode`

Khi player chọn Rest Node:

1. `player.currentHp = player.maxHp`
2. Player chọn +1 điểm vào Main Attribute tùy ý
3. `PlayerController.RecalculateBaseAttributes()`

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

## 11. Data / Config Layer

### `ScriptableObjects`

| ScriptableObject   | Nội dung                                                                                               |
| ------------------ | ------------------------------------------------------------------------------------------------------ |
| `CombatConfig`     | `HIT_THRESHOLD`, `BASE_DODGE`, `MAX_DODGE`, `BASE_MP_RECOVERY`, `HP_CAP`, `HP_HALF`, `MP_COEFF`        |
| `ArcConfig[3]`     | Tổng Node, tỷ lệ từng loại Node, tỷ lệ Rank item trong Shop, Gold reward                               |
| `ShopPriceConfig`  | Giá theo Rank, giá dịch vụ Enlighten/Embed/Purge, giá Socket lũy tiến                                  |
| `RewardRateConfig` | Bảng tỷ lệ Rank reward theo combatType × Arc                                                           |
| `EventConfig`      | Tỷ lệ từng Event, giá trị cụ thể từng Event                                                            |
| `WisdomSlotConfig` | WIS threshold để **mở** từng Spell Slot (độc lập với `minWisdomToImprint` trên từng `SpellDefinition`) |

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

### `SpellDefinitions`

```
SpellDefinition {
    id                   : string
    displayName          : string
    rank                 : Rank (I | II | III)
    element              : Fire | Water | Ice | Lightning
    baseCost             : int
    baseCooldown         : int
    minWisdomToImprint   : int      // WIS tối thiểu để Imprint (GDD — Spells); cast combat không dùng field này
    targetType           : Single | AllEnemies | Self | Random(n) | LowestHp(n)
    effects              : EffectApplication[]
}

EffectApplication {
    effectType   : EffectType
    valueFormula : string     // VD: "0.8 × fire_potency"
    condition    : Condition?
}
```

---

### `EquipmentDefinition` 

```
EquipmentDefinition {
    id          : string
    displayName : string
    slot        : EquipmentSlot    // Staff | Ring | Book | Garb | Boots
    rank        : Rank (I | II | III)
    modifiers   : StatModifier[]   // tối đa 1/2/3 theo Rank; PercentAdd chỉ xuất hiện Rank III
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

// Rank III — có PercentAdd
Staff_DragonFang:
  modifiers: [
    { stat: POT,         modType: Flat,       value: 12   },
    { stat: FirePotency, modType: Flat,       value: 20   },
    { stat: FirePotency, modType: PercentAdd, value: 0.15 }
  ]
```

---

### `RuneDefinition`

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
  statModifiers: [{ stat: FirePotency, modType: Flat, value: 10 }]

// Rank II — StatModifier với AllResistance PercentAdd
Rune_MageArmor:
  passiveType: StatModifier
  statModifiers: [{ stat: AllResistance, modType: PercentAdd, value: 0.10 }]

// Rank III — ConditionalTrigger (không dùng statModifiers)
Rune_Combustion:
  passiveType: ConditionalTrigger
  passiveConfig: { trigger: OnEffectApplied(Burn), action: HealSelf(5%MaxHp) }
```

---

### `SaveSystem` 

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

## 12. Luồng dữ liệu chính

### Luồng: Player cast Spell lên Enemy

```
Player chọn Spell → SpellCaster
  ├── [Validate] slot, cooldown, mp
  ├── HitDodgeResolver → Miss: abort + Enqueue(PlayMissAnim)
  ├── VisualQueue.Enqueue(PlaySpellAnim)
  ├── DamageCalculator(caster.GetEffectivePotency, target.GetEffectiveResistance)
  │       └── ArmorStack.TakeDamage → target.currentHp -= overflow
  │       └── VisualQueue.Enqueue(ShowDamageNumber)
  ├── EffectSystem(enemy).Apply(Burn)
  │       ├── Check element interaction → Neutralize / Refresh / Apply
  │       └── phát OnEffectApplied
  ├── CombatResolver.CheckCombinations(enemy)
  │       └── Burn + Dazed → Detonates
  │               └── DamageCalculator.ApplyRawDamage(ignoreResistance, ignoreArmor)
  │               └── VisualQueue.Enqueue(PlayDetonatesAnim, ShowDamageNumber)
  ├── player.currentMp -= cost
  └── CooldownTracker.Set(spell, baseCooldown)
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

## 13. Ghi chú triển khai

|Mục|Ghi chú|
|---|---|
|`EffectSystem` dùng Dictionary keyed by EffectType|Combo check O(1), không scan List|
|`ArmorStack` dùng SortedList descending|Damage luôn lấy MaxKey, overflow tự lan xuống|
|Stat Layering: 4 layer rõ ràng|L0 formula, L1 flat, L2 percent, L3 combat effect — không trộn lẫn|
|`storedXValue` thay vì `baseXValue`|Tên mới phản ánh đúng: đã tính qua L0+L1+L2, không phải "base" thuần|
|`StatModifier` struct dùng chung|Equipment và Rune StatModifier type đều dùng cùng struct, không duplicate|
|PercentAdd chỉ ở Rank III Equipment|Rank I và II chỉ có Flat để dễ đọc và cân bằng|
|`AllPotency` và `AllResistance` shorthand|Apply cho cả 4 nguyên tố trong 1 modifier thay vì 4 modifier riêng|
|`RecalculateBaseAttributes()` có thứ tự cố định|Bước 1 (collect) → 2 (main attr) → 3 (potency) → 4 (resist) → 5 (HP/MP) → 6 (recovery)|
|`IRunePassive.OnPurge()` bắt buộc|Phải unsubscribe mọi event đã đăng ký trong `OnEmbed()`|
|`CombatResolver` là mediator duy nhất|`EffectSystem` và `DamageCalculator` không được reference nhau|
|`VisualQueue` là barrier giữa các Phase|`TurnManager` không chuyển Phase khi queue chưa drain|
|`SaveSystem` chỉ save tại Node boundary|Không save mid-combat — tránh phức tạp và exploit|
|`CombatContext` là snapshot bất biến|Tạo mới mỗi lần enemy action, không giữ reference sống|
|`DecisionPolicy` là data-driven|Hành vi enemy do GD cấu hình trong `EnemyDefinition`, không hard-code|
|`GDD — Enemies` là dependency còn thiếu|`PriorityPolicy` rule cụ thể và `ScriptedPolicy` sequence cần nội dung từ GDD đó|
|`Detonates` bypass Resistance và Armor|Flag `ignoreResistance`, `ignoreArmor` trong `ApplyRawDamage`|
|Crystalize flag bật bước 3, Burn bước 5|Crystalize apply trong lượt hiện tại không bảo vệ ngay|
|`pendingCombatModifier` cần icon trên MapView|`PendingModifierView` subscribe `GameManager.OnPendingModifierChanged`|
|Logic không reference UI|UI subscribe event một chiều — không bao giờ ngược lại|
|Input không validate ở UI|UI gọi API → nhận `CastResult` / `PurchaseResult` → phản hồi|
|Unsubscribe trong `OnDestroy()`|Bắt buộc với mọi View, tương tự `IRunePassive.OnPurge()`|
|Enemy Intent là optional feature|Cần xác nhận với GD trước khi implement `GetIntendedSpell()`|
|Chi tiết widget từng screen|Thuộc UI Design doc riêng, ngoài scope Architecture doc|