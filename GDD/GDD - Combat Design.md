___
**Game:** Wandering Wanderer
**Author:** DukTofn
**Last Updated:** 05/04/2026
___

# 1. Document Direction

Tài liệu này cung cấp định nghĩa và cách hoạt động chi tiết của các thành phần, cơ chế và tương tác trong combat.

Không bao gồm các yếu tố về tiến trình game hoặc tiến trình sức mạnh của người chơi ngoài combat.

---

# 2. Attributes

## 2.1. Main Attributes

|Attribute|Viết tắt|Tác dụng trong combat|
|---|---|---|
|POTENCY|`POT`|Spell DMG + cường độ effect scaling.|
|SPIRIT|`SPI`|MP tối đa + MP Recovery scaling.|
|WISDOM|`WIS`|Yêu cầu mở Spell Slot (không ảnh hưởng trực tiếp trong combat).|
|VITALITY|`VIT`|HP tối đa + Resistance scaling.|
|AGILITY|`AGI`|Ảnh hưởng tỷ lệ trúng/né — xem **Section 2.2.4**.|

> Người chơi bắt đầu với **10 điểm mỗi attribute**.

## 2.2. Sub Attributes

### 2.2.1. Potencies

Chỉ số phản ánh hiệu quả của các effect theo từng nguyên tố tương ứng.

|Loại|Công thức scale gốc|
|---|---|
|Fire Potency|`fire_potency = pot_scale × POT`|
|Water Potency|`water_potency = pot_scale × POT`|
|Ice Potency|`ice_potency = pot_scale × POT`|
|Lightning Potency|`lightning_potency = pot_scale × POT`|

> `pot_scale` mặc định là 1.0. Trang bị và Rune có thể điều chỉnh hệ số này per nguyên tố.

### 2.2.2. Resistances

Chỉ số phản ánh khả năng giảm sát thương nhận vào theo từng nguyên tố.

|Loại|Công thức scale gốc|
|---|---|
|Fire Resistance|`fire_res = 1.0 × VIT`|
|Water Resistance|`water_res = 1.0 × VIT`|
|Ice Resistance|`ice_res = 1.0 × VIT`|
|Lightning Resistance|`lightning_res = 1.0 × VIT`|

**Công thức áp dụng Resistance lên damage nhận vào:**

```
damage_reduction = R / (R + 90)
actual_damage    = incoming_damage × (1 - damage_reduction)
                 = incoming_damage × 90 / (R + 90)
```

Trong đó `R` là giá trị Resistance tương ứng với nguyên tố của đòn tấn công.

**Bảng tham khảo (Player với VIT = R gốc):**

| VIT           | damage_reduction | actual_damage (từ 100 dmg) |
| ------------- | ---------------- | -------------------------- |
| 10 (khởi đầu) | 10.0%            | 90.0                       |
| 20            | 18.2%            | 81.8                       |
| 50            | 35.7%            | 64.3                       |
| 90            | 50.0%            | 50.0                       |
| 180           | 66.7%            | 33.3                       |
| ∞             | 100% (tiệm cận)  | 0                          |

> Resistance không bao giờ đạt 100% — luôn có sát thương thực tế.  
> Trang bị và Rune có thể tăng từng loại Resistance độc lập với VIT.

### 2.2.3. MP Recovery

Lượng mana hồi **thêm** mỗi lượt ngoài `BASE_MP_RECOVERY`.

```
mp_recovery = 1.0 × SPI
```

### 2.2.4. Hit & Dodge (AGI)

**Cơ chế:**  
Gọi bên tấn công là **A**, bên nhận là **B**.

```
hit_delta = AGI(A) - AGI(B)
```

|Điều kiện|Kết quả|
|---|---|
|`hit_delta >= HIT_THRESHOLD`|Tấn công **chắc chắn trúng** (dodge_chance = 0%)|
|`hit_delta < HIT_THRESHOLD`|Tính `dodge_chance` theo công thức bên dưới|

