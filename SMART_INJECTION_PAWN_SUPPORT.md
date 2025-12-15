# SmartInjectionManager - Pawn Support Confirmation

## ? 确认：支持所有 Pawn 类型

`SmartInjectionManager` 和常识库系统**已经完全支持**所有 Pawn 类型，包括：

- ? **殖民者** (Colonists)
- ? **囚犯** (Prisoners)
- ? **访客** (Guests)
- ? **敌人** (Enemies)
- ? **动物** (Animals)
- ? **机械体** (Mechanoids)
- ? **任何其他Pawn** (Any other Pawn types)

## ?? 代码验证

### 1. SmartInjectionManager.cs

```csharp
public static string InjectSmartContext(
    Pawn speaker,     // ← 接受任何 Pawn 类型
    Pawn listener,    // ← 接受任何 Pawn 类型
    string context,
    int maxMemories = 10,
    int maxKnowledge = 5)
{
    // 没有 speaker.IsColonist 检查
    // 没有 listener.IsColonist 检查
    
    // 1. 记忆注入 - 对所有Pawn有效
    var memoryComp = speaker.TryGetComp<FourLayerMemoryComp>();
    
    // 2. 常识注入 - 对所有Pawn有效
    string knowledgeText = memoryManager.CommonKnowledge.InjectKnowledgeWithDetails(
        context,
        maxKnowledge,
        out var knowledgeScores,
        speaker,    // ← 所有Pawn
        listener    // ← 所有Pawn
    );
}
```

### 2. CommonKnowledgeLibrary.cs

```csharp
// 过滤常识：只保留全局常识(-1)或专属于当前Pawn的常识
var filteredEntries = entries
    .Where(e => e.isEnabled)
    .Where(e => e.targetPawnId == -1 ||  // 全局常识
               (currentPawn != null && e.targetPawnId == currentPawn.thingIDNumber)) // 或专属于当前Pawn
    .ToList();
```

**关键点**：
- ? 没有 `.IsColonist` 检查
- ? 只检查 `targetPawnId` 是否匹配
- ? `thingIDNumber` 对所有Pawn类型有效

### 3. 专属常识功能

专属常识功能通过 `targetPawnId` 工作：

```csharp
public class CommonKnowledgeEntry
{
    public int targetPawnId = -1;  // -1 = 全局, >=0 = 专属于特定Pawn
}
```

**工作原理**：
- `-1`：全局常识，所有Pawn都可以看到
- `>= 0`：专属常识，只有 `thingIDNumber == targetPawnId` 的Pawn可以看到

**重要**：`thingIDNumber` 是 `Verse.Thing` 的属性，对所有Pawn类型（殖民者、囚犯、访客、敌人）都有效。

## ?? 实际应用示例

### 示例1：囚犯对话

```csharp
Pawn prisoner = ...;  // 囚犯
Pawn colonist = ...;  // 殖民者

string context = SmartInjectionManager.InjectSmartContext(
    speaker: prisoner,    // ← 囚犯
    listener: colonist,   // ← 殖民者
    context: "囚犯与殖民者对话",
    maxMemories: 10,
    maxKnowledge: 5
);

// ? 囚犯可以访问：
//    - 全局常识 (targetPawnId = -1)
//    - 囚犯专属常识 (targetPawnId = prisoner.thingIDNumber)
```

### 示例2：敌人AI对话（Mod扩展）

```csharp
Pawn raider = ...;  // 敌人

string context = SmartInjectionManager.InjectSmartContext(
    speaker: raider,      // ← 敌人
    listener: null,
    context: "敌人AI思考",
    maxMemories: 5,
    maxKnowledge: 3
);

// ? 敌人可以访问：
//    - 全局常识 (targetPawnId = -1)
//    - 敌人专属常识 (targetPawnId = raider.thingIDNumber)
```

### 示例3：访客对话

```csharp
Pawn visitor = ...;  // 访客

string context = SmartInjectionManager.InjectSmartContext(
    speaker: visitor,     // ← 访客
    listener: null,
    context: "访客与殖民地交流",
    maxMemories: 8,
    maxKnowledge: 4
);

// ? 访客可以访问：
//    - 全局常识 (targetPawnId = -1)
//    - 访客专属常识 (targetPawnId = visitor.thingIDNumber)
```

## ?? UI限制说明

虽然系统支持所有Pawn类型，但**UI界面**（`Dialog_CommonKnowledge.cs`）中的专属Pawn选择器**目前只显示殖民者**：

```csharp
// 添加所有殖民者选项
var colonists = Find.Maps?.SelectMany(m => m.mapPawns.FreeColonists).ToList();
```

这是一个**UI限制**，不是功能限制。如果需要，可以扩展UI来选择囚犯、访客等。

### 如何为非殖民者创建专属常识？

方法1：**手动设置 `targetPawnId`**（需要知道Pawn的ID）

方法2：**通过代码API**：
```csharp
var entry = new CommonKnowledgeEntry("标签", "内容");
entry.targetPawnId = prisoner.thingIDNumber;  // 直接设置囚犯ID
library.AddEntry(entry);
```

方法3：**扩展UI**（未来功能）：
- 在UI中添加"所有Pawn"选项
- 包括囚犯、访客、敌人等

## ? 总结

- ? **SmartInjectionManager** 完全支持所有Pawn类型
- ? **专属常识功能** 对所有Pawn类型有效
- ? **没有 `.IsColonist` 限制**
- ?? **UI限制**：专属Pawn选择器目前只显示殖民者（可扩展）

**结论**：系统已经完全支持您的需求，无需修改核心代码。如果需要UI支持非殖民者，可以修改 `Dialog_CommonKnowledge.cs` 的 `DrawEditPanel` 方法。
