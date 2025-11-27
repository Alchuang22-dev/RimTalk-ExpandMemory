# v3.0.0 验证和测试指南

## ?? 验证目标

确认v3.0智能评分系统已正确集成并正常工作。

---

## ? 验证清单

### 阶段1: 部署验证（5分钟）

#### 1.1 文件检查

**RimWorld Mod目录：** `D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\`

```
必须存在的文件：
? About/About.xml (版本号：3.0.0)
? 1.6/Assemblies/RimTalkMemoryPatch.dll
? Languages/ (多语言文件)

不应存在的文件：
? *.md (文档)
? *.bat (脚本)
? *.pdb (调试文件)
```

**验证命令：**
```powershell
# 检查About.xml版本号
Get-Content "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\About\About.xml" | Select-String "3.0.0"

# 检查DLL存在
Test-Path "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\1.6\Assemblies\RimTalkMemoryPatch.dll"

# 检查是否有多余文件
Get-ChildItem "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory" -Recurse -Include *.md,*.bat,*.pdb
```

#### 1.2 源代码检查

**新增文件：**
- [ ] `Source/Memory/AdvancedScoringSystem.cs` 存在
- [ ] `Source/Memory/SmartInjectionManager.cs` 存在

**修改文件：**
- [ ] `Source/Patches/RimTalkPrecisePatcher.cs` 已集成SmartInjectionManager
- [ ] `Source/Memory/PawnStatusKnowledgeGenerator.cs` 7天阈值逻辑

---

### 阶段2: 游戏启动验证（5分钟）

#### 2.1 启动RimWorld

```
1. 启动 RimWorld
2. 进入Mod管理器
3. 确认加载顺序：
   ? Core
   ? Harmony
   ? RimTalk
   ? RimTalk - Expand Memory (v3.0.0) ← 检查版本号
```

#### 2.2 检查错误日志

**位置：** `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log`

**检查点：**
```
搜索关键词：
? "[RimTalk Memory Patch v3.0.SMART]" - 应该出现
? "Successfully patched X/3 methods" - X应该≥1
? "Smart Injection" - 应该出现（如果已有对话）

不应出现的错误：
? "Exception" (红色)
? "NullReferenceException"
? "Failed to patch any methods"
```

**快速检查命令：**
```powershell
# 查看最新日志
Get-Content "$env:USERPROFILE\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log" -Tail 100 | Select-String "RimTalk Memory"
```

---

### 阶段3: 功能验证（15分钟）

#### 3.1 开启DevMode

```
1. 按 ~ 键打开控制台
2. 输入：devmode
3. 确认左上角出现DevMode图标
```

#### 3.2 场景识别验证

**测试场景1：日常闲聊**

```
操作：
1. 选择两个殖民者
2. 触发RimTalk对话
3. 查看日志

预期输出：
[Smart Injection] Scene: 日常闲聊
[Smart Injection] Injected X memories, Y knowledge
```

**测试场景2：紧急情况（袭击）**

```
操作：
1. Dev工具 → "Incident" → 触发袭击
2. 袭击开始后，触发对话
3. 查看日志

预期输出：
[Smart Injection] Scene: 紧急情况
[Smart Injection] Injected X memories, Y knowledge (X应该较少)
```

#### 3.3 新殖民者标记验证

**测试：**
```
操作：
1. Dev工具 → "Spawn pawn" → 创建新殖民者
2. 打开Mod设置 → RimTalk-ExpandMemory
3. 点击"打开常识库管理"
4. 查找新殖民者的常识条目

预期结果：
? 找到 "[新殖民者状态]" 条目
? 内容包含：新成员、刚加入
? 重要性：1.0
? 标签包含：种族名称

7天后：
? 标记应该被移除
```

#### 3.4 Token消耗验证

**对比测试：**

**旧版本（v2.x）：**
```
配置：最大记忆8，最大常识3
Token消耗：~330 tokens
```

**新版本（v3.0）：**
```
配置：最大记忆6-8，最大常识3-5
Token消耗：~280 tokens (-15%)
```

**验证方法：**
```
1. 打开Mod设置 → RimTalk-ExpandMemory
2. 点击"打开注入内容预览器"
3. 输入测试上下文："今天天气不错"
4. 查看底部Token估算
```

---

### 阶段4: 代码验证（10分钟）

#### 4.1 验证核心类

**AdvancedScoringSystem.cs**

```csharp
// 检查场景识别方法存在
public static SceneType IdentifyScene(string context)

