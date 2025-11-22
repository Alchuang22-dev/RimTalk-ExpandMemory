# ?? 常识库权重配置系统 - 实施文档

## ?? 当前状况

### 现有问题

**记忆系统**：? 有完整的权重配置
```csharp
public static class Weights
{
    public static float TimeDecay = 0.3f;
    public static float Importance = 0.3f;
    public static float KeywordMatch = 0.4f;
    public static float LayerBonus = 0.2f;
    public static float PinnedBonus = 0.5f;
    public static float UserEditedBonus = 0.3f;
}
```

**常识库**：? 没有权重配置，硬编码！
```csharp
// 硬编码在 CalculateRelevanceScore 中：
float baseScore = importance * 0.03f;           // 固定 3%
float jaccardScore = ... * 0.7f;                // 固定 70%
float tagScore = ... * 0.3f;                    // 固定 30%
float matchCountBonus = matchCount * 0.08f;     // 固定每个0.08
float matchScore = (jaccardScore * 0.7f + tagScore * 0.3f) * importance;
```

---

## ?? 解决方案

### 方案A：创建KnowledgeWeights类（推荐）???

**优点**：
- ? 与记忆系统一致
- ? 易于调整和测试
- ? 支持UI配置

**实现**：

```csharp
namespace RimTalk.Memory
{
    /// <summary>
    /// 常识库评分权重配置
    /// </summary>
    public static class KnowledgeWeights
    {
        // 基础权重
        public static float BaseScore = 0.03f;          // 基础分系数（重要性 * 0.03）
        
        // 匹配权重
        public static float JaccardWeight = 0.7f;       // Jaccard相似度权重
        public static float TagWeight = 0.3f;           // 标签匹配权重
        
        // 加分权重
        public static float MatchCountBonus = 0.08f;    // 每个匹配关键词加分
        
        /// <summary>
        /// 从设置中加载权重
        /// </summary>
        public static void LoadFromSettings(RimTalkMemoryPatchSettings settings)
        {
            if (settings == null) return;
            
            BaseScore = settings.knowledgeBaseScore;
            JaccardWeight = settings.knowledgeJaccardWeight;
            TagWeight = settings.knowledgeTagWeight;
            MatchCountBonus = settings.knowledgeMatchBonus;
        }
        
        /// <summary>
        /// 重置为默认值
        /// </summary>
        public static void ResetToDefault()
        {
            BaseScore = 0.03f;
            JaccardWeight = 0.7f;
            TagWeight = 0.3f;
            MatchCountBonus = 0.08f;
        }
    }
}
```

---

## ?? 实施步骤

### Step 1: 添加设置字段

**文件**：`Source/RimTalkSettings.cs`

```csharp
public class RimTalkMemoryPatchSettings : ModSettings
{
    // ... 现有字段 ...
    
    // === 常识库权重配置 ===
    public float knowledgeBaseScore = 0.03f;         // 基础分系数
    public float knowledgeJaccardWeight = 0.7f;      // Jaccard权重
    public float knowledgeTagWeight = 0.3f;          // 标签权重
    public float knowledgeMatchBonus = 0.08f;        // 匹配加分
    
    public override void ExposeData()
    {
        // ... 现有保存 ...
        
        // 常识库权重
        Scribe_Values.Look(ref knowledgeBaseScore, "knowledge_baseScore", 0.03f);
        Scribe_Values.Look(ref knowledgeJaccardWeight, "knowledge_jaccardWeight", 0.7f);
        Scribe_Values.Look(ref knowledgeTagWeight, "knowledge_tagWeight", 0.3f);
        Scribe_Values.Look(ref knowledgeMatchBonus, "knowledge_matchBonus", 0.08f);
    }
}
```

---

### Step 2: 创建KnowledgeWeights类

**文件**：`Source/Memory/CommonKnowledgeLibrary.cs`（在文件末尾添加）

```csharp
namespace RimTalk.Memory
{
    /// <summary>
    /// 常识库评分权重配置
    /// </summary>
    public static class KnowledgeWeights
    {
        public static float BaseScore = 0.03f;
        public static float JaccardWeight = 0.7f;
        public static float TagWeight = 0.3f;
        public static float MatchCountBonus = 0.08f;
        
        public static void LoadFromSettings(RimTalk.MemoryPatch.RimTalkMemoryPatchSettings settings)
        {
            if (settings == null) return;
            
            BaseScore = settings.knowledgeBaseScore;
            JaccardWeight = settings.knowledgeJaccardWeight;
            TagWeight = settings.knowledgeTagWeight;
            MatchCountBonus = settings.knowledgeMatchBonus;
        }
        
        public static void ResetToDefault()
        {
            BaseScore = 0.03f;
            JaccardWeight = 0.7f;
            TagWeight = 0.3f;
            MatchCountBonus = 0.08f;
        }
    }
}
```

---

### Step 3: 修改评分方法使用权重

**文件**：`Source/Memory/CommonKnowledgeLibrary.cs`

```csharp
public float CalculateRelevanceScore(List<string> contextKeywords)
{
    if (!isEnabled)
        return 0f;

    // 使用权重配置 ?
    float baseScore = importance * KnowledgeWeights.BaseScore;

    if (contextKeywords == null || contextKeywords.Count == 0)
        return baseScore;

    if (keywords == null || keywords.Count == 0)
        return baseScore;

    // 计算关键词匹配
    var matchedKeywords = keywords.Intersect(contextKeywords).ToList();
    int matchCount = matchedKeywords.Count;
    var union = keywords.Union(contextKeywords).Count();
    float jaccardScore = union > 0 ? (float)matchCount / union : 0f;

    // 检查标签是否匹配
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

    // 使用权重配置 ?
    float matchCountBonus = matchCount * KnowledgeWeights.MatchCountBonus;

    // 使用权重配置 ?
    float matchScore = (jaccardScore * KnowledgeWeights.JaccardWeight + 
                        tagScore * KnowledgeWeights.TagWeight) * importance;
    float totalScore = baseScore + matchScore + matchCountBonus;

    return totalScore;
}
```