**Công thức Dodge Chance:**

```
dodge_chance = clamp(BASE_DODGE × (1 - hit_delta / HIT_THRESHOLD), 0, MAX_DODGE)
```

|Hằng số|Mô tả|
|---|---|
|`HIT_THRESHOLD`|Ngưỡng AGI chênh lệch để đảm bảo trúng 100%|
|`BASE_DODGE`|Tỷ lệ né khi `hit_delta = 0` (2 bên bằng AGI nhau)|
|`MAX_DODGE`|Tỷ lệ né tối đa — không bao giờ đạt 100%|

> Tất cả hằng số AGI được quản lý qua **Scriptable Object** để GD tiện chỉnh trong quá trình balancing.

**Ví dụ hành vi (BASE_DODGE = 30%, MAX_DODGE = 50%, HIT_THRESHOLD = 20 — placeholder):**

|hit_delta|dodge_chance|
|---|---|
|≥ 20|0% (guaranteed hit)|
|10|15%|
|0|30%|
|−10|45%|
|≤ −14|50% (capped)|

> Effect **Energized** (AGI+) và **Chilled** (AGI−) tác động trực tiếp lên AGI dùng trong công thức này.

---

# 3. HP, Armor & MP

## 3.1. HP

### Hurt Order

Thứ tự nhận sát thương được xác định bởi `hurt_order`:

- **HP** luôn có `hurt_order = 0`.
- Mỗi stack **Armor** nhận `hurt_order = current_max_hurt_order + 1` khi được apply.
- Sát thương luôn trừ vào đối tượng có `hurt_order` cao nhất trước, xuống đến HP cuối cùng.

### HP Scaling

HP tối đa scale theo `VIT` với quy luật **giảm dần**.

**Công thức:**

```
max_hp = HP_CAP × VIT / (VIT + HP_HALF)
```

|Hằng số|Giá trị|Mô tả|
|---|---|---|
|`HP_CAP`|`1000`|HP tối đa tiệm cận (VIT → ∞)|
|`HP_HALF`|`90`|VIT tại đó `max_hp = HP_CAP / 2 = 500`|

> Tại VIT = 10 (khởi đầu): `max_hp = 1000 × 10 / (10 + 90) = 100`.

**Bảng tham khảo:**

| VIT           | max_hp | HP gain/điểm VIT |
| ------------- | ------ | ---------------- |
| 10 (khởi đầu) | 100    | —                |
| 15            | 143    | +8.5             |
| 20            | 182    | +7.7             |
| 30            | 250    | +6.8             |
| 50            | 357    | +5.4             |
| 90            | 500    | +3.6             |
| 180           | 667    | +1.9             |

## 3.2. Armor

**Armor** là một lớp HP tạm thời chặn sát thương trước khi trừ vào HP thật.

### Cấu trúc một Armor stack

```
armor_stack = {
    value     : int,   // lượng HP của stack, xác định bởi spell/rune/item tạo ra nó
    duration  : int,   // số lượt còn lại
    hurt_order: int    // = current_max_hurt_order + 1 khi được apply
}
```

### Cơ chế hoạt động

- Mỗi lần Armor được apply → tạo một **stack mới** với `value` và `duration` riêng.
- Sát thương trừ vào stack có `hurt_order` cao nhất trước.
- Khi `value` về 0 → stack bị xóa, sát thương thừa tràn sang `hurt_order` kế tiếp.
- Khi `duration` về 0 → stack bị xóa bất kể còn bao nhiêu `value`.
- Duration giảm vào **End Phase** của lượt bên sở hữu Armor.

### Ví dụ

```
Tình huống:
  HP            (hurt_order=0)
  Armor Stack A (hurt_order=1, value=30, duration=2)
  Armor Stack B (hurt_order=2, value=20, duration=1)

Nhận 35 sát thương:
  → Stack B (hurt_order=2): nhận 20 dmg → value=0, bị xóa. 15 dmg tràn sang.
  → Stack A (hurt_order=1): nhận 15 dmg → value=15 còn lại.
  → HP: không bị ảnh hưởng.
```