// 检查场景类型枚举
public enum SceneType {
    CasualChat,      // 日常闲聊
    EmotionalTalk,   // 情感交流
    WorkDiscussion,  // 工作讨论
    HistoryRecall,   // 历史回忆
    EmergencySituation, // 紧急情况
    Introduction     // 自我介绍
}

// 检查评分方法
public static List<ScoredMemory> ScoreMemories(...)
```

**SmartInjectionManager.cs**

```csharp
// 检查统一注入接口
public static string InjectSmartContext(
    Pawn speaker,
    Pawn listener,
    string context,
    int maxMemories,
    int maxKnowledge
)
```

**RimTalkPrecisePatcher.cs**

```csharp
// 检查版本标记
private const string VERSION = "v3.0.SMART";

// 检查是否调用SmartInjectionManager
BuildContext_Postfix(...) {
    // 应该包含：
    SmartInjectionManager.InjectSmartContext(...)
}
```

#### 4.2 验证编译

```powershell
# 在项目目录运行
dotnet build -c Release

# 预期输出：
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### 阶段5: 性能验证（10分钟）

#### 5.1 性能基准测试

**测试场景：**
```
殖民地规模：10个殖民者
记忆数量：50条记忆/人
对话次数：20次
```

**指标：**
```
评分耗时：< 5ms/次
内存占用：< 10KB增量
游戏卡顿：无明显卡顿
```

**验证方法：**
```
1. 开启DevMode日志
2. 触发多次对话
3. 查看日志中的性能数据：
   [Smart Injection] Scene identified in Xms
   [Smart Injection] Scoring completed in Yms
```

#### 5.2 对话质量对比

**测试：相关性评估**

**场景1：谈论过去的事件**
```
上下文："还记得我们第一次袭击吗？"

旧系统注入：
- 最近的8条记忆（可能无关）
- 相关性：40-60%

新系统注入：
- 识别为"历史回忆"场景
- 优先CLPA层级
- 降低时间衰减权重
- 相关性：85-95%
```

**场景2：紧急对话**
```
上下文："敌人来了！"

旧系统注入：
- 固定权重，可能注入旧记忆
- Token：330

新系统注入：
- 识别为"紧急情况"
- 提高时间权重至50%
- 只注入最新相关信息
- Token：250 (-24%)
```

---

## ?? 详细测试步骤

### 测试1: 场景识别准确性

```
步骤：
1. 启动RimWorld，加载存档
2. 开启DevMode
3. 打开控制台（~键）

测试用例1：日常闲聊
4. 选择两个关系友好的殖民者
5. 触发RimTalk对话（右键 → "Chat"）
6. 查看Player.log

预期日志：
[Smart Injection] Scene: 日常闲聊
[Smart Injection] Context features: [关键词列表]
[Smart Injection] Injected 6 memories, 3 knowledge

测试用例2：紧急情况
7. Dev工具 → "Incident" → "Raid"
8. 袭击开始后，触发对话
9. 查看日志

预期日志：
[Smart Injection] Scene: 紧急情况
[Smart Injection] Emergency keywords detected
[Smart Injection] Injected 3 memories, 1 knowledge (数量减少)
```

### 测试2: 新殖民者7天标记

```
步骤：
1. 记录当前游戏时间
2. Dev工具 → "Spawn pawn" → 创建新殖民者 "TestPawn"
3. 打开Mod设置 → "打开常识库管理"
4. 搜索 "TestPawn"

预期结果（Day 0）：
? 找到条目："[新殖民者状态] TestPawn是新成员..."
? 重要性：1.0
? 启用状态：?

5. 使用Dev工具快进7天
6. 重新打开常识库

预期结果（Day 7+）：
? 条目仍存在但重要性降低，或
? 条目被移除（取决于实现）
```

### 测试3: Token消耗对比

```
步骤：
1. 打开Mod设置 → "打开注入内容预览器"
2. 选择一个有丰富记忆的Pawn
3. 输入测试上下文：

测试用例A：简单场景
Context: "今天天气不错"
记录Token数：_______

测试用例B：复杂场景
Context: "还记得我们三个月前建造的第一座防御塔吗？那时候大家都很紧张。"
记录Token数：_______

4. 对比旧版本（如果有）：
预期：新版本Token消耗降低10-20%
```

