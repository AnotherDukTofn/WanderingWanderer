___
**Game:** Wandering Wanderer
**Author:** DukTofn
**Last Updated:** 05/04/2026
___

# 1. Introduction

**Name:** Wandering Wanderer.  
**Genre:** Turn-Based, Strategy, Mid-core.  
**Engine:** Unity.

**Game Pitch:** Wandering Wanderer là một game mid-core với hệ thống combat theo lượt. Người chơi nhập vai vào một pháp sư trên chuyến hành trình đi về quê hương của mình ở một vùng đất xa xăm. Trong suốt cuộc hành trình, vị pháp sư phải chiến đấu với nhiều kẻ địch, thu thập các trang bị để tăng sức mạnh cho bản thân, học các phép thuật mới để phục vụ quá trình chiến đấu.

---

# 2. Gameplay

## 2.1. Game Elements

### 2.1.1. Spells

- Là công cụ chính để người chơi vượt qua các thử thách trong game.
- Khi sử dụng sẽ tiêu tốn MP.
- Chia ra làm 4 nguyên tố chính: Lửa, Nước, Băng, Sét.
- Mỗi loại phép gây ra buff và debuff riêng biệt.
- Spell chia làm **3 Rank** theo mức độ sức mạnh:

|Rank|Mô tả|
|---|---|
|Rank I|Spell cơ bản — hiệu ứng đơn giản, damage/effect thấp|
|Rank II|Spell nâng cao — hiệu ứng mạnh hơn hoặc có thêm điều kiện kích hoạt|
|Rank III|Spell hiếm — damage/effect cao nhất, thường có cơ chế đặc biệt|

### 2.1.2. Main Attributes

Chỉ số của người chơi, ảnh hưởng trực tiếp đến quá trình chiến đấu. Có 5 loại chỉ số:

|Attribute|Viết tắt|Tác dụng|
|---|---|---|
|POTENCY|`POT`|Tăng sát thương và cường độ hiệu ứng của các phép.|
|SPIRIT|`SPI`|Tăng MP tối đa và lượng MP hồi lại mỗi lượt.|
|WISDOM|`WIS`|Đáp ứng yêu cầu mở Spell Slot.|
|VITALITY|`VIT`|Tăng HP tối đa và các chỉ số kháng nguyên tố.|
|AGILITY|`AGI`|Ảnh hưởng đến tỷ lệ trúng/né khi tấn công. Xem chi tiết tại _Combat Design — Section 2.2.4_.|

> Người chơi bắt đầu với **10 điểm mỗi attribute**.

### 2.1.3. Spell Slots

- Những ô để gắn phép, chỉ các phép được gắn trong ô mới có thể sử dụng trong combat.
- Người chơi bắt đầu với 1 ô phép. Có thể mở thêm khi `Enlighten` tại Magic Shop, tuy nhiên các ô tiếp theo chỉ mở khóa khi đủ `WIS`. Tối đa 5 ô khi max `WIS`.
- Gắn phép vào ô thông qua `Imprint Spells`, gỡ ra thông qua `Forget Spells` (chỉ khả dụng ngoài combat).

### 2.1.4. MP

- Là tài nguyên tiêu hao để cast spell.
- Người chơi bắt đầu trận chiến với đầy MP. Mỗi lượt được hồi lại một lượng nhất định.
- MP tối đa và lượng hồi mỗi lượt tăng **tuyến tính** theo `SPI`.

### 2.1.5. HP

- Là tài nguyên sống còn của người chơi.
- Người chơi bắt đầu một run với đầy HP. Chỉ hồi lại khi dùng Potion hoặc Rest tại Rest Node.
- HP tối đa scale theo `VIT` với quy luật **giảm dần** — điểm `VIT` đầu tiên cho nhiều HP hơn các điểm sau.

### 2.1.6. Equipment

Trang bị của người chơi, điều chỉnh các Main Attributes (`POT`, `SPI`, `WIS`, `VIT`, `AGI`) và cung cấp passive. Trang bị chia ra 5 loại:

|Loại|Tên|Focus|
|---|---|---|
|Trượng|Staff|`POT`|
|Nhẫn|Ring|`SPI`|
|Sách|Book|`WIS`|
|Y phục|Garb|`VIT`|
|Giày|Boots|`AGI`|

Trang bị chia làm **3 Rank** theo mức độ sức mạnh:

|Rank|Mô tả|
|---|---|
|Rank I|Cung cấp 1 chỉ số|
|Rank II|Cung cấp 2 chỉ số (hoặc ít hơn nhưng giá trị cao hơn)|
|Rank III|Cung cấp 3 chỉ số (hoặc ít hơn nhưng giá trị cao hơn)|

### 2.1.7. Equipment Slots

Các ô để gắn trang bị, tương ứng 1-1 với 5 loại trang bị:

- **Staff Slot** — Ô Trượng
- **Ring Slot** — Ô Nhẫn
- **Book Slot** — Ô Sách
- **Garb Slot** — Ô Y phục
- **Boot Slot** — Ô Giày

### 2.1.8. Runes

- Là những viên đá có thể khảm vào bản thân thông qua `Embed` tại Magic Shop.
- Không tăng Main Attributes, nhưng mỗi viên cung cấp passive riêng biệt.
- Tháo Rune thông qua `Purge` tại Magic Shop.
- Có thể nhận được từ Chest, mua trong Magic Shop, hoặc drop từ Combat.
- Rune chia làm **3 Rank** theo mức độ sức mạnh của passive:

|Rank|Mô tả|
|---|---|
|Rank I|Passive đơn giản, hiệu quả thấp|
|Rank II|Passive mạnh hơn hoặc có điều kiện kích hoạt|
|Rank III|Passive mạnh, thường ảnh hưởng đến nhiều cơ chế cùng lúc|

