# ?? 关键词匹配完全失效 - 根本原因分析

## ?? 问题总结

**症状**：
```
婴儿常识（重要性1.0，含"婴儿"关键词）→ 评分 0.9
无关常识（重要性0.5，无关键词）→ 评分 0.5
```

**结论**：评分 = 重要性 × 某个系数，关键词匹配**完全无效**！

---

## ?? 根本原因

### 原因1：调试预览器传递空上下文

**代码位置**：`Dialog_InjectionPreview.cs` 第280-286行

```csharp
knowledgeInjection = library.InjectKnowledgeWithDetails(
    "",  // ?? 传递空字符串！
    settings.maxInjectedKnowledge,
    out knowledgeScores,
    out keywordInfo,
    selectedPawn
);
```

**后果**：
1. `ExtractContextKeywords("")` 返回空列表
2. `contextKeywords.Count == 0`
3. 触发早期返回：`return importance * 0.3f`
4. **完全跳过关键词匹配逻辑！**

---

### 原因2：评分公式的错误逻辑

**代码位置**：`CommonKnowledgeLibrary.cs` 第89-93行

```csharp
if (contextKeywords == null || contextKeywords.Count == 0)
    return importance * 0.3f; // ?? 早期返回

if (keywords == null || keywords.Count == 0)
    return importance * 0.3f;  // ?? 早期返回
```

**问题**：
- 当上下文为空时，直接返回`importance * 0.3`
- **完全跳过**关键词匹配计算
- 导致所有常识只按重要性排序

---

### 原因3：测试数据疑点

**您的测试结果**：
```
婴儿常识（重要性1.0）→ 0.9
无关常识（重要性0.5）→ 0.5
```

**数学分析**：
```
0.9 ≠ 1.0 × 0.3 = 0.3 ?
0.5 = 0.5 × 1.0 = 0.5 ?

推测：
- 婴儿常识重要性可能是 0.9 / 0.3 = 3.0
- 或者有其他加成系数
```

---

## ?? 解决方案

### 方案1：修复调试预览器（推荐）?

**不要传递空上下文，而是构造一个有意义的测试上下文**：

```csharp
// 修改前
knowledgeInjection = library.InjectKnowledgeWithDetails(
    "",  // ? 空上下文
    settings.maxInjectedKnowledge,
    out knowledgeScores,
    out keywordInfo,
    selectedPawn
);

// 修改后
// 构造测试上下文：包含角色名字
string testContext = selectedPawn != null ? selectedPawn.LabelShort : "";
knowledgeInjection = library.InjectKnowledgeWithDetails(
    testContext,  // ? 有效上下文
    settings.maxInjectedKnowledge,
    out knowledgeScores,
    out keywordInfo,
    selectedPawn
);
```

**效果**：
- 提取角色名字作为关键词
- 触发正常的关键词匹配逻辑
- 评分会考虑关键词匹配度

---

### 方案2：优化评分公式（强烈推荐）???

**问题**：当前公式过度依赖重要性

```csharp
// 当前公式（错误）
if (contextKeywords.Count == 0)
    return importance * 0.3f;  // 只看重要性

// 正常公式
float score = (jaccardScore * 0.7f + tagScore * 0.3f) * importance;
```

**改进方案**：

#### 选项A：移除早期返回（激进）

```csharp
public float CalculateRelevanceScore(List<string> contextKeywords)
{
    if (!isEnabled)
        return 0f;

    // ? 删除这些早期返回
    // if (contextKeywords == null || contextKeywords.Count == 0)
    //     return importance * 0.3f;

    // 继续正常计算
    float jaccardScore = 0f;
    if (contextKeywords != null && contextKeywords.Count > 0 && 
        keywords != null && keywords.Count > 0)
    {
        var intersection = keywords.Intersect(contextKeywords).Count();
        var union = keywords.Union(contextKeywords).Count();
        jaccardScore = union > 0 ? (float)intersection / union : 0f;
    }

    // 即使无匹配，也按正常公式计算
    float tagScore = 0f;
    // ... 标签匹配逻辑 ...

    float score = (jaccardScore * 0.7f + tagScore * 0.3f) * importance;
    return score;
}
```

**效果**：
```
婴儿常识（无匹配）：
- jaccardScore = 0
- tagScore = 0
- score = (0×0.7 + 0×0.3) × 1.0 = 0 ?? 太低！

无关常识（无匹配）：
- score = 0

结果：所有常识评分都是0！?
```

---

#### 选项B：保留基础分但降低权重（温和）?

