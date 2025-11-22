# ??? 常识库标签匹配机制优化

## ?? 问题分析

### 当前问题

1. **关键词自动提取误判率高**:
   - 2-4字分词不准确
   - 产生大量噪音词
   - 用户无法控制

2. **关键词无法导出导入**:
   - 只保存在内存中
   - 导出格式不包含关键词
   - 无法备份和分享

3. **匹配逻辑复杂**:
   - Jaccard相似度 + 标签匹配
   - 用户难以理解评分
   - 调试困难

---

## ?? 解决方案

### 方案：标签优先 + 可选关键词

**核心思路**:
- ? **默认只使用标签匹配** - 简单可靠
- ? **保留关键词功能** - 高级用户可选
- ? **不自动提取关键词** - 避免误判
- ? **关键词可导出导入** - 便于管理

---

## ?? 实施方案

### Step 1: 修改评分逻辑

**新的评分公式**:

```csharp
// 基础分
baseScore = importance * 0.05

// 标签匹配（主要）
if (tag匹配上下文):
    tagScore = 1.0
else:
    tagScore = 0

// 关键词匹配（可选）
if (keywords.Count > 0):  // 只有手动设置了关键词才参与
    keywordScore = Jaccard(keywords, contextKeywords)
else:
    keywordScore = 0

// 综合评分
totalScore = baseScore + (tagScore * importance * TagWeight) 
           + (keywordScore * importance * KeywordWeight)
```

**权重默认值**:
```
TagWeight = 0.8      // 标签权重80%
KeywordWeight = 0.2  // 关键词权重20%（仅当用户手动设置时）
```

---

### Step 2: 修改导出格式

**当前格式**:
```
[标签]内容
```

**新格式**（可选关键词）:
```
[标签]内容
[标签]内容 #关键词1,关键词2,关键词3
[标签]内容
```

**示例**:
```
[规则]回复控制在80字以内
[角色]Alice擅长建造 #Alice,建造,建筑,construction
[世界观]这是边缘世界
```

---

### Step 3: UI改进

**常识库管理器**:

```
┌─────────────────────────────────────────────┐
│ 标签: [角色]                                │
│ 内容: Alice擅长建造                         │
│ 重要性: 0.8 [━━━━━━━━━━]                    │
│                                             │
│ 关键词（可选）:                             │
│ ┌─────────────────────────────────────────┐ │
│ │ Alice,建造,建筑,construction             │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│ ? 自动提取关键词（不推荐）                  │
│                                             │
│ 说明:                                       │
│ ? 留空则只使用标签匹配（推荐）              │
│ ? 手动添加关键词可提高匹配精度              │
│ ? 用逗号分隔多个关键词                      │
└─────────────────────────────────────────────┘
```

---

### Step 4: 代码实现

#### 4.1 CommonKnowledgeEntry

```csharp
public class CommonKnowledgeEntry : IExposable
{
    public string id;
    public string tag;
    public string content;
    public float importance;
    public List<string> keywords;  // 保留，但不自动填充
    public bool isEnabled;
    public bool autoExtractKeywords;  // 是否自动提取（默认false）
    
    public CommonKnowledgeEntry(string tag, string content) : this()
    {
        this.tag = tag;
        this.content = content;
        // 不再自动提取关键词
    }
    
    /// <summary>
    /// 计算评分（标签优先）
    /// </summary>
    public float CalculateRelevanceScore(List<string> contextKeywords)
    {
        if (!isEnabled) return 0f;
        
        float baseScore = importance * 0.05f;
        
        if (contextKeywords == null || contextKeywords.Count == 0)
            return baseScore;
        
        // 1. 标签匹配（主要）
        float tagScore = 0f;
        if (!string.IsNullOrEmpty(tag))
        {
            foreach (var keyword in contextKeywords)
            {
                if (tag.Equals(keyword, StringComparison.OrdinalIgnoreCase) ||
                    tag.Contains(keyword) || keyword.Contains(tag))
                {
                    tagScore = 1.0f;
                    break;
                }
            }
        }
        
        // 2. 关键词匹配（可选，仅当用户手动设置时）
        float keywordScore = 0f;
        if (keywords != null && keywords.Count > 0)
        {
            var matchedKeywords = keywords.Intersect(contextKeywords, 
                StringComparer.OrdinalIgnoreCase).ToList();
            int matchCount = matchedKeywords.Count;
            var union = keywords.Union(contextKeywords, 
                StringComparer.OrdinalIgnoreCase).Count();
            
            if (union > 0)
            {
                keywordScore = (float)matchCount / union;
            }
        }
        
        // 综合评分
        float tagPart = tagScore * importance * KnowledgeWeights.TagWeight;
        float keywordPart = keywordScore * importance * KnowledgeWeights.KeywordWeight;
        
        return baseScore + tagPart + keywordPart;
    }
}
```

#### 4.2 导出/导入

