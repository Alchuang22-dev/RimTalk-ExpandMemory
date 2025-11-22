# ??? 标签自动扩展设计

## ?? 问题

用户习惯写直观的标签，但这些标签不一定能被匹配：

```
用户写: [美狐,战斗]
上下文: "美狐攻击敌人"
结果: 匹配不到"战斗" ?
```

---

## ?? 解决方案：标签自动扩展

### 核心思路

用户写一个标签，系统自动扩展为多个同义词/相关词：

```
用户写: [战斗]
系统扩展为: [战斗,攻击,打架,敌人,伤害,kill,fight]
```

---

## ?? 实现方案

### 方案A：预定义同义词词典 ? **推荐**

```csharp
public static class TagExpansion
{
    private static Dictionary<string, List<string>> expansionDict = new Dictionary<string, List<string>>()
    {
        // 战斗相关
        {"战斗", new List<string> {"战斗", "攻击", "打架", "敌人", "伤害", "fight", "attack", "enemy"}},
        {"攻击", new List<string> {"攻击", "战斗", "打击", "伤害", "attack", "damage"}},
        {"防御", new List<string> {"防御", "保护", "守护", "墙壁", "防护", "defense", "protect"}},
        
        // 建造相关
        {"建造", new List<string> {"建造", "建筑", "施工", "修建", "construction", "build"}},
        {"修理", new List<string> {"修理", "维修", "修复", "repair", "fix"}},
        
        // 烹饪相关
        {"烹饪", new List<string> {"烹饪", "做饭", "料理", "厨师", "cooking", "cook", "chef"}},
        {"食物", new List<string> {"食物", "吃", "饭", "餐", "food", "meal", "eat"}},
        
        // 医疗相关
        {"医疗", new List<string> {"医疗", "治疗", "医生", "护士", "医治", "medical", "doctor", "heal"}},
        {"受伤", new List<string> {"受伤", "伤", "伤势", "伤口", "injured", "wound", "hurt"}},
        
        // 社交相关
        {"社交", new List<string> {"社交", "聊天", "交流", "对话", "说话", "social", "talk", "chat"}},
        {"朋友", new List<string> {"朋友", "友谊", "伙伴", "同伴", "friend", "buddy"}},
        
        // 工作相关
        {"工作", new List<string> {"工作", "劳动", "任务", "干活", "work", "job", "task"}},
        {"休息", new List<string> {"休息", "睡觉", "放松", "娱乐", "rest", "sleep", "relax"}},
        
        // 情绪相关
        {"开心", new List<string> {"开心", "高兴", "快乐", "愉快", "happy", "joy"}},
        {"难过", new List<string> {"难过", "悲伤", "伤心", "痛苦", "sad", "unhappy"}},
        {"生气", new List<string> {"生气", "愤怒", "恼火", "angry", "mad"}},
        
        // 角色相关
        {"角色", new List<string> {"角色", "殖民者", "人", "pawn", "colonist"}},
        {"技能", new List<string> {"技能", "能力", "擅长", "skill", "ability"}},
        
        // 世界观相关
        {"世界观", new List<string> {"世界观", "设定", "背景", "lore", "setting"}},
        {"规则", new List<string> {"规则", "限制", "要求", "rule", "requirement"}},
    };
    
    /// <summary>
    /// 扩展标签
    /// </summary>
    public static List<string> ExpandTag(string tag)
    {
        if (string.IsNullOrEmpty(tag))
            return new List<string>();
        
        // 检查是否有预定义扩展
        if (expansionDict.TryGetValue(tag, out var expanded))
        {
            return new List<string>(expanded);
        }
        
        // 没有预定义，返回原标签
        return new List<string> { tag };
    }
    
    /// <summary>
    /// 扩展多个标签
    /// </summary>
    public static List<string> ExpandTags(List<string> tags)
    {
        var result = new HashSet<string>();
        
        foreach (var tag in tags)
        {
            var expanded = ExpandTag(tag);
            foreach (var exp in expanded)
            {
                result.Add(exp);
            }
        }
        
        return result.ToList();
    }
}
```

---

## ?? 效果对比

### 场景1：用户写[战斗]

