# Spells — GDD

## Mục lục
- [Wisdom (WIS) và Imprint](#wisdom-wis-và-imprint)
- [1. Rank I](#1-rank-i)
- [2. Rank II](#2-rank-ii)

---

## Wisdom (WIS) và Imprint

- Mỗi spell trong data có field **`minWisdomToImprint`** (số nguyên ≥ 0): **WIS** hiện tại của nhân vật (main attribute sau khi cộng equipment/rune, cùng nguồn với chỉ số dùng cho mở slot) phải **≥** giá trị này mới được **Imprint** vào một Spell Slot **đã mở**.
- Ngưỡng Imprint **độc lập** với **mở slot** (`WisdomSlotConfig` + dịch vụ `Enlighten`): có thể có nhiều ô trống nhưng vẫn không Imprint được spell Rank cao nếu WIS chưa đủ.
- **Balancing:** thường Rank I thấp, Rank II trung bình, Rank III cao — bảng dưới dùng cột **Min WIS (Imprint)** `[TBD]` cho đến khi lock số.

---

# 1. Rank I

| ID  | Name         | Description                                                                                                                                        | Element   | Cooldown | Min WIS (Imprint) |
| --- | ------------ | -------------------------------------------------------------------------------------------------------------------------------------------------- | --------- | -------- | ----------------- |
| 01  | Fireball     | Deal (`[TBD]% fire_pot`) fire damage to selected enemy                                                                                             | Fire      | 1        | [TBD]             |
| 02  | Ignite       | Apply burn to the target, `burn` damage is increase by (`[TBD]% fire_pot`)% for the next 3 turn                                                    | Fire      | 2        | [TBD]             |
| 03  | Candle Ghost | Deal (`[TBD]% fire_pot`) fire damage to three lowest HP enemies (nếu nhiều kẻ địch có cùng lượng máu thì tấn công kẻ địch có `fire_res` thấp nhất) | Fire      | 2        | [TBD]             |
| 04  | Heal         | Heal for the amount of (`[TBD]% water_pot`).                                                                                                       | Water     | 2        | [TBD]             |
| 05  | Splash       | Deal `([TBD]% water_pot)` water damage to selected enemy                                                                                           | Water     | 1        | [TBD]             |
| 06  | Bubble       | Stun the selected enemy for 1 turn                                                                                                                 | Water     | 3        | [TBD]             |
| 07  | Ice Shard    | Deal (`[TBD]% ice_pot`) ice damage to selected enemy                                                                                               | Ice       | 1        | [TBD]             |
| 08  | Ice Shield   | Apply Armor (`[TBD]% ice_pot`, 3). (value, duration)                                                                                               | Ice       | 2        | [TBD]             |
| 09  | Cold Breathe | Deal `([TBD]% ice_pot)` ice damage to all enemies                                                                                                  | Ice       | 2        | [TBD]             |
| 10  | Shock        | Deal (`[TBD]% lightning_pot`) lightning damage to selected enemy                                                                                   | Lightning | 1        | [TBD]             |
| 11  | Spark        | Deal (`[TBD]% lightning_pot`) lightning damage to selected enemy and 50% of the main damage to 1 random enemy                                      | Lightning | 1        | [TBD]             |
| 12  | Pulse        | Deal (`[TBD]% lightning_pot`) lightning damage to 3 random enemies                                                                                 | Lightning | 2        | [TBD]             |

# 2. Rank II

| ID  | Name         | Description                                                                                    | Element | Cooldown | Min WIS (Imprint) |
| --- | ------------ | ---------------------------------------------------------------------------------------------- | ------- | -------- | ----------------- |
| 13  | Pyroblast    | Deal (`[TBD]% fire_pot`) fire damage to selected enemy and 25% of the main damage to the rest. | Fire    | 2        | [TBD]             |
| 14  | Flame Pillar | Deal (`[TBD]% fire_pot`) fire damage to selected enemy                                         | Fire    | [TBD]    | [TBD]             |