```csharp
public string FormatForExport()
{
    var sb = new StringBuilder();
    sb.Append($"[{tag}]{content}");
    
    // 如果有关键词，导出关键词
    if (keywords != null && keywords.Count > 0)
    {
        sb.Append(" #");
        sb.Append(string.Join(",", keywords));
    }
    
    return sb.ToString();
}

private CommonKnowledgeEntry ParseLine(string line)
{
    // 查找标签
    int tagStart = line.IndexOf('[');
    int tagEnd = line.IndexOf(']');
    
    if (tagStart == -1 || tagEnd == -1)
        return new CommonKnowledgeEntry("通用", line);
    
    string tag = line.Substring(tagStart + 1, tagEnd - tagStart - 1).Trim();
    
    // 查找关键词（#后面的内容）
    int keywordStart = line.IndexOf('#', tagEnd);
    string content;
    List<string> keywords = new List<string>();
    
    if (keywordStart >= 0)
    {
        // 有关键词
        content = line.Substring(tagEnd + 1, keywordStart - tagEnd - 1).Trim();
        string keywordStr = line.Substring(keywordStart + 1).Trim();
        keywords = keywordStr.Split(',')
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrEmpty(k))
            .ToList();
    }
    else
    {
        // 无关键词
        content = line.Substring(tagEnd + 1).Trim();
    }
    
    var entry = new CommonKnowledgeEntry(tag, content);
    entry.keywords = keywords;
    return entry;
}
```

---

## ?? 效果对比

### 场景1: 纯标签匹配（推荐）

**常识**:
```
[规则]回复控制在80字以内
[角色]Alice擅长建造
[世界观]这是边缘世界
```

**上下文关键词**: Alice, 建造, 工作

**匹配结果**:
```
? [角色]Alice擅长建造 - 标签"角色"未匹配，但内容包含"Alice"（0.05）
? 实际上这条不应该匹配！
```

**问题**: 纯标签匹配可能遗漏相关常识

---

### 场景2: 标签 + 手动关键词（推荐）

**常识**:
```
[规则]回复控制在80字以内
[角色]Alice擅长建造 #Alice,建造,建筑,construction
[世界观]这是边缘世界
```

**上下文关键词**: Alice, 建造, 工作

**匹配结果**:
```
? [角色]Alice擅长建造 - 关键词匹配(Alice,建造) + 标签(0) = 高分
```

**效果**: 精准匹配！

---

### 场景3: 标签精准设计（最佳实践）

**常识**:
```
[Alice]擅长建造
[Bob]擅长烹饪
[规则]回复控制在80字以内
```

**上下文关键词**: Alice, 建造, 工作

**匹配结果**:
```
? [Alice]擅长建造 - 标签精准匹配！
```

**效果**: 最简单可靠！

---

## ?? 使用建议

### ? 推荐做法

#### 1. 使用精准标签（最简单）

```
错误: [角色]Alice擅长建造
正确: [Alice]擅长建造

错误: [规则]回复限制
正确: [字数限制]回复控制在80字以内
```

**优点**:
- ? 无需关键词
- ? 匹配精准
- ? 易于管理

#### 2. 手动添加关键词（更精准）

```
[角色]Alice擅长建造 #Alice,建造,建筑,construction,build
[规则]回复限制 #字数,80字,简洁,限制
```

**优点**:
- ? 提高匹配率
- ? 覆盖同义词
- ? 可导出导入

#### 3. 避免自动提取

```
? 不推荐: 启用"自动提取关键词"
? 推荐: 手动指定或留空
```

**原因**:
- 自动提取误判率高
- 产生大量噪音
- 难以控制

---

### ? 避免陷阱

#### 1. 标签太宽泛

```
错误: [规则]所有回复都要简洁明了控制字数
     标签"规则"太宽泛，匹配所有包含"规则"的场景
     
正确: [字数限制]回复控制在80字以内
     标签精准，只匹配字数相关场景
```

#### 2. 关键词太多

```
错误: #回复,控制,字数,以内,简洁,明了,不要,延伸,话题,... (20个)
     关键词过多，增加误匹配

正确: #字数,80字,简洁,限制 (4-5个核心词)
     精准覆盖核心概念
```

#### 3. 重复信息

```
错误: [Alice]Alice擅长建造 #Alice,Alice,alice
     重复的信息

正确: [Alice]擅长建造 #建造,建筑,construction
     避免重复，添加同义词
```

---

## ?? 迁移指南

### 从旧版本迁移

**旧格式**（自动关键词）:
```
[规则]回复控制在80字以内
[角色]Alice擅长建造
```

**新格式**（可选关键词）:
```
[字数限制]回复控制在80字以内 #字数,80字,简洁
[Alice]擅长建造 #建造,建筑,construction
```

**迁移步骤**:

1. **导出现有常识**:
   ```
   常识库管理 → 导出到文件
   ```

2. **手动优化标签**:
   ```
   [规则] → [字数限制]
   [角色] → [Alice]
   ```

3. **添加关键词**（可选）:
   ```
   #字数,80字,简洁
   #建造,建筑,construction
   ```

4. **重新导入**:
   ```
   常识库管理 → 导入文本
   ```

---

## ?? 配置建议

### 权重配置

```
标签权重: 80% (主要)
关键词权重: 20% (辅助)
基础分系数: 0.05
```

**适用场景**: 标签精准设计 + 少量关键词

### 严格模式

```
标签权重: 100%
关键词权重: 0%
基础分系数: 0.03
```

**适用场景**: 只使用标签，不使用关键词

### 宽松模式

```
标签权重: 60%
关键词权重: 40%
基础分系数: 0.08
```

**适用场景**: 大量使用关键词，标签作为分类

---

## ?? 总结

### 核心改进

1. **标签优先** - 主要匹配机制
2. **可选关键词** - 高级功能，用户可选
3. **不自动提取** - 避免误判
4. **可导出导入** - 便于管理

### 使用建议

1. **优先使用精准标签** - 最简单可靠
2. **必要时添加关键词** - 提高匹配率
3. **避免自动提取** - 保持干净

### 迁移路径

```
旧版本 → 导出 → 优化标签 → 添加关键词（可选）→ 导入
```

---

**现在常识库更简单、更可靠、更可控！** ??
