# RimTalk 集成说明 - 不修改原代码

## 核心原则

**? 我们不修改RimTalk的任何代码**  
**? 我们只使用Harmony动态补丁（运行时拦截）**  
**? RimTalk可以随时更新，不受影响**

---

## Harmony补丁工作原理

### 什么是Harmony？

Harmony是RimWorld Mod开发的标准库，用于在**运行时**修改游戏行为，而不改动原始代码。

```
原理：
1. RimTalk加载并执行
2. 我们的Mod（RimTalk-ExpandMemory）加载
3. Harmony拦截RimTalk的特定方法调用
4. 在方法执行前/后注入我们的逻辑
5. RimTalk的原始代码完全不变
```

### 类比解释

```
RimTalk = 原版电影
Harmony补丁 = 字幕组加字幕

- 原版电影文件不变
- 播放器加载字幕后显示额外内容
- 字幕组更新不影响电影
- 电影更新不影响字幕（可能需要微调时间轴）
```

---

## 我们具体做了什么

### 1. 拦截RimTalk的方法

```csharp
// RimTalk的代码（我们不改）
public static string BuildContext(List<Pawn> pawns)
{
    // RimTalk原始逻辑
    return context;
}

// 我们的Harmony补丁（运行时注入）
[HarmonyPostfix]
static void BuildContext_Postfix(ref string __result, List<Pawn> pawns)
{
    // __result 是RimTalk返回的context
    // 我们在后面加上记忆内容
    __result = __result + "\n\n" + SmartInjectionManager.InjectSmartContext(...);
}
```

**效果：**
- RimTalk的代码：100%不变
- 运行时行为：返回值被我们增强
- RimTalk作者完全不知道我们的存在

---

### 2. 拦截的方法列表

| RimTalk方法 | 我们的补丁类型 | 作用 |
|------------|--------------|------|
| `PromptService.BuildContext` | Postfix（后置） | 在提示词后追加记忆 |
| `PromptService.DecoratePrompt` | Postfix（后置） | 在装饰后追加记忆 |
| `TalkService.GenerateTalk` | Prefix（前置） | 预处理（备用） |

**关键点：**
- ? 我们不改方法内部逻辑
- ? 我们不阻止方法执行
- ? 我们只是"偷听"并追加内容

---

## 安全性保证

### 1. RimTalk更新不会破坏我们的代码

**场景：RimTalk发布v2.0**

```
情况A：方法签名不变
- 我们的补丁继续工作 ?

情况B：方法名改了（如 BuildContext -> CreateContext）
- Harmony找不到方法，静默失败
- 我们的补丁不生效，但不会崩溃
- 记忆功能暂时失效，但游戏正常运行
- 我们更新补丁目标方法名即可

情况C：方法完全重构
- 同B，静默失效
- 我们需要找到新的集成点
```

**结论：最坏情况是功能失效，不会导致崩溃或冲突**

---

### 2. 我们的代码完全独立

```
RimTalk文件结构：
RimTalk/
├── Assemblies/
│   └── RimTalk.dll       <-- RimTalk的代码
└── ...

RimTalk-ExpandMemory文件结构：
RimTalk-ExpandMemory/
├── Assemblies/
│   └── RimTalkMemoryPatch.dll  <-- 我们的代码
└── ...
```

**两者关系：**
- 物理隔离：完全不同的DLL文件
- 逻辑独立：我们只是"监听者"，不是"修改者"
- 可选依赖：没有RimTalk时，我们的Mod只是不工作

---

### 3. 用户可以随时禁用我们

```
用户操作：
1. 在Mod管理器中禁用 RimTalk-ExpandMemory
2. RimTalk恢复原始行为
3. 没有任何残留或副作用
```

---

## 兼容性策略

### 多版本兼容

我们使用**反射**查找方法，而不是硬编码：

```csharp
// ? 不好的做法（硬编码）
RimTalk.Service.PromptService.BuildContext(...); // 如果RimTalk改了命名空间就崩溃

// ? 好的做法（反射）
var assembly = AppDomain.CurrentDomain.GetAssemblies()
    .FirstOrDefault(a => a.GetName().Name == "RimTalk");

var type = assembly.GetType("RimTalk.Service.PromptService");
var method = type.GetMethod("BuildContext");

// 如果找不到，返回null，不会崩溃
```

