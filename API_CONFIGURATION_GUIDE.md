# ?? API配置指南 - 解决401错误

## ? **当前问题**

```
[Embedding] API call failed: The remote server returned an error: (401) Unauthorized.
```

**原因：** API Key未配置或配置错误

---

## ? **解决方案**

### **步骤1：获取API Key**

#### **推荐：DeepSeek（最便宜，~?0.01/月）**

1. 访问：https://platform.deepseek.com/
2. 注册账号（手机号验证）
3. 充值：?5-?10（够用很久）
4. 进入 "API Keys" 页面
5. 点击 "Create API Key"
6. 复制生成的Key（格式：`sk-xxxxxxxxxx`）

**重要：** 保存好API Key，只显示一次！

---

### **步骤2：在Mod中配置API**

#### **2.1 打开设置**

```
RimWorld游戏内
→ Options（选项）
→ Mod Settings（Mod设置）
→ RimTalk-Expand Memory
```

#### **2.2 找到AI配置区域**

现在**AI配置**和**实验性功能**默认已展开，直接可见！

```
╔═══════════════════════════════════╗
║ RimTalk - Expand Memory          ║
╠═══════════════════════════════════╣
║ ...                               ║
║ ▼ AI配置 ← 默认展开              ║
║   ? 优先使用RimTalk的AI配置      ║
║                                   ║
║   独立AI配置:                     ║
║   提供商: [DeepSeek ?] [OpenAI]  ║
║   API Key: [输入框] ← 在这里填写  ║
║   API URL: https://api.deepseek...║
║   模型: deepseek-chat             ║
║                                   ║
║ ▼ ?? 实验性功能 ← 默认展开        ║
║   ?? 语义嵌入 (v3.1)              ║
║   ? 启用语义嵌入                  ║
║   ?? 需要配置API Key              ║
╚═══════════════════════════════════╝
```

---

#### **2.3 配置步骤**

**Step 1：选择提供商**

点击 **"DeepSeek"** 按钮（会自动设置API URL和模型）

**Step 2：填写API Key**

在 **"API Key"** 输入框中粘贴你的Key：
```
sk-xxxxxxxxxxxxxxxxxxxxxxxxx
```

**Step 3：验证配置**

检查自动填充的内容：
- API URL: `https://api.deepseek.com/v1/embeddings`
- 模型: `deepseek-chat`

**Step 4：保存**

点击设置窗口底部的 **"Apply"** 或 **"OK"**

---

### **步骤3：启用实验性功能**

在 **"?? 实验性功能"** 区域（默认已展开）：

```
? 启用语义嵌入 ← 勾选这个

状态会从：
  ? 请配置API Key
变为：
  ? API已配置
```

---

### **步骤4：重启游戏并验证**

#### **4.1 重启游戏**

完全退出RimWorld，然后重新启动

#### **4.2 开启DevMode查看日志**

1. 按 **`~`** 键开启DevMode
2. 查看日志（F12或查看Player.log）

#### **4.3 成功的日志应该显示：**

```
[Embedding] ? Initialized successfully!
[Embedding]   Provider: DeepSeek
[Embedding]   Dimension: 1024D
[Embedding]   API Key: sk-xxxxxxx... (length: 51)
```

#### **4.4 如果仍然401错误，检查：**

```
[Embedding] ? API Key is not configured!
```
→ 返回步骤2重新配置

```
[Embedding] Calling DeepSeek API...
[Embedding] API Key: sk-xxxxxx... (length: 51)
[Embedding] API Error 401: {"error": "Invalid API key"}
```
→ API Key错误，检查是否复制完整

---

## ?? **详细日志说明**

### **正常日志（成功）：**

```
[Embedding] ? Initialized successfully!
[Embedding]   Provider: DeepSeek
[Embedding]   Dimension: 1024D
[Embedding]   API Key: sk-abc123de... (length: 51)

[Embedding] Calling DeepSeek API...
[Embedding] API URL: https://api.deepseek.com/v1/embeddings
[Embedding] Request body: {"model":"deepseek-embedding","input":"xxx"}...
[Embedding] Response status: OK
[Embedding] Response: {"object":"list","data":[{"embedding":[0.123...
```

### **错误日志（API Key未配置）：**

```
[Embedding] ? API Key is not configured!
[Embedding] Please configure API Key in Mod Settings:
[Embedding]   Options → Mod Settings → RimTalk-Expand Memory
[Embedding]   → AI配置 → 填写 API Key
```

### **错误日志（401 Unauthorized）：**

```
[Embedding] Calling DeepSeek API...
[Embedding] API Key: sk-xxxxxx... (length: 51)
[Embedding] API Error 401: {"error": {"message": "Invalid API key"}}
```

**可能原因：**
1. API Key复制不完整
2. API Key已被删除/禁用
3. API Key格式错误（应以`sk-`开头）

---

## ?? **快速测试API配置**

### **测试命令（可选）：**

在DevMode下，可以手动触发一次嵌入测试：

1. 按 **F1** 打开Debug Actions
2. 搜索 "Embedding"
3. 找到 "Test Embedding Service"
4. 点击执行

查看日志输出验证配置是否正确。

---

## ?? **完整配置示例**

### **DeepSeek配置（推荐）：**

```
提供商: DeepSeek
API Key: sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
API URL: https://api.deepseek.com/v1/embeddings
模型: deepseek-embedding

成本: ~?0.01/月
速度: 快
质量: 高
```

### **OpenAI配置：**

```
提供商: OpenAI
API Key: sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
API URL: https://api.openai.com/v1/embeddings
模型: text-embedding-ada-002

成本: ~$0.01/月
速度: 中
质量: 最高
```

### **Google Gemini配置：**

```
提供商: Google
API Key: AIza...xxxxxxxxxxxxxxxx
API URL: https://generativelanguage.googleapis.com/v1beta/...
模型: text-embedding-004

成本: 免费额度
速度: 快
质量: 高
```

---

## ?? **常见问题**

### **Q1: 找不到AI配置区域**

**A:** 现在AI配置默认展开，在设置菜单中间位置

### **Q2: 填写了API Key但仍然401**

**A:** 检查：
1. API Key是否完整（没有空格或换行）
2. 提供商选择是否正确
3. 是否点击了"Apply"保存
4. 是否重启了游戏

### **Q3: DeepSeek API Key在哪里获取？**

**A:** 
1. https://platform.deepseek.com/
2. 登录 → API Keys → Create API Key
3. 复制完整的Key（以`sk-`开头）

### **Q4: 是否必须配置API？**

**A:** 
- 如果只使用基础功能：? 不需要
- 如果要启用语义嵌入：? 需要
- 如果要启用RAG检索：? 推荐（否则降级）

### **Q5: API成本会很高吗？**

**A:** 
- DeepSeek: ~?0.01/月（极低）
- OpenAI: ~$0.01/月（低）
- Gemini: 免费额度够用

---

## ?? **快速开始**

### **最简配置（3分钟）：**

1. ? 获取DeepSeek API Key
2. ? 游戏内打开Mod设置
3. ? AI配置区域（已展开）
4. ? 点击"DeepSeek"
5. ? 粘贴API Key
6. ? 点击"Apply"
7. ? 勾选"启用语义嵌入"
8. ? 重启游戏

**完成！** ??

---

## ?? **更新日志**

### **v3.3.1 修复：**
- ? AI配置区域默认展开
- ? 实验性功能区域默认展开
- ? 增加滚动视图高度到2400
- ? 添加详细的API Key验证日志
- ? 在初始化时显示API Key前缀和长度

**现在配置API更简单了！** ???
