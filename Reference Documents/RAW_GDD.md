# GAME DESIGN DOCUMENT: FRUIT FORTRESS
**Author:** HieuLM 
**Genre:** Idle, Action, Strategy 
**Visual Style:** Top-down 2D 

---

## 1. INTRODUCTION
**Fruit Fortress** is a tower defense game featuring automated combat mechanics. The game utilizes random Spawning and Power-up systems to drive replayability, targeting the player's desire for progression and curiosity about character skills and strength.

---

## 2. GAME STRUCTURE

### 2.1. Game Elements

#### 2.1.1. Fruits (HQ)
* Fruits are generated randomly after the user performs a **Pull** action.
* The Fruit determines the level and type of the Hero that will be summoned later.
* There are 4 Star levels. At 4 Stars, a Fruit automatically disappears and summons a Fruit Hero of the corresponding type/level.

#### 2.1.2. The Queue
* This object manages and displays currently owned Fruits.
* **Capacity:** There are 7 empty slots in the queue and 1 separate "Hold" slot.
* **Constraint:** Users cannot perform a Pull action if all queue slots (excluding Hold) are full.

#### 2.1.3. Hold Slot
* A dedicated slot to manage/display a Fruit the user wants to preserve.
* It is located separately from the rest of the Queue.

#### 2.1.4. Pull Rate
* The Pull Rate has multiple levels, which determine the probability of obtaining Fruits at different star tiers.
* Higher Pull Rate levels increase the "Luck" factor, yielding higher star-tier Fruits.
* *Example Rates (Level 1):* 80% Common, 18% Rare, 2% Epic.

#### 2.1.5. Energy (E)
* Energy is the resource required to perform the **Pull** action.
* **Initialization:** Users start with a set amount (e.g., 20) and a regeneration rate (e.g., 1E/s).
* **Upgrades:** Stats can be modified via in-game Power-ups or meta-progression outside the match.

#### 2.1.6. The Wall
* The Wall is the objective the user must protect.
* If the Wall's HP reaches 0, the user loses the game.

#### 2.1.7. Monsters
* Monsters are both the threat and the victory target for the player.
* **Stats:** Level, HP, Mana, Mana/Attack, Move Speed, Attack Speed, Range, Damage, Armor, and EXP reward.

#### 2.1.8. Fruit Heroes (AHHQ)
* Heroes are summoned via the **Spawn** action.
* **Behavior:** They attack the nearest monster within range.
* **Stats:** Level, HP, Mana, Mana/Attack, Move Speed, Attack Speed, Range, Damage, Armor.

---

## 3. GAME MECHANICS

### 3.1. Combat Mechanics (AI & Targeting)
Characters (Monsters and Heroes) select targets based on a **Target Score** rule.

**The Target Score Formula:**
> `target_score = distance_score * priority_score` 
> *The entity selects the target with the highest target_score.*

1.  **Distance Score:** Calculated as `attack_range - distance`. The closer the target, the higher the score.
2.  **Priority Score:** Calculated as `type_score + effect_score`.
    * **Type Score:** Represents base priority (e.g., Monsters prioritize The Wall > Defending Character > Attacking Character).
    * **Effect Score:** Dynamic values from skills (e.g., a "Taunt" skill increases effect_score, forcing enemies to target the caster).

**Behavior Flow:**
1.  Scan for targets in range.
2.  Calculate Target Score.
3.  Chase and Attack target.
4.  **Re-evaluate Event:** If the target dies, moves out of range, or the attacker is hit by Hard CC (Stun, Knockback, Taunt).

**Monster Spawning (Waves):**
* Monsters spawn in **Batches** structured as: `[list of monsters] + [start time] + [end time] + [gap time] + [spawn count] + [loop boolean]`.
* If `is_loop` is true, the batch repeats after finishing the list.

### 3.2. Pull Mechanic
* **Condition:** User has enough Energy and an empty Queue slot.
* **Result:** Energy is deducted; a random Fruit type/star is generated based on Pull Rate.
* **Exceptions:**
    * *Low Energy:* Text turns red; button press triggers a "shake" effect.
    * *Full Queue:* Displays text "No more room".
    * *New User Rule:* The first pulls may follow a fixed hidden rule rather than true RNG to assist new players.

### 3.3. Merge Mechanic
* **Condition:** 3 Fruits of the same Type and Star Level exist in the Queue.
* **Result:** The 3 Fruits merge into 1 Fruit of the next Star Level (Star + 1).
* **Exception (Auto-Spawn):** If merging three **3-Star** Fruits, the result is immediately converted into a **4-Star Hero** on the field (skipping the Queue).

### 3.4. Spawn Mechanic
* **Condition:** At least 1 Fruit exists in the Queue.
* **Result:** All Fruits in the Queue (excluding Hold) are converted to Heroes.
    * Spawn position depends on Hero type (Melee spawns forward, Ranged spawns back).

---

## 4. PROGRESSION

### 4.1. Leveling
* Players collect EXP by killing monsters to increase the in-game level.
* **Reward:** Upon leveling up, the player selects a **Power-up** that applies only to the current match.

### 4.2. Victory Condition
* **Monster Counter:** Tracks the number of killed monsters vs. total monsters (e.g., 20/55).
* **Win:** The player wins by completing the kill counter progress.