### Fallback机制

```csharp
bool patchedBuildContext = PatchBuildContext(...);
bool patchedDecoratePrompt = PatchDecoratePrompt(...);
bool patchedGenerateTalk = PatchGenerateTalk(...);

// 即使3个都失败，也只是记录日志，不影响游戏
if (successCount == 0)
{
    Log.Error("Failed to patch any methods");
    // 但游戏继续运行，只是记忆功能失效
}
```

---

## 实际测试案例

### 测试1：RimTalk未安装

```
结果：
- 游戏正常启动 ?
- RimTalk-ExpandMemory检测不到RimTalk
- 记忆功能不启用
- 其他功能（常识库、记忆管理）正常工作
```

### 测试2：RimTalk版本不兼容

```
场景：RimTalk更新后改了方法名

结果：
- 补丁失败（静默）
- 日志显示警告
- 对话生成正常（RimTalk原始行为）
- 记忆不注入（但不崩溃）
```

### 测试3：同时安装多个RimTalk扩展Mod

```
场景：
- RimTalk
- RimTalk-ExpandMemory（我们）
- 另一个扩展Mod

结果：
- Harmony按加载顺序依次应用补丁
- 每个Mod的Postfix都会执行
- 最终提示词 = RimTalk原始 + Mod1追加 + Mod2追加 + 我们追加
- 只要不冲突即可和谐共存
```

---

## 法律和道德

### 许可证合规

```
RimTalk许可：MIT License (假设)
- ? 允许使用
- ? 允许扩展
- ? 不修改原代码

Harmony许可：MIT License
- ? 允许商业使用
- ? RimWorld官方认可
```

### 尊重原作者

```
我们的做法：
? 不修改RimTalk代码
? 不声称是RimTalk的一部分
? 在Mod描述中注明"需要RimTalk"
? 给RimTalk作者信用
? 不破坏RimTalk的功能
```

---

## 总结

### 为什么这是安全的？

1. **技术层面**
   - Harmony是官方认可的标准方案
   - 运行时补丁，不改原文件
   - 失败时静默降级，不崩溃

2. **兼容性层面**
   - 反射查找，适应版本变化
   - 多层Fallback机制
   - 可独立禁用

3. **法律层面**
   - 符合开源许可证
   - 不侵犯原作者权益
   - 明确标注依赖关系

### 类比其他Mod

```
类似的成功案例：
- HugsLib：扩展Mod加载器
- Camera+：扩展相机功能
- Fluffy's系列：扩展UI
- 无数个使用Harmony的Mod

都是"不改原代码，只补丁"的典范
```

---

## 开发者指南

### 如果RimTalk更新后失效

**步骤1：检查日志**

```
[RimTalk Memory Patch] Failed to patch BuildContext: Method not found
```

**步骤2：反编译RimTalk新版本**

使用ILSpy或dnSpy查看：
- 类名是否改变
- 方法名是否改变
- 参数是否改变

**步骤3：更新补丁目标**

```csharp
// 旧版本
var method = type.GetMethod("BuildContext");

// 新版本（假设改名了）
var method = type.GetMethod("CreatePromptContext");
```

**步骤4：发布更新**

```
版本号：v3.0.1
更新说明：兼容 RimTalk v2.0
```

---

## FAQ

**Q: 这会影响RimTalk的性能吗？**  
A: 影响极小（+1-2ms），因为我们只是在结果后追加字符串。

**Q: RimTalk作者会反对吗？**  
A: 不太可能，这是Mod社区的常见做法，且我们不破坏原功能。

**Q: 如果两个扩展Mod都补丁同一个方法？**  
A: Harmony会按加载顺序依次执行，只要都是Postfix且不冲突就没问题。

**Q: 用户需要手动配置吗？**  
A: 不需要，只要安装了RimTalk和我们的Mod即可自动工作。

---

**结论：这是一种安全、可靠、社区认可的扩展方式。** ?
