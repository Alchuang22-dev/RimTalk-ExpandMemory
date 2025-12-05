# Prompt Caching 实现指南（模型端缓存）

## ?? **概述**

**Prompt Caching** 是AI模型提供商（如OpenAI、DeepSeek）提供的显式缓存功能，可以将重复的prompt前缀缓存到模型端，从而：

- ? **降低50%费用**：缓存命中的token按50%价格计费
- ? **提升响应速度**：跳过重复token的处理
- ? **自动管理**：模型端自动维护缓存（有效期5-10分钟）

---

## ?? **核心思路**

### 当前prompt结构
```
完整prompt = 固定system指令 + 变化的记忆/常识 + 用户输入
```

### 优化后结构
```
system消息（可缓存） = 固定指令（如角色设定、规则）
user消息（不缓存） = 记忆注入 + 常识注入 + 用户对话
```

**效果**：
- 每个殖民者的固定指令只在首次调用时计费
- 后续对话（5-10分钟内）缓存命中，费用降低50%

---

## ?? **实现方案**

### 方案1：拆分system和user（推荐）

修改 `IndependentAISummarizer.cs` 的 `BuildJsonRequest()` 方法：

```csharp
// ? v3.3.4: Prompt Caching优化
private static string BuildJsonRequest(string prompt)
{
    // Google提供商保持原样
    if (provider == "Google")
    {
        // ... 现有代码 ...
    }
    
    // OpenAI/DeepSeek：拆分system和user
    StringBuilder sb = new StringBuilder();
    
    // 1. 提取固定的system部分
    string systemPrompt = "你是一个RimWorld殖民地的角色扮演AI。\\n" +
                         "请用极简的语言总结记忆内容。\\n" +
                         "只输出总结文字，不要其他格式。";
    
    // 2. 动态内容（记忆列表）作为user部分
    string userPrompt = prompt.Replace("\\", "\\\\")
                              .Replace("\"", "\\\"")
                              .Replace("\n", "\\n")
                              .Replace("\r", "")
                              .Replace("\t", "\\t");
    
    sb.Append("{");
    sb.Append("\"model\":\"" + model + "\",");
    sb.Append("\"messages\":[");
    
    // 3. system消息（带缓存控制）
    sb.Append("{\"role\":\"system\",");
    sb.Append("\"content\":\"" + systemPrompt + "\"");
    
    // ? 关键：添加缓存控制
    if (provider == "OpenAI")
    {
        // OpenAI Prompt Caching（GPT-4/3.5）
        sb.Append(",\"cache_control\":{\"type\":\"ephemeral\"}");
    }
    else if (provider == "DeepSeek")
    {
        // DeepSeek缓存控制
        sb.Append(",\"cache\":true");
    }
    
    sb.Append("},");
    
    // 4. user消息（变化内容）
    sb.Append("{\"role\":\"user\",");
    sb.Append("\"content\":\"" + userPrompt + "\"");
    sb.Append("}],");
    
    sb.Append("\"temperature\":0.7,");
    sb.Append("\"max_tokens\":200");
    
    // ? DeepSeek全局缓存开关
    if (provider == "DeepSeek")
    {
        sb.Append(",\"enable_prompt_cache\":true");
    }
    
    sb.Append("}");
    return sb.ToString();
}
```

---

### 方案2：标记重复前缀（高级）

如果记忆注入中也有固定部分，可以进一步标记：

```csharp
{
  "model": "gpt-4",
  "messages": [
    {
      "role": "system",
      "content": "你是一个RimWorld AI...",
      "cache_control": {"type": "ephemeral"}  // ?? 固定指令
    },
    {
      "role": "user",
      "content": "【常识】\\n世界观知识...\\n\\n【记忆】\\n...",
      "cache_control": {"type": "ephemeral"}  // ?? 常识部分也可缓存
    },
    {
      "role": "user",
      "content": "[用户对话]"  // ?? 用户输入不缓存
    }
  ]
}
```

**注意**：
- OpenAI限制：只有最后一条system消息可以被缓存
- DeepSeek更灵活：可以标记多条消息

---

## ?? **性能评估**

### 典型场景（GPT-4 Turbo）

| 场景 | 首次调用 | 缓存命中 | 节省 |
|-----|---------|---------|------|
| 记忆总结（200 tokens） | $0.006 | **$0.003** | 50% |
| 对话生成（500 tokens） | $0.015 | **$0.0075** | 50% |

### 每日费用估算（10个殖民者，每人5次对话）

| 缓存状态 | 每日费用 | 月度费用 |
|---------|---------|---------|
| 无缓存 | $0.75 | $22.5 |
| 启用缓存 | **$0.40** | **$12** |
| **节省** | **$0.35/天** | **$10.5/月** |

---

## ?? **注意事项**

### 1. 缓存有效期
- OpenAI：5-10分钟（自动过期）
- DeepSeek：约10分钟
- **建议**：高频对话时效果最佳（如游戏中连续对话）

### 2. 兼容性
- ? OpenAI GPT-4 Turbo、GPT-3.5 Turbo
- ? DeepSeek Chat、Coder
- ? Google Gemini（暂不支持）

### 3. 调试
查看响应头中的缓存信息：
```
X-Cache-Control-Stats: hit=1, miss=0, token_savings=150
```

### 4. 最佳实践
- system prompt保持简洁（<500 tokens）
- 避免频繁修改system内容
- 记忆/常识注入放在user消息中

---

## ?? **测试验证**

### 步骤1：启用DevMode日志
```csharp
if (Prefs.DevMode)
{
    Log.Message($"[AI] Cache enabled: {useCaching}");
    Log.Message($"[AI] System tokens: {systemPrompt.Length / 4}"); // 粗略估算
}
```

### 步骤2：观察API响应
- 首次调用：`usage.cached_tokens = 0`
- 后续调用：`usage.cached_tokens > 0` ?

### 步骤3：费用对比
记录一周的API费用，对比优化前后。

---

## ?? **配置选项（可选）**

在 `RimTalkSettings.cs` 添加开关：

```csharp
public bool enablePromptCaching = true;  // 启用Prompt Caching
```

在设置UI中：
```csharp
listing.CheckboxLabeled("?? 启用Prompt Caching（降低50%费用）", ref enablePromptCaching);
```

---

## ?? **部署建议**

### 阶段1：测试环境
1. 修改 `BuildJsonRequest()`
2. 使用DevMode验证JSON格式
3. 单次API调用测试

### 阶段2：生产环境
1. 关闭DevMode日志
2. 监控缓存命中率
3. 观察费用变化

### 阶段3：优化
1. 调整system prompt长度
2. 分析缓存失效原因
3. 微调缓存策略

---

## ?? **参考文档**

- [OpenAI Prompt Caching](https://platform.openai.com/docs/guides/prompt-caching)
- [DeepSeek API文档](https://platform.deepseek.com/api-docs)
- [本项目issue讨论](https://github.com/sanguodxj-byte/RimTalk-ExpandMemory/issues)

---

## ?? **支持**

如有问题，请提交issue或联系开发者。

**祝您降本增效！** ??