```csharp
public float CalculateRelevanceScore(List<string> contextKeywords)
{
    if (!isEnabled)
        return 0f;

    // 计算关键词匹配
    float jaccardScore = 0f;
    float tagScore = 0f;
    
    if (contextKeywords != null && contextKeywords.Count > 0)
    {
        if (keywords != null && keywords.Count > 0)
        {
            var intersection = keywords.Intersect(contextKeywords).Count();
            var union = keywords.Union(contextKeywords).Count();
            jaccardScore = union > 0 ? (float)intersection / union : 0f;
        }
        
        // 标签匹配
        if (!string.IsNullOrEmpty(tag))
        {
            foreach (var keyword in contextKeywords)
            {
                if (tag.Contains(keyword) || keyword.Contains(tag))
                {
                    tagScore = 0.3f;
                    break;
                }
            }
        }
    }
    
    // 综合评分：关键词匹配70% + 标签匹配30% + 基础分10%
    float matchScore = (jaccardScore * 0.7f + tagScore * 0.3f);
    float baseScore = 0.1f; // 基础分，确保重要常识不会被完全忽略
    
    float score = (matchScore * 0.9f + baseScore * 0.1f) * importance;
    return score;
}
```

**效果**：
```
婴儿常识（重要性1.0，无匹配）：
- matchScore = 0
- baseScore = 0.1
- score = (0×0.9 + 0.1×0.1) × 1.0 = 0.01 ? 有底线

婴儿常识（重要性1.0，匹配"婴儿"）：
- jaccardScore = 1/50 = 0.02
- matchScore = 0.02×0.7 = 0.014
- score = (0.014×0.9 + 0.1×0.1) × 1.0 = 0.023 ? 略高于无匹配
```

---

#### 选项C：匹配加权（最佳）???

```csharp
public float CalculateRelevanceScore(List<string> contextKeywords)
{
    if (!isEnabled)
        return 0f;

    // 计算关键词匹配
    float jaccardScore = 0f;
    float tagScore = 0f;
    int matchCount = 0;
    
    if (contextKeywords != null && contextKeywords.Count > 0 && 
        keywords != null && keywords.Count > 0)
    {
        var matchedKeywords = keywords.Intersect(contextKeywords).ToList();
        matchCount = matchedKeywords.Count;
        var union = keywords.Union(contextKeywords).Count();
        jaccardScore = union > 0 ? (float)matchCount / union : 0f;
        
        // 标签匹配
        if (!string.IsNullOrEmpty(tag))
        {
            foreach (var keyword in contextKeywords)
            {
                if (tag.Contains(keyword) || keyword.Contains(tag))
                {
                    tagScore = 0.3f;
                    break;
                }
            }
        }
    }
    
    // 匹配加权：有匹配时提升权重
    float matchBonus = matchCount > 0 ? 1.5f : 1.0f;
    
    // 基础分：确保重要常识有最低分
    float baseScore = importance * 0.05f;
    
    // 匹配分
    float matchScore = (jaccardScore * 0.8f + tagScore * 0.2f) * importance * matchBonus;
    
    // 综合评分
    float score = Math.Max(baseScore, matchScore);
    return score;
}
```

**效果**：
```
婴儿常识（重要性1.0，无匹配）：
- matchCount = 0
- matchBonus = 1.0
- matchScore = 0
- baseScore = 1.0 × 0.05 = 0.05
- score = max(0.05, 0) = 0.05 ? 有底线

婴儿常识（重要性1.0，匹配"婴儿"1个）：
- matchCount = 1
- matchBonus = 1.5 ?
- jaccardScore = 1/50 = 0.02
- matchScore = (0.02×0.8) × 1.0 × 1.5 = 0.024
- score = max(0.05, 0.024) = 0.05
  ?? 还是不够！因为Jaccard太小

改进：增加匹配数量加成
- matchBonus = 1.0 + matchCount × 0.3
  = 1.0 + 1 × 0.3 = 1.3
- matchScore = 0.016 × 1.3 = 0.021
  
进一步改进：直接加上匹配数量分
- matchCountScore = matchCount × 0.05
  = 1 × 0.05 = 0.05
- score = baseScore + matchScore + matchCountScore
  = 0.05 + 0.016 + 0.05 = 0.116 ? 高于阈值0.10！
```

---

### 方案3：最终推荐算法

```csharp
public float CalculateRelevanceScore(List<string> contextKeywords)
{
    if (!isEnabled)
        return 0f;

    // 1. 计算基础分（确保重要常识不会太低）
    float baseScore = importance * 0.03f;

    // 2. 如果无上下文，只返回基础分
    if (contextKeywords == null || contextKeywords.Count == 0)
        return baseScore;

    if (keywords == null || keywords.Count == 0)
        return baseScore;

    // 3. 计算关键词匹配
    var matchedKeywords = keywords.Intersect(contextKeywords).ToList();
    int matchCount = matchedKeywords.Count;
    var union = keywords.Union(contextKeywords).Count();
    float jaccardScore = union > 0 ? (float)matchCount / union : 0f;

    // 4. 计算标签匹配
    float tagScore = 0f;
    if (!string.IsNullOrEmpty(tag))
    {
        foreach (var keyword in contextKeywords)
        {
            if (tag.Contains(keyword) || keyword.Contains(tag))
            {
                tagScore = 0.3f;
                break;
            }
        }
    }

    // 5. 匹配数量直接加分（每个匹配+0.05）
    float matchCountBonus = matchCount * 0.05f;

    // 6. 综合评分
    float matchScore = (jaccardScore * 0.7f + tagScore * 0.3f) * importance;
    float totalScore = baseScore + matchScore + matchCountBonus;

    return totalScore;
}
```

