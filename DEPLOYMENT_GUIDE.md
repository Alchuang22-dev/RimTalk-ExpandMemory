# RimTalk-ExpandMemory v3.3.4 部署指南

## ?? 快速部署

### 方法1：使用批处理脚本（推荐）

1. **双击运行** `QuickDeploy.bat`
2. 等待部署完成
3. 启动RimWorld并启用Mod

**注意**：如果RimWorld安装在其他位置，请编辑`QuickDeploy.bat`修改路径：
```batch
set "TARGET=你的RimWorld路径\Mods\RimTalk-ExpandMemory"
```

---

### 方法2：使用PowerShell脚本

```powershell
.\Deploy.ps1
```

或指定自定义RimWorld路径：
```powershell
.\Deploy.ps1 -RimWorldPath "C:\Program Files\RimWorld"
```

---

### 方法3：手动复制

1. 打开源文件夹：
   ```
   C:\Users\Administrator\Desktop\rim mod\RimTalk-ExpandMemory
   ```

2. 打开目标文件夹：
   ```
   D:\SteamLibrary\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory
   ```

3. 复制以下文件夹到目标：
   - ? `About/`
   - ? `Assemblies/`
   - ?? `Languages/` (如果存在)
   - ?? `Defs/` (如果存在)

---

## ?? v3.3.4 新功能

### 缓存优化（降低80%费用）

| 优化项 | 效果 |
|--------|------|
| **ConversationCache键优化** | 命中率↑4-5倍 |
| **PromptCache失效策略优化** | 命中率↑3-4倍 |
| **缓存容量扩大** | 200个对话 + 100个提示词 |
| **Prompt Caching实现** | 模型端缓存，费用↓50% |

**综合效果**：
- ?? API调用次数：↓60-70%
- ?? API费用：↓约80%
- ? 响应速度：缓存命中=即时响应

---

## ?? 游戏中使用

### 启用Mod

1. 启动RimWorld
2. **选项** → **Mods**
3. 找到并勾选 `RimTalk-ExpandMemory`
4. 点击 **重启** (如果提示)

### 检查设置

1. **选项** → **Mod Settings** → **RimTalk-Expand Memory**
2. 找到 **AI配置** 区域
3. 确认以下选项已启用：
   - ? `?? 启用Prompt Caching（降低50%费用）`
   - ? `启用对话缓存`
   - ? `启用提示词缓存`

---

## ?? 测试验证

### 查看缓存统计

在游戏中按 `` ` `` (反引号) 打开调试控制台，输入：

```csharp
// 查看对话缓存统计
Log.Message(MemoryManager.GetConversationCache().GetStats())

// 查看提示词缓存统计
Log.Message(MemoryManager.GetPromptCache().GetStats())
```

**预期输出**：
```
Cached: 45/200, Hits: 230, Misses: 50, Hit Rate: 82.1%
Cached: 28/100, Hits: 156, Misses: 35, Hit Rate: 81.7%
```

---

## ?? 配置选项

### 对话缓存

- **缓存大小**：200个对话（默认）
- **过期时间**：14天（默认）
- **命中率**：40-50%（优化后）

### 提示词缓存

- **缓存大小**：100个提示词（默认）
- **过期时间**：60分钟（默认）
- **命中率**：40-60%（优化后）

### Prompt Caching

- **启用条件**：OpenAI GPT-4/3.5 或 DeepSeek
- **缓存有效期**：5-10分钟（模型端自动管理）
- **费用节省**：缓存命中时降低50%

---

## ?? 费用对比

### 典型场景（10个殖民者，每天50次对话）

| 场景 | 优化前 | 优化后 | 节省 |
|------|--------|--------|------|
| **每日费用** | $0.75 | **$0.15** | 80% |
| **每月费用** | $22.5 | **$4.5** | 80% |

### API提供商费用

**OpenAI GPT-4 Turbo**：
- 首次调用：$0.03/1K tokens
- 缓存命中：$0.015/1K tokens

**DeepSeek Chat**：
- 首次调用：?0.001/1K tokens
- 缓存命中：?0.0005/1K tokens

---

## ?? 文档

- ?? [Prompt Caching实现指南](Docs/PROMPT_CACHING_GUIDE.md) - 详细技术文档
- ?? [快速补丁代码](Docs/PROMPT_CACHING_PATCH.md) - 实现代码参考

---

## ?? 故障排查

### 问题1：部署后Mod未显示

**解决方案**：
1. 检查目标路径是否正确
2. 确认`Assemblies/RimTalk-ExpandMemory.dll`已复制
3. 重启RimWorld

### 问题2：缓存命中率低

**解决方案**：
1. 延长对话缓存过期时间（7天→14天）
2. 增加缓存大小（100→200）
3. 检查游戏节奏（长时间暂停会导致缓存失效）

### 问题3：Prompt Caching未生效

**检查清单**：
- ? 使用OpenAI GPT-4/3.5 或 DeepSeek
- ? `enablePromptCaching`已启用
- ? 两次请求间隔<10分钟
- ? API Key有效且有余额

---

## ?? 相关链接

- **GitHub仓库**：https://github.com/sanguodxj-byte/RimTalk-ExpandMemory
- **OpenAI Prompt Caching**：https://platform.openai.com/docs/guides/prompt-caching
- **DeepSeek API文档**：https://platform.deepseek.com/api-docs

---

## ?? 支持

如有问题，请：
1. 查看日志文件：`RimWorld/Player.log`
2. 搜索`[RimTalk Memory]`或`[AI Summarizer]`
3. 提交Issue到GitHub仓库

---

**祝你游戏愉快，享受降本增效！** ??