## 3.3. MP

### Công thức

```
max_mp           = MP_COEFF × SPI
turn_mp_recovery = BASE_MP_RECOVERY + 1.0 × SPI
```

|Hằng số|Giá trị|Mô tả|
|---|---|---|
|`MP_COEFF`|`10`|Hệ số MP tối đa tuyến tính theo SPI|
|`BASE_MP_RECOVERY`|[Balancing]|Lượng MP hồi gốc mỗi lượt, không phụ thuộc SPI — quản lý qua Scriptable Object|

> MP scale **tuyến tính** theo SPI (khác với HP dùng diminishing returns).  
> Tại SPI = 10 (khởi đầu): `max_mp = 100`.

**Bảng tham khảo:**

|SPI|max_mp|turn_mp_recovery (BASE_MP_RECOVERY = 0)|
|---|---|---|
|10 (khởi đầu)|100|10|
|20|200|20|
|50|500|50|
|100|1000|100|

### Trạng thái đầu combat

- Người chơi bắt đầu combat với **MP đầy** (`current_mp = max_mp`).
- MP hồi lại vào **Start Phase** của mỗi lượt.

---

# 4. Enemy Attributes

Kẻ địch **không** dùng hệ Main Attribute của Player. Thay vào đó, mỗi enemy được định nghĩa trực tiếp bằng các giá trị sub attribute và danh sách spell.

### Cấu trúc một Enemy

```
enemy = {
    max_hp            : int,

    // Potencies
    fire_potency      : float,
    water_potency     : float,
    ice_potency       : float,
    lightning_potency : float,

    // Resistances (áp dụng cùng công thức R / (R + 90) như Player)
    fire_res          : float,
    water_res         : float,
    ice_res           : float,
    lightning_res     : float,

    // Behavior
    spells            : Spell[]   // danh sách spell được gắn sẵn
}
```

> **Số spell = số behavior** của enemy (Minion: 1–2, Elite: 2–3, Boss: 3–4).  
> Pattern hành vi (random, cycle, priority...) được định nghĩa trong tài liệu **Enemy Design** riêng.

---

# 5. Turn Logic & Structure

## 5.1. Turn Structure

Cấu trúc của 1 turn gồm 3 Phase:

```
Start Phase → Action Phase → End Phase
```

### 5.1.1. Start Phase

Phase khởi đầu của mỗi lượt. Xử lý các effect có `resolve_time = START`.

**Hồi MP:**

```
current_mp = min(current_mp + turn_mp_recovery, max_mp)
```

**Resolve Order:**

|Thứ tự|Effect / Hành động|Lý do|
|---|---|---|
|1|MP Recovery|Hồi mana trước mọi thứ để đảm bảo resource nhất quán.|
|2|Frozen check|Nếu đang bị Frozen → flag skip Action Phase, giải Frozen.|
|3|Crystalize check|Nếu Crystalize active → bật flag miễn nhiễm damage + debuff cho lượt này.|
|4|Regen|Hồi HP trước DoT — tránh chết oan khi có cả hai cùng lúc.|
|5|Burn (DoT)|Gây sát thương. Nếu Crystalize flag đang bật → bỏ qua.|
|6|Các status còn lại|Enrage, Drenched, Chilled, Dazed, Fortified, Energized...|
|7|Combination check|Kiểm tra và kích hoạt Overdrive / Detonates nếu đủ điều kiện.|

> **Lưu ý Crystalize:** Flag miễn nhiễm được bật ở bước 3 (trước Burn ở bước 5). Nếu Crystalize được _apply_ trong lượt này (chưa active vào đầu Start Phase) thì **không** có hiệu lực ngay — chỉ bảo vệ từ lượt sau.