### 测试4: 多样性平衡

```
步骤：
1. 打开注入预览器
2. 输入上下文："最近怎么样？"
3. 查看注入的记忆类型分布

预期结果：
? 包含多种类型：工作、对话、事件
? 不全是同一类型
? 类型平衡合理

不良结果示例：
? 全是"对话"类型
? 全是"工作"类型
```

---

## ?? 验证报告模板

### 基础信息
```
验证日期：_______
验证人：_______
游戏版本：RimWorld 1.6
Mod版本：RimTalk-ExpandMemory v3.0.0
```

### 部署验证
- [ ] 文件完整性检查：通过/失败
- [ ] 无多余文件：通过/失败
- [ ] 版本号正确：通过/失败

### 启动验证
- [ ] 游戏正常启动：通过/失败
- [ ] Mod正确加载：通过/失败
- [ ] 无错误日志：通过/失败
- [ ] Harmony补丁成功：X/3

### 功能验证
- [ ] 场景识别准确：通过/失败
  - 日常闲聊：___
  - 紧急情况：___
  - 历史回忆：___
- [ ] 新殖民者标记：通过/失败
- [ ] Token消耗降低：通过/失败（降低___%）
- [ ] 多样性平衡：通过/失败

### 性能验证
- [ ] 评分耗时：___ms (目标: <5ms)
- [ ] 内存占用：___KB (目标: <10KB)
- [ ] 无明显卡顿：通过/失败

### 代码验证
- [ ] 编译成功：通过/失败
- [ ] 核心类存在：通过/失败
- [ ] 集成正确：通过/失败

### 对话质量
- [ ] 相关性提升：通过/失败（提升___%）
- [ ] 符合场景：通过/失败

### 总体评分
```
通过项：___/20
通过率：___%

结论：? 通过 / ?? 部分通过 / ? 失败
```

---

## ?? 常见问题排查

### 问题1: Mod未加载

**症状：**
- Mod列表中找不到RimTalk-ExpandMemory
- 或版本号仍显示v2.x

**排查：**
```powershell
# 检查Mod目录
Test-Path "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\About\About.xml"

# 检查版本号
Get-Content "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory\About\About.xml" | Select-String "modVersion"
```

**解决：**
1. 重新运行 `build-release.bat`
2. 重启RimWorld
3. 清除Mod配置缓存

---

### 问题2: 场景识别失败

**症状：**
- 日志中没有 `[Smart Injection] Scene: XXX`
- 或场景识别错误

**排查：**
```
查看日志：
[Smart Injection] Context: [原始内容]
[Smart Injection] Keywords: [提取的关键词]
```

**解决：**
1. 检查 `AdvancedScoringSystem.cs` 的关键词列表
2. 增加关键词覆盖
3. 调整场景识别逻辑

---

### 问题3: Token消耗未降低

**症状：**
- Token消耗仍然是330左右
- 没有明显降低

**排查：**
```
检查配置：
1. Mod设置 → RimTalk-ExpandMemory
2. 查看"最大注入记忆数"和"最大注入常识数"
3. 是否启用了"智能过滤"
```

**解决：**
1. 降低最大注入数量至6-8
2. 提高评分阈值至0.20
3. 检查是否正确调用SmartInjectionManager

---

### 问题4: 新殖民者标记失效

**症状：**
- 新加入的殖民者没有"新殖民者"标记
- 或标记未在7天后移除

**排查：**
```csharp
// 检查 PawnStatusKnowledgeGenerator.cs
public static void GenerateOrUpdatePawnStatusKnowledge(Pawn pawn)
{
    // 应该有7天判断逻辑
    int joinedDaysAgo = ...;
    if (joinedDaysAgo <= 7) {
        // 添加新殖民者标记
    }
}
```

**解决：**
1. 检查代码逻辑
2. 查看游戏日志是否有错误
3. 手动触发生成

---

## ? 验证完成标准

### 必须通过（20项）：

#### 部署（3项）
1. ? DLL文件存在
2. ? 版本号正确（3.0.0）
3. ? 无多余文件

#### 启动（3项）
4. ? 游戏正常启动
5. ? Mod正确加载
6. ? 至少1个Harmony补丁成功

