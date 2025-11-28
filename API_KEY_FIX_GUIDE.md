# ?? API配置问题修复指南

## ? **已修复的问题**

### **问题1：切换提供商后Key混用**

**之前：**
```
1. 填入 flo_xxx（其他AI的Key）
2. 切换到 DeepSeek
3. Key还是 flo_xxx ? 错误！
4. DeepSeek报401错误
```

**现在：**
```
1. 填入 flo_xxx
2. 切换到 DeepSeek
3. Key自动清空 ? 安全！
4. 强制用户重新填写正确的DeepSeek Key
```

---

### **问题2：Key格式无法验证**

**之前：**
- 用户填错Key格式也看不出来
- 只有调用API时才报401错误

**现在：**
- ? 实时验证Key格式
- ? 立即显示错误提示
- ?? 提示正确格式

---

## ?? **新的使用流程**

### **步骤1：选择配置模式**

#### **模式A：跟随RimTalk（推荐）**

```
? 优先使用RimTalk的AI配置

说明：
?? 将优先使用RimTalk的API配置
?? 如果RimTalk未配置，则使用下方的独立配置
?? 建议：直接在RimTalk Mod设置中配置API
```

**适用场景：** 
- 已经在RimTalk中配置了API
- 想要统一管理API配置

**配置步骤：**
1. 打开RimTalk Mod设置
2. 配置你的API Key（DeepSeek推荐）
3. ExpandMemory自动跟随

---

#### **模式B：独立配置**

```
? 优先使用RimTalk的API配置

说明：
?? 使用独立API配置（不跟随RimTalk）
?? 需要在下方手动配置提供商和API Key
```

**适用场景：**
- 想为ExpandMemory单独配置API
- RimTalk使用A提供商，ExpandMemory使用B提供商

**配置步骤：** 见下方

---

### **步骤2：配置独立API（模式B）**

#### **2.1 选择提供商**

```
提供商: [OpenAI] [DeepSeek ?] [Google]
```

**?? 重要提示：**
- 切换提供商时，**API Key会自动清空**
- 这是为了防止混用不同提供商的Key
- 请重新填写正确的Key

---

#### **2.2 填写API Key**

```
API Key: [输入框]

实时验证：
? Key格式正确 (sk-abc123de...)
或
? Key格式错误！DeepSeek应为: sk-xxxxxxxxxx
或
?? 请输入API Key
```

**各提供商的Key格式：**

| 提供商 | Key格式 | 示例 |
|--------|---------|------|
| **DeepSeek** | `sk-xxxxxxxxxx` | sk-abc123def456... |
| **OpenAI** | `sk-xxxxxxxxxx` | sk-xyz789ghi012... |
| **Google** | `AIzaxxxxxxxxxx` | AIzaSyABC123... |

---

#### **2.3 验证配置**

**正确配置示例（DeepSeek）：**

```
提供商: [DeepSeek ?]

API Key: sk-abc123def456...
? Key格式正确 (sk-abc123...)

API URL: https://api.deepseek.com/v1/chat/completions
模型: deepseek-chat
```

**错误配置示例：**

```
提供商: [DeepSeek ?]

API Key: flo_87d1...8ba5
? Key格式错误！DeepSeek应为: sk-xxxxxxxxxx

→ 需要更正：获取正确的DeepSeek API Key
```

---

## ?? **故障排除**

### **问题1：切换提供商后Key被清空**

**原因：** 防止混用不同提供商的Key（设计行为）

**解决：**
1. 这是**正常现象**，保护性设计
2. 重新填写当前提供商的正确Key
3. 保存设置

---

### **问题2：显示"Key格式错误"**

**原因：** Key格式不匹配当前提供商

**解决：**

#### **情况A：复制错了提供商的Key**

```
当前选择: DeepSeek
填入的Key: flo_xxx (错误，这不是DeepSeek的Key)

解决方案：
1. 访问 https://platform.deepseek.com/
2. 获取正确的DeepSeek Key（sk-xxx格式）
3. 重新填入
```