**效果**：
```
场景1：婴儿常识（重要性1.0），婴儿角色（含"婴儿"关键词）
- baseScore = 1.0 × 0.03 = 0.03
- matchCount = 1 (婴儿)
- jaccardScore = 1/50 = 0.02
- matchScore = (0.02×0.7) × 1.0 = 0.014
- matchCountBonus = 1 × 0.05 = 0.05
- totalScore = 0.03 + 0.014 + 0.05 = 0.094
  ?? 还是低于0.10！

调整：提高matchCountBonus
- matchCountBonus = matchCount × 0.08
  = 1 × 0.08 = 0.08
- totalScore = 0.03 + 0.014 + 0.08 = 0.124 ? 高于0.10！

场景2：无关常识（重要性0.5），婴儿角色
- baseScore = 0.5 × 0.03 = 0.015
- matchCount = 0
- matchScore = 0
- matchCountBonus = 0
- totalScore = 0.015 ? 低于0.10，不注入 ?正确！
```

---

## ?? 实施步骤

### 步骤1：修复调试预览器

```csharp
// Dialog_InjectionPreview.cs
string testContext = selectedPawn?.LabelShort ?? "";
knowledgeInjection = library.InjectKnowledgeWithDetails(
    testContext,  // 使用角色名作为上下文
    settings.maxInjectedKnowledge,
    out knowledgeScores,
    out keywordInfo,
    selectedPawn
);
```

### 步骤2：优化评分算法

```csharp
// CommonKnowledgeLibrary.cs
public float CalculateRelevanceScore(List<string> contextKeywords)
{
    if (!isEnabled)
        return 0f;

    float baseScore = importance * 0.03f;

    if (contextKeywords == null || contextKeywords.Count == 0)
        return baseScore;

    if (keywords == null || keywords.Count == 0)
        return baseScore;

    var matchedKeywords = keywords.Intersect(contextKeywords).ToList();
    int matchCount = matchedKeywords.Count;
    
    // Jaccard相似度
    var union = keywords.Union(contextKeywords).Count();
    float jaccardScore = union > 0 ? (float)matchCount / union : 0f;

    // 标签匹配
    float tagScore = 0f;
    if (!string.IsNullOrEmpty(tag))
    {
        foreach (var keyword in contextKeywords)
        {
            if (tag.Contains(keyword) || keyword.Contains(tag))
            {
                tagScore = 0.3f;
                break;
            }
        }
    }

    // 匹配数量加分（关键！）
    float matchCountBonus = matchCount * 0.08f;

    // 综合评分
    float matchScore = (jaccardScore * 0.7f + tagScore * 0.3f) * importance;
    float totalScore = baseScore + matchScore + matchCountBonus;

    return totalScore;
}
```

### 步骤3：同步更新详细评分方法

```csharp
public KnowledgeScoreDetail CalculateRelevanceScoreWithDetails(List<string> contextKeywords)
{
    // ... 与上面相同的逻辑 ...
    // 记录matchCountBonus到detail
    detail.MatchCountBonus = matchCountBonus;
    detail.TotalScore = baseScore + matchScore + matchCountBonus;
    
    return detail;
}
```

---

## ?? 预期效果

### 修复后的评分

```
婴儿常识（重要性1.0），婴儿角色
├─ baseScore: 0.03
├─ matchCount: 1 (婴儿)
├─ jaccardScore: 0.02
├─ matchScore: 0.014
├─ matchCountBonus: 0.08 ?
└─ totalScore: 0.124 ? 高于阈值0.10

无关常识（重要性0.5），婴儿角色
├─ baseScore: 0.015
├─ matchCount: 0
├─ matchScore: 0
├─ matchCountBonus: 0
└─ totalScore: 0.015 ? 低于阈值0.10

结果：婴儿常识被优先选中 ?
```

---

## ? 总结

### 问题根源
1. ? 调试预览器传递空上下文
2. ? 评分算法过度依赖Jaccard相似度
3. ? Jaccard相似度在小样本下太低

### 解决方案
1. ? 传递有效上下文（角色名）
2. ? 添加匹配数量直接加分
3. ? 保留基础分确保重要性不被忽略

### 关键改进
- **匹配数量加分**：每个匹配+0.08分
- **基础分**：重要性×0.03
- **Jaccard相似度**：保留但权重降低

**这样即使只匹配1个关键词，也能获得足够的分数！** ??