### 2.1.9. Rune Sockets

- Các ô để khảm Runes. Mở khóa bằng cách mua vật phẩm tương ứng tại Magic Shop.
- Giới hạn tối đa 4 Sockets. Giá mua Socket tiếp theo tăng lũy tiến theo số Socket đã có.
- Người chơi bắt đầu với 0 Sockets.

### 2.1.10. Magic Shop

- Mỗi shop có **15 mặt hàng** chia thành **3 gian hàng**, mỗi gian có **5 mặt hàng** của một loại: Equipment, Spell, Rune.
- Rank của mặt hàng phụ thuộc vào tiến trình game (Arc sau có xu hướng xuất hiện Rank cao hơn).
- Có bán Rune Sockets.
- Cung cấp dịch vụ `Enlighten` (mở Spell Slot), `Embed` và `Purge` Rune (đều tốn vàng).
- Giá cố định theo Rank, không phân biệt loại mặt hàng. Chi tiết xem tại _Progression Design — Section 4_.

### 2.1.11. Enemies

Kẻ địch chia làm 3 loại theo độ phức tạp hành vi:

|Loại|Số hành vi (Spell)|Ghi chú|
|---|---|---|
|Minion|1 – 2|Quái thường|
|Elite|2 – 3|Quái tinh anh|
|Boss|3 – 4|Quái trùm|

Mỗi enemy được định nghĩa trực tiếp bằng HP, Potencies, Resistances và danh sách spell được gắn sẵn — không dùng hệ Main Attribute của Player. Chi tiết xem tại _Combat Design — Section 4_.

### 2.1.12. Nodes

Đơn vị nhỏ nhất trên bản đồ. Các loại:

- **Combat Node:** Vào combat. Chia 3 loại nhỏ tương ứng với loại kẻ thù (Minion / Elite / Boss).
- **Shop Node:** Mở Magic Shop.
- **Rest Node:** Hồi phục toàn bộ HP và cộng thêm 1 điểm vào Main Attribute tùy chọn.
- **Event Node:** Sự kiện ngẫu nhiên — có thể tích cực, tiêu cực hoặc đánh đổi.

---

## 2.2. Core Mechanics

### 2.2.1. Elemental Effects

Là các buff/debuff gây ra bởi các phép nguyên tố.

- Dùng phép lên **bản thân/đồng minh** → apply **buff**.
- Dùng phép lên **kẻ địch** → apply **debuff**.

|Element|**Fire**|**Water**|**Ice**|**Lightning**|
|---|---|---|---|---|
|**Buff (+)**|Enrage (DMG+)|Refreshing (Mana Recover)|Fortified (DEF+)|Energized (AGI+)|
|**Debuff (−)**|Burn (DoT)|Drenched (DMG−)|Chilled (AGI−)|Dazed (DEF−)|

**Các effect đặc biệt** (kích hoạt khi đồng thời có 2 effect nhất định):

|Effect|Điều kiện|Mô tả|
|---|---|---|
|**Detonates**|Burn + Dazed|Gây Damage Burst lên tất cả kẻ địch|
|**Frozen**|Drenched + Chilled|Vô hiệu hóa mục tiêu trong lượt tiếp theo|
|**Overdrive**|Enrage + Energized|Lượt tiếp theo: tấn công luôn trúng và gây 1.5× sát thương|
|**Crystalize**|Refreshing + Fortified|Lượt tiếp theo: miễn nhiễm sát thương và debuff|

**Các effect độc lập khác:**

- **Regen:** Hồi HP vào Start Phase mỗi lượt.
- **Armor:** Chặn một lượng sát thương nhất định trước khi trừ vào HP.
- **Distracted:** Tăng mana cost của spell trong một số lượt nhất định.

### 2.2.2. Element Interactions

Quy ước tương khắc:

```
Nước > Lửa > Băng > Sét > Nước
">" = Khắc
```

|Tình huống|Kết quả|
|---|---|
|Effect B đang active, apply Effect A mà B > A|Effect A bị giải, B giữ nguyên.|
|Effect B đang active, apply Effect A mà A > B|Effect B bị giải, apply Effect A.|
|Effect A đang active, apply buff/debuff cùng nguyên tố A|Cả hai bị giải trừ (Neutralize).|

### 2.2.3. Turn Logic

Bắt đầu game với Player Turn → Enemy Turn → lặp lại luân phiên.

**Cấu trúc Turn:**

```
Start Phase → Action Phase → End Phase
```

- **Start Phase:** Hồi MP. Xử lý các effect tại thời điểm Start (DoT, Regen, Frozen, Crystalize...).
- **Action Phase:** Người chơi chọn Spell để cast (hoặc Enemy thực hiện hành vi).
- **End Phase:** Giảm duration các effect, xóa các effect hết hạn.

Chi tiết cơ chế combat xem tại _GDD — Combat Design_.

---

## 2.3. Progression

### 2.3.1. Level Structure

Trong một run, người chơi đi qua 3 Arc. Mỗi Arc gồm nhiều Node nối với nhau thành dạng đồ thị có hướng. Người chơi chỉ có thể di chuyển sang Node kề phía trước và không thể quay lại Node đã đi qua. Chi tiết xem tại _GDD — Progression Design_.

### 2.3.2. Combat Win/Lose Condition

- **WIN:** Tiêu diệt toàn bộ kẻ địch trong combat.
- **LOSE:** HP của Player về 0.

---

# 3. Theme

## 3.1. Visual Style

[TBD]

## 3.2. Sound Direction

[TBD]