#### **情况B：Key复制不完整**

```
正确的Key: sk-abc123def456ghi789...
复制的Key: abc123def456... (缺少sk-前缀)

解决方案：
1. 重新复制完整的Key
2. 确保包含 "sk-" 前缀
```

---

### **问题3：跟随RimTalk但仍然报错**

**可能原因：**

#### **原因A：RimTalk未配置API**

**检查：**
```
1. 打开RimTalk Mod设置
2. 查看AI配置是否为空
3. 如果为空，勾选"使用独立配置"
```

#### **原因B：RimTalk配置的也是错误Key**

**检查：**
```
1. 查看RimTalk的API Key格式
2. 确认是否为正确的提供商
3. 修正RimTalk的配置
```

---

## ?? **配置检查清单**

### **检查1：提供商选择**

```
? OpenAI、DeepSeek、Google之一被选中
? 提供商按钮显示"?"标记
```

### **检查2：API Key格式**

```
? DeepSeek/OpenAI: sk-开头
? Google: AIza开头
? 显示"? Key格式正确"
```

### **检查3：API URL自动填充**

```
DeepSeek: https://api.deepseek.com/v1/chat/completions
OpenAI: https://api.openai.com/v1/chat/completions
Google: https://generativelanguage.googleapis.com/v1beta/models/
```

### **检查4：模型自动设置**

```
DeepSeek: deepseek-chat
OpenAI: gpt-3.5-turbo
Google: gemini-pro
```

---

## ?? **完整配置示例**

### **示例1：使用DeepSeek（独立配置）**

```
? 优先使用RimTalk的API配置

独立AI配置:
提供商: [DeepSeek ?]

API Key: sk-abc123def456...
? Key格式正确 (sk-abc123...)

API URL: https://api.deepseek.com/v1/chat/completions
模型: deepseek-chat

→ 保存 → 重启游戏 → 完成！
```

---

### **示例2：跟随RimTalk配置**

```
? 优先使用RimTalk的API配置

说明：
?? 将优先使用RimTalk的API配置
?? 如果RimTalk未配置，则使用下方的独立配置

→ 前往RimTalk Mod设置配置API
→ ExpandMemory自动跟随
→ 完成！
```

---

## ?? **重要提醒**

### **1. 切换提供商时Key会被清空**

这是**安全特性**，防止混用不同API的Key：

```
操作流程：
1. 当前：DeepSeek + sk-abc...
2. 点击：OpenAI
3. 结果：OpenAI + [空Key]
4. 必须：重新填写OpenAI的Key
```

### **2. 保存后需重启游戏**

API配置在**初始化时**加载，修改后需要重启：

```
修改流程：
1. 修改API配置
2. 点击"Apply"保存
3. 完全退出RimWorld
4. 重新启动游戏
5. 配置生效
```

### **3. Key格式验证仅检查前缀**

实时验证只检查前缀，不保证Key有效：

```
sk-abc123... ? 格式正确（但可能无效/过期）
flo_xxx... ? 格式错误
AIzaSyABC... ? 格式正确（Google）
```

真正的有效性需要调用API才知道。

---

## ?? **快速修复**

### **如果你遇到401错误：**

```
步骤1：检查Key格式
  → 看到"? Key格式错误"
  → 获取正确格式的Key

步骤2：检查提供商匹配
  → 提供商 = DeepSeek
  → Key = sk-xxx（DeepSeek的Key）
  → 不是其他AI的Key

步骤3：重新配置
  → 清空当前Key
  → 重新获取正确Key
  → 填入并保存
  → 重启游戏

步骤4：验证
  → 开DevMode查看日志
  → 应该看到：
    [AI] Initialized (DeepSeek/deepseek-chat)
    [Embedding] ? Initialized successfully!
```

---

**现在重启游戏，API配置会更安全和清晰！** ???
