# 常识库自动生成功能 - 待实现

## 功能概述

在常识库管理界面（Dialog_CommonKnowledge）添加一个折叠区域，包含两个自动生成按钮：

### 1. 殖民者状态常识生成 ?
- **按钮**: ?? 殖民者状态常识
- **功能**: 为每个殖民者生成状态信息
- **生成内容**:
  - 入殖时长（新人/老成员/元老）
  - 重要关系（配偶、恋人）
- **标签格式**: `殖民者,{名字}`
- **重要性**: 0.6

### 2. 事件记录常识生成 ?  
- **按钮**: ?? 事件记录常识
- **功能**: 从游戏日志提取重大事件
- **提取来源**: `Find.PlayLog.AllEntries`
- **事件类型**:
  - 死亡、击杀
  - 袭击、战斗
  - 结婚、订婚、分手
  - 加入、逃跑、离开
  - 完成建造、研究
  - 贸易、商队
- **标签格式**: `事件,历史`
- **重要性**: 0.6
- **时间范围**: 只记录最近60天的事件

## 实现位置

### 需要修改的文件
`Source/Memory/UI/Dialog_CommonKnowledge.cs`

### 添加位置
1. **类字段**（第22行左右，在`editMode`之后）:
```csharp
private bool expandAutoGenerate = false;
```

2. **DoWindowContents方法**（第56行，工具栏之后、搜索框之前）:
```csharp
// ? 自动生成常识折叠区
Rect autoGenRect = new Rect(0f, yPos, inRect.width, expandAutoGenerate ? 120f : 35f);
DrawAutoGenerateSection(autoGenRect);
yPos += expandAutoGenerate ? 125f : 40f;
```

3. **新增方法**（在`ExportToFile`方法之后、`Dialog_TextInput`类之前）:
   - `DrawAutoGenerateSection(Rect rect)` - 绘制折叠区
   - `DrawAutoGenerateContent(Rect rect)` - 绘制按钮
   - `GeneratePawnStatusKnowledge()` - 生成殖民者状态
   - `GenerateEventRecordKnowledge()` - 生成事件记录  
   - `ExtractEventInfo(LogEntry logEntry)` - 提取事件信息

## 关键代码片段

### 提取事件信息
```csharp
var gameHistory = Find.PlayLog;
if (gameHistory != null)
{
    var recentEntries = gameHistory.AllEntries
        .Where(e => e != null)
        .OrderByDescending(e => e.Age)
        .Take(100);

    foreach (var logEntry in recentEntries)
    {
        string text = logEntry.ToGameStringFromPOV(null, false);
        // 过滤和处理...
    }
}
```

### 计算时间
```csharp
int currentTick = Find.TickManager.TicksGame;
int ticksAgo = currentTick - logEntry.Age;
int daysAgo = ticksAgo / GenDate.TicksPerDay;
```

### 去重检查
```csharp
bool exists = library.Entries.Any(e => 
    e.content.Contains(eventDesc.Substring(0, Math.Min(15, eventDesc.Length)))
);
```

## 注意事项

1. **不要覆盖现有方法**: 只添加新方法，不要修改`DrawEntryList`等现有方法
2. **API正确性**: 使用`Find.PlayLog.AllEntries`而不是不存在的`map.battleLog`
3. **时间计算**: 使用`GenDate.TicksPerDay`进行tick到天的转换
4. **去重**: 检查内容前15个字符是否已存在
5. **折叠状态**: 使用`TexButton.Collapse`和`TexButton.Reveal`图标

## 测试建议

1. 进入游戏后打开常识库管理
2. 展开"?? 自动生成常识"折叠区
3. 点击"?? 殖民者状态常识"，确认生成成功
4. 点击"?? 事件记录常识"，确认从日志提取成功
5. 检查生成的常识是否去重正确

## 完整代码

完整代码太长无法在此展示，请参考提交历史或以下文件：
- 备份: `Dialog_CommonKnowledge.cs.autoGen.backup`
- Git提交: 查找包含"auto generate knowledge"的提交

## 状态

- [x] 功能设计完成
- [x] 代码编写完成
- [ ] 测试验证（待下次更新）
- [ ] 部署到游戏（待下次更新）

## 下次更新步骤

1. 从Git恢复`Dialog_CommonKnowledge.cs`
2. 手动添加上述代码片段（不使用edit_file工具）
3. 编译并测试
4. 部署到1.5和1.6版本