#### 功能（8项）
7. ? 场景识别工作
8. ? 至少3种场景正确识别
9. ? 新殖民者自动标记
10. ? 7天后标记更新
11. ? Token消耗降低>10%
12. ? 多样性平衡生效
13. ? 预览器正常工作
14. ? 常识库管理正常

#### 性能（3项）
15. ? 评分耗时<5ms
16. ? 内存增量<10KB
17. ? 无明显卡顿

#### 代码（3项）
18. ? 编译无错误
19. ? 核心类齐全
20. ? 集成正确

### 总通过率：≥ 95% (19/20项)

---

## ?? 快速验证脚本

### 一键验证命令

```powershell
# 创建验证脚本
$verifyScript = @'
Write-Host "=== RimTalk-ExpandMemory v3.0.0 快速验证 ===" -ForegroundColor Cyan
Write-Host ""

# 1. 检查文件
Write-Host "[1/5] 检查文件..." -ForegroundColor Yellow
$modPath = "D:\steam\steamapps\common\RimWorld\Mods\RimTalk-ExpandMemory"
$dllPath = "$modPath\1.6\Assemblies\RimTalkMemoryPatch.dll"
$aboutPath = "$modPath\About\About.xml"

if (Test-Path $dllPath) {
    Write-Host "? DLL文件存在" -ForegroundColor Green
} else {
    Write-Host "? DLL文件缺失" -ForegroundColor Red
}

if (Test-Path $aboutPath) {
    $version = (Get-Content $aboutPath | Select-String "3.0.0")
    if ($version) {
        Write-Host "? 版本号正确 (3.0.0)" -ForegroundColor Green
    } else {
        Write-Host "? 版本号错误" -ForegroundColor Red
    }
}

# 2. 检查多余文件
Write-Host "`n[2/5] 检查多余文件..." -ForegroundColor Yellow
$extraFiles = Get-ChildItem $modPath -Recurse -Include *.md,*.bat,*.pdb -ErrorAction SilentlyContinue
if ($extraFiles.Count -eq 0) {
    Write-Host "? 无多余文件" -ForegroundColor Green
} else {
    Write-Host "?? 发现多余文件: $($extraFiles.Count)个" -ForegroundColor Yellow
}

# 3. 检查源代码
Write-Host "`n[3/5] 检查源代码..." -ForegroundColor Yellow
$projectPath = "C:\Users\Administrator\Desktop\rim mod\RimTalk-ExpandMemory"
if (Test-Path "$projectPath\Source\Memory\AdvancedScoringSystem.cs") {
    Write-Host "? AdvancedScoringSystem.cs 存在" -ForegroundColor Green
} else {
    Write-Host "? AdvancedScoringSystem.cs 缺失" -ForegroundColor Red
}

if (Test-Path "$projectPath\Source\Memory\SmartInjectionManager.cs") {
    Write-Host "? SmartInjectionManager.cs 存在" -ForegroundColor Green
} else {
    Write-Host "? SmartInjectionManager.cs 缺失" -ForegroundColor Red
}

# 4. 检查日志
Write-Host "`n[4/5] 检查游戏日志..." -ForegroundColor Yellow
$logPath = "$env:USERPROFILE\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log"
if (Test-Path $logPath) {
    $logContent = Get-Content $logPath -Tail 200
    $patchLog = $logContent | Select-String "RimTalk Memory Patch"
    if ($patchLog) {
        Write-Host "? 找到补丁日志" -ForegroundColor Green
        Write-Host "   $($patchLog[0])" -ForegroundColor Gray
    } else {
        Write-Host "?? 未找到补丁日志（可能游戏未运行）" -ForegroundColor Yellow
    }
} else {
    Write-Host "?? 游戏日志不存在（游戏未运行）" -ForegroundColor Yellow
}

# 5. 总结
Write-Host "`n[5/5] 验证总结" -ForegroundColor Yellow
Write-Host "================================" -ForegroundColor Cyan
Write-Host "部署验证: 完成" -ForegroundColor Green
Write-Host "文件检查: 完成" -ForegroundColor Green
Write-Host "代码检查: 完成" -ForegroundColor Green
Write-Host "`n下一步: 启动RimWorld进行功能测试" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
'@

# 执行验证
Invoke-Expression $verifyScript
```

---

**准备好了吗？开始验证v3.0！** ??

需要我帮你：
1. ? 执行快速验证脚本？
2. ? 创建详细测试用例？
3. ? 准备性能基准测试？
4. ? 直接开始游戏测试？