### 5.1.2. Action Phase

Người chơi chọn các Spell để cast (hoặc Enemy thực hiện hành vi theo pattern).

- Người chơi có thể cast nhiều Spell trong một lượt miễn đủ MP.
- Nếu đang bị **Frozen**: toàn bộ Action Phase bị skip.

### 5.1.3. End Phase

Xử lý các effect có `resolve_time = END`. Giảm duration của tất cả effect đang active.

**Resolve Order:**

| Thứ tự | Effect / Hành động   | Lý do                                    |
| ------ | -------------------- | ---------------------------------------- |
| 1      | Effect duration tick | Giảm duration tất cả effect đang active. |
| 2      | Effect expiry check  | Xóa các effect về 0 duration.            |
| 3      | Armor duration tick  | Giảm duration các Armor stack.           |
| 4      | Armor expiry check   | Xóa các Armor stack về 0 duration.       |
| 5      | Spell cooldown tick  | Giảm tất cả cooldown các spell đang cd.  |

---

# 6. Effect Design

## 6.1. Buffs

|Effect|Mô tả|Điều kiện apply|Duration|
|---|---|---|---|
|**Enrage**|`all_potencies += 15%` (non-self-stackable)|Sử dụng phép Lửa lên bản thân|∞|
|**Refreshing**|`mp_recovery += 25%` (non-self-stackable)|Sử dụng phép Nước lên bản thân|∞|
|**Fortified**|`all_resistances += 15%` (non-self-stackable)|Sử dụng phép Băng lên bản thân|∞|
|**Energized**|`AGI += 30%` (non-self-stackable)|Sử dụng phép Sét lên bản thân|∞|
|**Overdrive**|`all_potencies += 15%`, `AGI = ∞` (guaranteed hit)|Enrage + Energized (any order)|This turn only|
|**Crystalize**|Miễn nhiễm damage và debuff (non-self-stackable)|Refreshing + Fortified (any order)|Next turn only|
|**Regen**|Hồi 10% `max_hp` vào Start Phase mỗi lượt (self-stackable)|Spell / Item có apply Regen|Depends|

## 6.2. Debuffs

|Effect|Mô tả|Điều kiện apply|Duration|
|---|---|---|---|
|**Burn**|Nhận 10% `max_hp` damage vào Start Phase mỗi lượt (có thể giảm bởi Resistance, non-stackable)|Bị trúng phép Lửa|∞|
|**Drenched**|`all_potencies -= 10%` (non-self-stackable)|Bị trúng phép Nước|∞|
|**Chilled**|`AGI -= 30%` (non-self-stackable)|Bị trúng phép Băng|∞|
|**Dazed**|`all_resistances -= 15%` (non-self-stackable)|Bị trúng phép Sét|∞|
|**Detonates**|Ngay lập tức nhận 30% `max_hp` damage (không thể giảm bởi Resistance)|Burn + Dazed (any order)|Instant|
|**Frozen**|Vô hiệu hóa Action Phase (non-self-stackable)|Drenched + Chilled (any order)|Next turn only|
|**Distracted**|`mana_cost += 15%`|Spell / Item có apply Distracted|Depends|

---

# 7. Ghi chú & Mục cần hoàn thiện

|Mục|Trạng thái|Ghi chú|
|---|---|---|
|Hằng số AGI (HIT_THRESHOLD, BASE_DODGE, MAX_DODGE)|⏳ Balancing|Quản lý qua Scriptable Object|
|BASE_MP_RECOVERY|⏳ Balancing|Quản lý qua Scriptable Object|
|Spell design chi tiết (bao gồm Rank I/II/III)|📄 Tài liệu riêng|—|
|Rune design chi tiết (bao gồm Rank I/II/III)|📄 Tài liệu riêng|—|
|Enemy Design (pattern, spell pool cụ thể)|📄 Tài liệu riêng|—|