同步更新`CalculateRelevanceScoreWithDetails`方法。

---

### Step 4: 添加UI配置

**文件**：`Source/RimTalkSettings.cs`

```csharp
private void DrawDynamicInjectionSettings(Listing_Standard listing)
{
    // ... 现有记忆权重配置 ...
    
    listing.Gap();
    listing.GapLine();
    
    // === 常识库权重配置 ===
    Text.Font = GameFont.Tiny;
    GUI.color = new Color(1f, 1f, 0.8f);
    listing.Label("常识库评分权重:");
    GUI.color = Color.white;
    Text.Font = GameFont.Small;
    
    listing.Label($"  基础分系数: {knowledgeBaseScore:F2}");
    knowledgeBaseScore = listing.Slider(knowledgeBaseScore, 0.01f, 0.1f);
    
    listing.Label($"  Jaccard相似度权重: {knowledgeJaccardWeight:P0}");
    knowledgeJaccardWeight = listing.Slider(knowledgeJaccardWeight, 0f, 1f);
    
    listing.Label($"  标签匹配权重: {knowledgeTagWeight:P0}");
    knowledgeTagWeight = listing.Slider(knowledgeTagWeight, 0f, 1f);
    
    listing.Label($"  匹配数量加分: {knowledgeMatchBonus:F2}");
    knowledgeMatchBonus = listing.Slider(knowledgeMatchBonus, 0.01f, 0.2f);
    
    // 应用权重
    KnowledgeWeights.LoadFromSettings(this);
}
```

---

### Step 5: 初始化权重

**文件**：`Source/RimTalkSettings.cs` 或 `Source/Memory/MemoryManager.cs`

```csharp
// 在Mod加载时初始化
[StaticConstructorOnStartup]
public static class KnowledgeWeightsInitializer
{
    static KnowledgeWeightsInitializer()
    {
        var settings = RimTalkMemoryPatchMod.Settings;
        if (settings != null)
        {
            KnowledgeWeights.LoadFromSettings(settings);
            Log.Message("[Knowledge] Weights loaded from settings");
        }
    }
}
```

---

## ?? 默认权重说明

### 当前硬编码值

| 参数 | 值 | 说明 |
|------|-----|------|
| baseScore系数 | 0.03 | 重要性的3%作为基础分 |
| Jaccard权重 | 0.7 | Jaccard相似度占70% |
| 标签权重 | 0.3 | 标签匹配占30% |
| 匹配加分 | 0.08 | 每个匹配关键词+0.08分 |

### 调整建议

**提高匹配精度**：
```
Jaccard权重: 0.7 → 0.8
标签权重: 0.3 → 0.2
匹配加分: 0.08 → 0.10
```

**降低门槛（更容易匹配）**：
```
基础分系数: 0.03 → 0.05
匹配加分: 0.08 → 0.12
```

**重视标签**：
```
Jaccard权重: 0.7 → 0.5
标签权重: 0.3 → 0.5
```

---

## ?? 使用场景

### 场景1：严格匹配模式

```
配置：
- 基础分系数: 0.02（降低）
- Jaccard权重: 0.8（提高）
- 标签权重: 0.2（降低）
- 匹配加分: 0.10（提高）

效果：
- 只有高度相关的常识被注入
- 减少噪音
- 适合精准控制
```

### 场景2：宽松匹配模式

```
配置：
- 基础分系数: 0.05（提高）
- Jaccard权重: 0.6（降低）
- 标签权重: 0.4（提高）
- 匹配加分: 0.06（降低）

效果：
- 更多常识被注入
- 适合探索性对话
- 丰富上下文
```

### 场景3：标签优先模式

```
配置：
- Jaccard权重: 0.5
- 标签权重: 0.5（相等）
- 匹配加分: 0.08

效果：
- 标签和内容同等重要
- 适合分类管理的常识库
```

---

## ? 优点

1. **灵活性**：用户可自定义权重
2. **一致性**：与记忆系统风格统一
3. **可测试**：方便A/B测试不同配置
4. **可扩展**：未来可添加更多权重

---

## ?? 实施优先级

| 步骤 | 优先级 | 工作量 | 影响 |
|------|--------|--------|------|
| Step 1: 添加设置字段 | ??? | 5分钟 | 必需 |
| Step 2: 创建Weights类 | ??? | 10分钟 | 必需 |
| Step 3: 修改评分方法 | ??? | 15分钟 | 必需 |
| Step 4: 添加UI | ?? | 20分钟 | 推荐 |
| Step 5: 初始化 | ? | 5分钟 | 可选 |

**总工作量**：~55分钟

---

## ?? 总结

### 修改前

```csharp
// 硬编码，无法调整
float baseScore = importance * 0.03f;
float matchScore = (jaccardScore * 0.7f + tagScore * 0.3f) * importance;
float matchCountBonus = matchCount * 0.08f;
```

### 修改后

```csharp
// 可配置，灵活调整
float baseScore = importance * KnowledgeWeights.BaseScore;
float matchScore = (jaccardScore * KnowledgeWeights.JaccardWeight + 
                    tagScore * KnowledgeWeights.TagWeight) * importance;
float matchCountBonus = matchCount * KnowledgeWeights.MatchBonus;
```

**用户可以在UI中调整权重，无需修改代码！** ??