**扩展前**:
```
标签: ["战斗"]
上下文: "美狐攻击敌人"
匹配: ? 无匹配
```

**扩展后**:
```
标签: ["战斗", "攻击", "打架", "敌人", "伤害", "fight", "attack", "enemy"]
上下文: "美狐攻击敌人"
匹配: ? "攻击", "敌人"
```

---

### 场景2：用户写[美狐,战斗]

**扩展前**:
```
标签: ["美狐", "战斗"]
上下文: "美狐使用灵能攻击"
匹配率: 1/2 = 50% (只匹配"美狐")
```

**扩展后**:
```
原标签: ["美狐", "战斗"]
扩展后: ["美狐", "战斗", "攻击", "打架", "敌人", "伤害", "fight", "attack", "enemy"]
上下文: "美狐使用灵能攻击"
匹配率: 2/9 = 22% (匹配"美狐", "攻击")
```

**问题**: 扩展后标签过多，匹配率反而降低！

---

## ?? 改进方案：智能扩展

### 方案B：只扩展，不改变匹配逻辑

**核心思路**：扩展标签但保持原标签数量不变用于计算匹配率

```csharp
public class CommonKnowledgeEntry
{
    public string tag;                    // 原始标签: "美狐,战斗"
    private List<string> originalTags;    // 原始标签列表: ["美狐", "战斗"]
    private List<string> expandedTags;    // 扩展后: ["美狐", "战斗", "攻击", "敌人", ...]
    
    /// <summary>
    /// 计算匹配（使用扩展标签，但基于原标签数量）
    /// </summary>
    public float CalculateRelevanceScore(List<string> contextKeywords)
    {
        if (!isEnabled) return 0f;
        
        float baseScore = importance * 0.05f;
        
        if (contextKeywords == null || contextKeywords.Count == 0)
            return baseScore;
        
        // 获取扩展标签用于匹配
        if (expandedTags == null)
        {
            originalTags = GetTags(); // ["美狐", "战斗"]
            expandedTags = TagExpansion.ExpandTags(originalTags); // ["美狐", "战斗", "攻击", ...]
        }
        
        // 使用扩展标签匹配
        int matchedCount = 0;
        foreach (var expandedTag in expandedTags)
        {
            foreach (var keyword in contextKeywords)
            {
                if (expandedTag.Equals(keyword, StringComparison.OrdinalIgnoreCase) ||
                    expandedTag.Contains(keyword) || 
                    keyword.Contains(expandedTag))
                {
                    matchedCount++;
                    break;
                }
            }
        }
        
        // 但匹配率基于原标签数量！
        float matchRate = (float)matchedCount / originalTags.Count;
        
        float totalScore = baseScore + (matchRate * importance * TagWeight);
        return totalScore;
    }
}
```

---

## ?? 改进后效果

### 场景：[美狐,战斗]

```
原标签: ["美狐", "战斗"] - 2个
扩展标签: ["美狐", "战斗", "攻击", "打架", "敌人", "伤害"] - 8个

上下文: "美狐攻击敌人"

匹配:
- "美狐" ?
- "攻击" ? (扩展的)
- "敌人" ? (扩展的)

匹配数: 3
匹配率: 3 / 2 = 150% (超过100%！)
```

**问题**: 匹配率超过100%，不合理！

---

## ?? 最终方案：匹配次数限制

```csharp
/// <summary>
/// 计算匹配（智能扩展，限制匹配次数）
/// </summary>
public float CalculateRelevanceScore(List<string> contextKeywords)
{
    if (!isEnabled) return 0f;
    
    float baseScore = importance * 0.05f;
    
    if (contextKeywords == null || contextKeywords.Count == 0)
        return baseScore;
    
    // 获取原始标签和扩展标签
    var originalTags = GetTags();
    var expandedTags = TagExpansion.ExpandTags(originalTags);
    
    // 计算匹配（每个原标签只能匹配一次）
    int matchedOriginalCount = 0;
    var matchedOriginalTags = new HashSet<string>();
    
    foreach (var originalTag in originalTags)
    {
        // 获取该原标签的扩展版本
        var expansions = TagExpansion.ExpandTag(originalTag);
        
        // 检查是否有任何扩展版本被匹配
        bool matched = false;
        foreach (var expansion in expansions)
        {
            foreach (var keyword in contextKeywords)
            {
                if (expansion.Equals(keyword, StringComparison.OrdinalIgnoreCase) ||
                    expansion.Contains(keyword) || 
                    keyword.Contains(expansion))
                {
                    matched = true;
                    matchedOriginalTags.Add(originalTag);
                    break;
                }
            }
            if (matched) break;
        }
        
        if (matched)
            matchedOriginalCount++;
    }
    
    // 匹配率基于原标签数量
    float matchRate = (float)matchedOriginalCount / originalTags.Count;
    
    float totalScore = baseScore + (matchRate * importance * TagWeight);
    return totalScore;
}
```

