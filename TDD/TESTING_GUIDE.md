___
**Game:** Wandering Wanderer 
**Author:** DukTofn 
**Last Updated:** 09/04/2026
___

# TDD — Testing Guide (Unity)

1. [Tổng quan Testing Strategy](#1-t%E1%BB%95ng-quan-testing-strategy)
2. [Cài đặt Unity Test Framework](#2-c%C3%A0i-%C4%91%E1%BA%B7t-unity-test-framework)
3. [Edit Mode Tests](#3-edit-mode-tests)
4. [Play Mode Tests](#4-play-mode-tests)
5. [Manual Tests](#5-manual-tests)
6. [Test Scene Setup](#6-test-scene-setup)
7. [Hướng dẫn viết test theo từng system](#7-h%C6%B0%E1%BB%9Bng-d%E1%BA%ABn-vi%E1%BA%BFt-test-theo-t%E1%BB%ABng-system)

---

## 1. Tổng quan Testing Strategy

### Ba tầng test

```
[Manual Test]      ← End-to-end, UI, "cảm giác" game
[Play Mode Test]   ← Cần Coroutine, MonoBehaviour, timing
[Edit Mode Test]   ← Pure logic, nhanh, không cần scene
```

**Nguyên tắc:** Càng xuống thấp càng ưu tiên. Mọi logic thuần (DamageCalculator, EffectSystem, ArmorStack...) phải có Edit Mode Test trước khi integrate lên Play Mode.

### Phân loại theo system

| System             | Loại test ưu tiên                              | Lý do                                               |
| ------------------ | ---------------------------------------------- | --------------------------------------------------- |
| `ArmorStack`       | Edit Mode                                      | Pure C# logic, không cần scene                      |
| `DamageCalculator` | Edit Mode                                      | Pure math                                           |
| `HitDodgeResolver` | Edit Mode                                      | Pure math + random                                  |
| `EffectSystem`     | Edit Mode                                      | Pure logic                                          |
| `CombatResolver`   | Edit Mode                                      | Cần mock entity                                     |
| `SpellSlotManager` | Edit Mode                                      | Pure data                                           |
| `CooldownTracker`  | Edit Mode                                      | Pure data                                           |
| `SpellCaster`      | Edit Mode (validation) + Play Mode (full flow) | Validation là pure logic; full flow cần VisualQueue |
| `TurnManager`      | Play Mode                                      | Cần Coroutine cho Phase transitions                 |
| `VisualQueue`      | Play Mode                                      | Cần Coroutine                                       |
| `MapSystem`        | Edit Mode                                      | Graph generation là pure algorithm                  |
| `GoldLedger`       | Edit Mode                                      | Pure math                                           |
| `RewardSystem`     | Edit Mode                                      | Pure logic + random                                 |
| `SaveSystem`       | Edit Mode                                      | File I/O                                            |
| UI Views           | Play Mode + Manual                             | Cần scene và visual                                 |

---

## 2. Cài đặt Unity Test Framework

### 2.1. Bật Test Framework

1. **Window → Package Manager** → tìm "Test Framework" → Install (thường đã có sẵn)
2. **Window → General → Test Runner** để mở cửa sổ test

### 2.2. Tạo Assembly Definitions

Cấu trúc `.asmdef` cần thiết:

```
Assets/
  Scripts/
    Logic/
      Logic.asmdef              ← KHÔNG reference UnityEngine
    Unity/
      Unity.asmdef              ← reference UnityEngine, reference Logic.asmdef
  Tests/
    EditMode/
      Tests.EditMode.asmdef     ← reference Logic.asmdef, NUnit
    PlayMode/
      Tests.PlayMode.asmdef     ← reference Logic.asmdef + Unity.asmdef, NUnit + UnityEngine.TestRunner
```

**`Tests.EditMode.asmdef` settings:**

```json
{
  "name": "Tests.EditMode",
  "references": ["Logic", "Unity.TestRunner", "UnityEngine.TestRunner"],
  "includePlatforms": ["Editor"],
  "optionalUnityReferences": ["TestAssemblies"]
}
```

**`Tests.PlayMode.asmdef` settings:**

```json
{
  "name": "Tests.PlayMode",
  "references": ["Logic", "Unity.asmdef", "Unity.TestRunner", "UnityEngine.TestRunner"],
  "optionalUnityReferences": ["TestAssemblies"]
}
```

### 2.3. Verify setup

Tạo file test mẫu, chạy Test Runner → thấy test xuất hiện là OK:

```csharp
// Assets/Tests/EditMode/SanityTest.cs
using NUnit.Framework;

public class SanityTest
{
    [Test]
    public void OnePlusOneEqualsTwo()
    {
        Assert.AreEqual(2, 1 + 1);
    }
}
```

---

## 3. Edit Mode Tests

### 3.1. Cấu trúc cơ bản

```csharp
using NUnit.Framework;
using WanderingWanderer.Logic; // namespace của Logic assembly

[TestFixture]
public class ArmorStackTests
{
    private ArmorStack _armor;

    [SetUp]
    public void SetUp()
    {
        // Chạy trước mỗi [Test] — khởi tạo fresh instance
        _armor = new ArmorStack();
    }

    [TearDown]
    public void TearDown()
    {
        // Chạy sau mỗi [Test] — cleanup nếu cần
        _armor = null;
    }

    [Test]
    public void TakeDamage_LessThanArmor_ReturnsZeroOverflow()
    {
        _armor.ApplyArmor(value: 50, duration: 3);

        float overflow = _armor.TakeDamage(30f);

        Assert.AreEqual(0f, overflow);
    }

    [Test]
    public void TakeDamage_MoreThanArmor_ReturnsCorrectOverflow()
    {
        _armor.ApplyArmor(value: 20, duration: 3);

        float overflow = _armor.TakeDamage(35f);

        Assert.AreEqual(15f, overflow, delta: 0.001f);
    }
}
```

### 3.2. Test nhiều case với `[TestCase]`

```csharp
[TestCase(10f, 10f, 90.0f)]   // VIT=10, 90% damage
[TestCase(90f, 90f, 50.0f)]   // VIT=90, 50% damage
[TestCase(180f, 180f, 33.33f)] // VIT=180, 33.3% damage
public void DamageCalculator_ResistanceFormula_CorrectReduction(
    float resistance, float rawDamage, float expectedActual)
{
    float actual = DamageCalculator.Calculate(rawDamage, resistance);

    Assert.AreEqual(expectedActual, actual, delta: 0.1f);
}
```

### 3.3. Test event được phát

```csharp
[Test]
public void EffectSystem_ApplyEffect_FiresOnEffectAppliedEvent()
{
    var effectSystem = new EffectSystem();
    EffectType receivedEffect = EffectType.None;
    effectSystem.OnEffectApplied += (e) => receivedEffect = e;

    effectSystem.Apply(EffectType.Burn);

    Assert.AreEqual(EffectType.Burn, receivedEffect);
}

[Test]
public void EffectSystem_NeutralizeCase_DoesNotFireOnEffectApplied()
{
    var effectSystem = new EffectSystem();
    effectSystem.Apply(EffectType.Enrage); // Fire Buff
    
    bool eventFired = false;
    effectSystem.OnEffectApplied += (_) => eventFired = true;

    // Apply Fire Debuff khi có Fire Buff → Neutralize, không fire event
    effectSystem.Apply(EffectType.Burn);

    Assert.IsFalse(eventFired);
    Assert.IsFalse(effectSystem.Has(EffectType.Enrage));
    Assert.IsFalse(effectSystem.Has(EffectType.Burn));
}
```

### 3.4. Test với Mock (không dùng Moq — dùng hand-written stub)

Vì không nên thêm dependency framework phức tạp cho game nhỏ, dùng stub class đơn giản:

```csharp
// Tạo stub trong folder Tests/EditMode/Stubs/
public class StubEntity : IEntity
{
    public int MaxHp { get; set; } = 100;
    public int CurrentHp { get; set; } = 100;
    public bool CrystalizeFlag { get; set; } = false;
    public ArmorStack ArmorStack { get; } = new ArmorStack();
    public EffectSystem EffectSystem { get; } = new EffectSystem();
    
    public float GetEffectivePotency(Element element) => 10f;
    public float GetEffectiveResistance(Element element) => 0f;
    public float GetEffectiveAGI() => 10f;
}

// Dùng trong test
[Test]
public void CombatResolver_Detonates_BypassesArmorAndResistance()
{
    var target = new StubEntity { MaxHp = 100, CurrentHp = 100 };
    target.ArmorStack.ApplyArmor(50, 3); // Có 50 Armor
    var resolver = new CombatResolver();
    
    // Trigger Detonates
    resolver.ResolveDetonates(target);
    
    // 30% × 100 = 30 damage, bypass armor
    Assert.AreEqual(70, target.CurrentHp); // 100 - 30 = 70
    Assert.AreEqual(50, target.ArmorStack.TotalValue); // Armor không bị ảnh hưởng
}
```

### 3.5. Test statistical (Random)

```csharp
[Test]
public void RandomPolicy_WithMultipleSpells_AllSpellsSelectedOverTime()
{
    var policy = new RandomPolicy();
    var available = new[] { SpellId.Fireball, SpellId.IceShard, SpellId.Shock };
    var context = new CombatContext(); // empty context
    var counts = new Dictionary<SpellId, int>
    {
        [SpellId.Fireball] = 0,
        [SpellId.IceShard] = 0,
        [SpellId.Shock] = 0,
    };

    const int iterations = 1000;
    for (int i = 0; i < iterations; i++)
    {
        var selected = policy.SelectSpell(available, context);
        counts[selected]++;
    }

    // Mỗi spell nên được chọn ít nhất 1 lần trong 1000 lần
    foreach (var count in counts.Values)
        Assert.Greater(count, 0, "Spell không bao giờ được chọn — policy bị lệch");
    
    // Xác suất mỗi cái ~33%, cho phép sai số ±10%
    foreach (var count in counts.Values)
        Assert.AreEqual(iterations / 3f, count, delta: iterations * 0.10f);
}
```

### 3.6. Test ScriptableObject data (không cần scene)

```csharp
[Test]
public void RewardRateConfig_AllRowsSumTo100Percent()
{
    // Load asset từ Resources (đặt config vào Assets/Resources/Config/)
    var config = Resources.Load<RewardRateConfig>("Config/RewardRateConfig");
    
    Assert.IsNotNull(config, "RewardRateConfig không tìm thấy trong Resources");
    
    foreach (var row in config.minionEliteRates)
    {
        float sum = row.rankI + row.rankII + row.rankIII;
        Assert.AreEqual(100f, sum, delta: 0.01f, 
            message: $"Arc {row.arc} Minion/Elite rates không cộng lại đúng 100%");
    }
}
```

---

## 4. Play Mode Tests

### 4.1. Cấu trúc cơ bản

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class VisualQueueTests
{
    [UnityTest]
    public IEnumerator VisualQueue_SingleCommand_DrainEventFires()
    {
        // Setup
        var queueGO = new GameObject("VisualQueue");
        var queue = queueGO.AddComponent<VisualQueue>();
        bool drained = false;
        queue.OnQueueDrained += () => drained = true;

        // Act
        queue.Enqueue(new WaitCommand(0.1f)); // Command delay 0.1s

        // Wait đủ để command chạy xong
        yield return new WaitForSeconds(0.3f);

        // Assert
        Assert.IsTrue(drained);

        // Cleanup
        Object.Destroy(queueGO);
    }
}

// Helper command cho test
public class WaitCommand : IActionCommand
{
    private readonly float _seconds;
    public WaitCommand(float seconds) { _seconds = seconds; }
    
    public IEnumerator Execute()
    {
        yield return new WaitForSeconds(_seconds);
    }
}
```

### 4.2. Test TurnManager phase order

```csharp
[UnityTest]
public IEnumerator TurnManager_PhaseOrder_IsCorrect()
{
    // Setup
    var scene = SceneManager.LoadScene("TestCombatScene", LoadSceneMode.Additive);
    yield return null; // Chờ scene load

    var turnManager = Object.FindObjectOfType<TurnManager>();
    var phases = new List<(Phase phase, EntityType owner)>();
    turnManager.OnPhaseChanged += (phase, owner) => phases.Add((phase, owner));

    // Act: chạy 1 turn đầy đủ
    turnManager.StartCombat();
    yield return new WaitForSeconds(3f); // Đủ thời gian cho 1 Player Turn + 1 Enemy Turn

    // Assert
    Assert.AreEqual(Phase.Start, phases[0].phase);
    Assert.AreEqual(EntityType.Player, phases[0].owner);
    Assert.AreEqual(Phase.Action, phases[1].phase);
    Assert.AreEqual(EntityType.Player, phases[1].owner);
    Assert.AreEqual(Phase.End, phases[2].phase);
    Assert.AreEqual(EntityType.Player, phases[2].owner);
    Assert.AreEqual(Phase.Start, phases[3].phase);
    Assert.AreEqual(EntityType.Enemy, phases[3].owner);
    // ... tiếp tục

    SceneManager.UnloadSceneAsync("TestCombatScene");
}
```

### 4.3. Test View unsubscribe

```csharp
[UnityTest]
public IEnumerator PlayerStatusView_OnDestroy_UnsubscribesAllEvents()
{
    // Setup
    var playerController = new PlayerController();
    var viewGO = new GameObject("PlayerStatusView");
    var view = viewGO.AddComponent<PlayerStatusView>();
    view.Initialize(playerController);
    yield return null;

    // Verify đã subscribe
    int countBefore = playerController.OnHpChanged.GetInvocationList().Length;
    Assert.Greater(countBefore, 0);

    // Destroy view
    Object.Destroy(viewGO);
    yield return null; // Chờ OnDestroy chạy

    // Verify đã unsubscribe
    int countAfter = playerController.OnHpChanged.GetInvocationList().Length;
    Assert.AreEqual(0, countAfter, "View chưa unsubscribe OnHpChanged sau khi Destroy");
}
```

### 4.4. Test timing với LogAssert

```csharp
[UnityTest]
public IEnumerator PhaseHandler_Regen_BeforeBurn_InSameStartPhase()
{
    // Setup: player có cả Regen và Burn
    // Verify: HP tăng trước khi HP giảm

    var log = new List<string>();
    // Patch PhaseHandler để log thứ tự
    // (hoặc dùng event OnRegenApplied, OnBurnApplied với timestamp)

    // ... setup scene ...

    yield return null;

    int regenIndex = log.IndexOf("Regen");
    int burnIndex = log.IndexOf("Burn");
    Assert.Less(regenIndex, burnIndex, "Regen phải xảy ra trước Burn");
}
```

---

## 5. Manual Tests

### 5.1. Checklist Manual Test — Combat

Thực hiện trong Unity Editor Play Mode với scene `TestCombatScene`.

**Basic combat flow:**

- [ ] Player Turn bắt đầu: SpellBar active, TurnIndicator hiển thị "Player Turn"
- [ ] Enemy Turn: SpellBar disabled, enemy animation chạy
- [ ] Tap spell khi enemy turn: không có gì xảy ra (hoặc feedback "Not your turn")
- [ ] HP bar giảm mượt mà khi nhận damage (không snap)
- [ ] Damage number xuất hiện đúng vị trí target, fade sau 1 giây

**Effect visuals:**

- [ ] Burn: icon lửa trên enemy, không có khi chưa bị
- [ ] Crystalize: hiệu ứng shield trên player khi active
- [ ] Overdrive: visual khác biệt rõ ràng với trạng thái bình thường
- [ ] Effect expire: icon biến mất, không còn hiệu ứng

**Combat end:**

- [ ] Enemy HP = 0: death animation, sau đó Reward screen
- [ ] Player HP = 0: Game Over, không phát thêm event nào

**Kiểm tra console:** Không có `NullReferenceException`, không có `MissingReferenceException`, không có warning về unsubscribed events.

---

### 5.2. Checklist Manual Test — Map

- [ ] Map load: tất cả Node hiển thị, đúng icon theo loại
- [ ] Chỉ các Node kề phía trước mới có thể tap (Node xa hơn hoặc đã đi qua → không tap được)
- [ ] Node đã visit: icon dim/grey, không vào lại được
- [ ] `pendingCombatModifier` icon xuất hiện sau Force Trade event, biến mất sau combat tiếp theo

---

### 5.3. Checklist Manual Test — Shop

- [ ] 15 item hiển thị đúng 3 gian (Equipment/Spell/Rune)
- [ ] Mua item khi đủ tiền: Gold giảm, item vào inventory
- [ ] Mua item khi thiếu tiền: Gold không giảm, shake animation hoặc message
- [ ] Enlighten khi đủ WIS: 1 Spell Slot mới mở khóa
- [ ] Embed Rune: rune vào socket, passive có hiệu lực ngay
- [ ] Purge Rune: rune ra khỏi socket, passive mất hiệu lực ngay

---

### 5.4. Checklist Manual Test — Save/Load

1. Chơi đến sau combat thứ 2 (chọn item reward xong)
2. Stop Play Mode
3. Start Play Mode lại
4. Verify: đúng Arc, đúng Node map đã visit, đúng inventory, đúng Gold
5. Verify: không có item bị duplicate hoặc mất

---

### 5.5. Performance check

Mở **Window → Analysis → Profiler** trong khi test:

|Kiểm tra|Ngưỡng chấp nhận|
|---|---|
|Frame time trong combat|< 16ms (60fps)|
|GC Allocation trong combat loop|< 1KB/frame (không alloc trong hot path)|
|Memory sau 10 combat (không load/unload scene)|Không tăng liên tục (no leak)|
|`VisualQueue` queue length peak|< 20 commands per Phase|

---

## 6. Test Scene Setup

### 6.1. `TestCombatScene`

Scene đơn giản để test combat logic. **Không cần visual đẹp.**

**Hierarchy:**

```
TestCombatScene
  ├── [Combat]
  │     ├── TurnManager
  │     ├── PhaseHandler
  │     ├── CombatResolver
  │     ├── VisualQueue
  │     ├── Player
  │     │     ├── PlayerController    ← component
  │     │     ├── ArmorStack          ← component
  │     │     ├── EffectSystem        ← component
  │     │     ├── SpellSlotManager    ← component
  │     │     └── CooldownTracker     ← component
  │     └── Enemy_Test
  │           ├── EnemyController     ← component, gắn TestMinion definition
  │           ├── ArmorStack          ← component
  │           └── EffectSystem        ← component
  ├── [UI]
  │     ├── PlayerStatusView
  │     ├── SpellBarView
  │     ├── EnemyStatusView
  │     └── TurnIndicatorView
  └── [Config]
        └── ConfigLoader             ← load CombatConfig SO vào runtime
```

**`ConfigLoader`** là MonoBehaviour đơn giản:

```csharp
public class ConfigLoader : MonoBehaviour
{
    [SerializeField] private CombatConfig combatConfig;
    
    void Awake()
    {
        CombatConfig.Instance = combatConfig; // hoặc inject theo cách project dùng
    }
}
```

---

### 6.2. `TestMapScene`

**Hierarchy:**

```
TestMapScene
  ├── MapSystem
  ├── NodeRouter
  ├── GameManager (minimal)
  └── [UI]
        └── MapGraphView
```

Gắn `ArcConfig_1` vào `MapSystem` để test sinh map Arc 1.

---

### 6.3. Test Configuration

Tạo thêm `CombatConfig_Test.asset` với giá trị cực đoan để test edge case:

|Field|Giá trị test|Mục đích|
|---|---|---|
|`HIT_THRESHOLD`|1|Dễ guaranteed hit|
|`BASE_DODGE`|0.5|Dodge 50% khi bằng AGI|
|`MAX_DODGE`|0.9|Max dodge 90%|
|`BASE_MP_RECOVERY`|999|Mana luôn đầy để test spell mà không lo mana|

---

## 7. Hướng dẫn viết test theo từng system

### 7.1. `ArmorStack` — Edit Mode

**File:** `Assets/Tests/EditMode/Combat/ArmorStackTests.cs`

Test cases cần cover:

1. Apply 1 stack, damage < stack value → overflow = 0
2. Apply 1 stack, damage > stack value → overflow đúng
3. Apply 2 stack, damage xuyên hết stack 1 → stack 2 nhận overflow đúng
4. Apply 2 stack, damage xuyên cả 2 → HP nhận overflow cuối
5. `Tick()` → duration giảm đúng
6. `Tick()` nhiều lần đến 0 → stack bị xóa, value dù còn
7. Stack mới có `hurt_order` cao hơn stack cũ
8. Damage luôn vào stack có `hurt_order` cao nhất trước

---

### 7.2. `EffectSystem` — Edit Mode

**File:** `Assets/Tests/EditMode/Combat/EffectSystemTests.cs`

Test cases cần cover:

1. Apply effect → `Has()` trả về true
2. Tick đến 0 → `Has()` trả về false
3. Apply Neutralize pair (Fire Buff + Fire Debuff) → cả hai gone
4. Apply same type (Debuff + Debuff cùng element) → Refresh, không tạo mới
5. Apply khi bị khắc → abort, cái cũ giữ nguyên
6. Apply khi khắc cái cũ → cái cũ bị giải, cái mới được apply
7. Events phát đúng lúc: applied, removed, combination

---

### 7.3. `DamageCalculator` — Edit Mode

**File:** `Assets/Tests/EditMode/Combat/DamageCalculatorTests.cs`

Test cases cần cover:

1. Formula đúng với nhiều giá trị resistance (dùng `[TestCase]`)
2. `crystalizeFlag = true` → damage = 0
3. `ignoreResistance = true` → bypass formula
4. `ignoreArmor = true` → bypass ArmorStack, trừ thẳng HP
5. Damage qua ArmorStack: đúng overflow, đúng HP cuối

---

### 7.4. `MapSystem` — Edit Mode

**File:** `Assets/Tests/EditMode/Map/MapSystemTests.cs`

Test cases cần cover (tất cả chạy với 100 random seed khác nhau):

1. Tổng số Node đúng theo config
2. Node cuối là Boss
3. Mọi path đến Boss đều có đủ Elite tối thiểu
4. Mọi path đến Boss đều có đủ Rest tối thiểu
5. Mọi path đến Boss đều có đủ Shop tối thiểu
6. Không có Node bị cô lập (unreachable)
7. Mỗi Node có 1–3 cạnh ra

```csharp
[Test]
public void MapSystem_GenerateArc1_AllPathsHaveMinimumEliteNodes()
{
    var config = Resources.Load<ArcConfig>("Config/ArcConfig_1");
    var mapSystem = new MapSystem();
    
    for (int seed = 0; seed < 100; seed++)
    {
        var graph = mapSystem.Generate(config, seed);
        var allPaths = graph.GetAllPathsToEnd();
        
        foreach (var path in allPaths)
        {
            int eliteCount = path.Count(n => n.Type == NodeType.Elite);
            Assert.GreaterOrEqual(eliteCount, config.minEliteOnPath,
                $"Seed {seed}: Path không đủ Elite node");
        }
    }
}
```

---

### 7.5. `TurnManager` — Play Mode

**File:** `Assets/Tests/PlayMode/Combat/TurnManagerTests.cs`

Test cases cần cover:

1. Phase order đúng (Start → Action → End, Player → Enemy → Player...)
2. `OnPhaseChanged` event phát đúng thời điểm
3. Combat end khi enemy HP = 0
4. Combat end khi player HP = 0
5. Frozen: Action Phase bị skip đúng 1 lượt
6. Phase chỉ chuyển SAU khi VisualQueue drain

---

### 7.6. `SaveSystem` — Edit Mode

**File:** `Assets/Tests/EditMode/Meta/SaveSystemTests.cs`

Test cases cần cover:

1. `Save()` tạo file tại đúng path
2. `Load()` đọc file và khôi phục đúng state
3. Save với `pendingCombatModifier = null` → Load không có modifier
4. Save với modifier → Load có modifier đúng
5. Save với map seed → `MapSystem.Generate(seed)` cho kết quả giống hệt

**Lưu ý:** Dùng temp path trong test để không ảnh hưởng save game thật:

```csharp
[SetUp]
public void SetUp()
{
    SaveSystem.OverrideSavePath(Application.temporaryCachePath + "/test_save.json");
}

[TearDown]
public void TearDown()
{
    File.Delete(Application.temporaryCachePath + "/test_save.json");
    SaveSystem.ResetSavePath();
}
```

---

## Phụ lục: Chạy test nhanh

**Trong Unity Editor:**

- Mở **Window → General → Test Runner**
- Tab **EditMode**: click **Run All** để chạy tất cả Edit Mode test
- Tab **PlayMode**: click **Run All** để chạy Play Mode test (mất lâu hơn, mở play mode)

**Xem kết quả:**

- ✅ Xanh = pass
- ❌ Đỏ = fail, click vào xem message + stack trace
- ⚠️ Vàng = test bị skip (đánh dấu `[Ignore]`)

**Khi test fail:**

1. Đọc kỹ Assert message — nó cho biết expected vs actual
2. Nếu là Edit Mode test: breakpoint trong test method, chạy Debug
3. Nếu là Play Mode test: thêm `Debug.Log` trong test hoặc system để trace

**Tip:** Chạy Edit Mode test thường xuyên trong quá trình code (sau mỗi feature nhỏ). Play Mode test chạy trước khi merge hoặc cuối buổi làm việc.