---

## ?? 最终效果

### 场景：[美狐,战斗]

```
原标签: ["美狐", "战斗"]

扩展:
- "美狐" → ["美狐"]
- "战斗" → ["战斗", "攻击", "打架", "敌人", "伤害", "fight", "attack", "enemy"]

上下文: "美狐攻击敌人"

匹配:
- "美狐" → 匹配"美狐" ?
- "战斗" → 匹配"攻击" ? (扩展词匹配)

匹配数: 2
匹配率: 2 / 2 = 100%
总分: 0.05 + (1.0 * 0.8 * 0.3) = 0.29 ?
```

**完美！** ?

---

## ?? UI设计

### 常识库编辑界面

```
┌─────────────────────────────────────────────┐
│ 标签: [美狐,战斗]                           │
│                                             │
│ ?? 自动扩展预览:                            │
│ ? 美狐 → 美狐                               │
│ ? 战斗 → 战斗, 攻击, 打架, 敌人, ...       │
│                                             │
│ 内容: 美狐是灵能大师                        │
│ 重要性: 0.8                                 │
└─────────────────────────────────────────────┘
```

### 调试预览器

```
常识: [美狐,战斗]美狐是灵能大师

原标签: 美狐, 战斗
扩展后: 美狐, 战斗, 攻击, 打架, 敌人, 伤害, ...

上下文关键词: 美狐, 使用, 灵能, 攻击, 敌人

匹配分析:
? 美狐 (原标签) → 匹配 "美狐"
? 战斗 (原标签) → 匹配 "攻击" (扩展词)

匹配率: 2/2 = 100%
总分: 0.29 ?
```

---

## ?? 配置选项

### 用户可配置

```csharp
public class RimTalkMemoryPatchSettings : ModSettings
{
    // 是否启用标签自动扩展
    public bool enableTagExpansion = true;
    
    // 是否显示扩展预览
    public bool showExpansionPreview = true;
}
```

### UI

```
? 启用标签自动扩展
  自动将"战斗"扩展为"战斗,攻击,敌人,伤害"等同义词
  
? 显示扩展预览
  在编辑常识时显示扩展后的标签列表
```

---

## ?? 同义词词典管理

### 用户可编辑

允许用户自定义同义词映射：

```
Mod设置 → 标签扩展词典

战斗 → 战斗, 攻击, 打架, 敌人, 伤害, fight, attack
建造 → 建造, 建筑, 施工, 修建, construction, build
烹饪 → 烹饪, 做饭, 料理, 厨师, cooking, cook

[添加新映射]
[导入词典]
[导出词典]
[恢复默认]
```

---

## ?? 优先级

### Phase 1 (MVP)
1. ? 实现基础扩展逻辑
2. ? 预定义常用标签词典
3. ? 修改匹配算法

### Phase 2 (增强)
1. ?? UI显示扩展预览
2. ?? 调试预览器显示扩展
3. ?? 配置开关

### Phase 3 (高级)
1. ?? 用户自定义词典
2. ?? 词典导入导出
3. ?? 智能学习（根据匹配历史优化）

---

## ?? 总结

**核心价值**:
- ? 用户写直观的标签（"战斗"）
- ? 系统自动扩展为匹配词（"攻击"、"敌人"）
- ? 匹配率基于原标签数量
- ? 复制粘贴的常识库也能正常工作

**用户体验**:
```
用户: 我就写"战斗"
系统: 没问题，我会自动匹配"攻击"、"敌人"等相关词！
```

---

**现在用户可以放心写"战斗"了！** ??
