# RimTalk-ExpandActions 集成指南

## ?? 目标

让 `RimTalk-ExpandActions` 自动向 `RimTalk-ExpandMemory` 的常识库添加行为指令常识。

## ?? API 使用方法

### 方法1：批量导入常识（推荐）

```csharp
using RimTalk.Memory;

// 在 ExpandActions 的 StaticConstructor 或初始化代码中调用
[StaticConstructorOnStartup]
public static class ExpandActionsKnowledgeInjector
{
    static ExpandActionsKnowledgeInjector()
    {
        try
        {
            // 定义行为指令常识（多行文本）
            string knowledgeText = @"
[行为-战斗|0.9]遭遇敌人时保持冷静，优先寻找掩体，与队友配合作战
[行为-建造|0.8]建造前检查材料充足，优先完成紧急建筑
[行为-医疗|0.9]治疗伤员时优先处理重伤患者，确保医疗室清洁
[行为-种植|0.7]种植作物时关注生长周期，及时收获防止腐烂
[行为-研究|0.8]进行研究时保持专注，优先解锁关键科技
[行为-社交|0.7]与他人交流时保持友好，避免引发冲突
[行为-休息|0.6]感到疲惫时及时休息，保持良好的精神状态
[行为-娱乐|0.6]适当娱乐有助于缓解压力，但不要过度沉迷
[行为-狩猎|0.8]狩猎时注意安全距离，避免被猎物反击
[行为-烹饪|0.7]烹饪食物时保持卫生，避免食物中毒
[行为-搬运|0.6]搬运物资时规划路线，提高效率
[行为-清洁|0.6]保持环境整洁有助于健康，定期清理垃圾
[行为-采矿|0.7]采矿时注意结构稳定，防止坍塌
[行为-驯兽|0.7]驯服动物时保持耐心，使用温和的方法
[行为-守卫|0.8]站岗时保持警惕，发现异常立即报警
";

            // 调用 ExpandMemory 的公共 API
            int importedCount = CommonKnowledgeLibrary.ImportFromExternalMod(
                knowledgeText, 
                sourceModName: "ExpandActions", 
                overwriteExisting: false  // 不覆盖已存在的条目
            );
            
            Log.Message($"[ExpandActions] Successfully imported {importedCount} behavior knowledge entries");
        }
        catch (Exception ex)
        {
            Log.Error($"[ExpandActions] Failed to inject knowledge: {ex.Message}");
        }
    }
}
```

### 方法2：单条添加常识

```csharp
using RimTalk.Memory;

// 动态添加单条常识
public static void AddBehaviorKnowledge(string behavior, string instruction, float importance = 0.7f)
{
    bool success = CommonKnowledgeLibrary.AddKnowledgeEntry(
        tag: $"行为-{behavior}",
        content: instruction,
        importance: importance,
        sourceModName: "ExpandActions"
    );
    
    if (success)
    {
        Log.Message($"[ExpandActions] Added knowledge: [{behavior}] {instruction}");
    }
}

// 示例：在特定事件发生时添加常识
public static void OnColonistMasterSkill(Pawn pawn, SkillDef skill)
{
    if (skill == SkillDefOf.Construction)
    {
        AddBehaviorKnowledge(
            behavior: "建造专家",
            instruction: $"{pawn.LabelShort}是建造专家，在建造项目中表现出色",
            importance: 0.85f
        );
    }
}
```

### 方法3：从文件加载常识

```csharp
using RimTalk.Memory;
using System.IO;

[StaticConstructorOnStartup]
public static class ExpandActionsKnowledgeLoader
{
    static ExpandActionsKnowledgeLoader()
    {
        try
        {
            // 从 ExpandActions 的 Data 文件夹加载常识
            string modPath = LoadedModManager.GetMod<ExpandActions_Mod>().Content.RootDir;
            string knowledgeFilePath = Path.Combine(modPath, "Data", "BehaviorKnowledge.txt");
            
            if (File.Exists(knowledgeFilePath))
            {
                string knowledgeText = File.ReadAllText(knowledgeFilePath, Encoding.UTF8);
                
                int importedCount = CommonKnowledgeLibrary.ImportFromExternalMod(
                    knowledgeText, 
                    sourceModName: "ExpandActions", 
                    overwriteExisting: false
                );
                
                Log.Message($"[ExpandActions] Loaded {importedCount} knowledge entries from {knowledgeFilePath}");
            }
            else
            {
                Log.Warning($"[ExpandActions] Knowledge file not found: {knowledgeFilePath}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[ExpandActions] Failed to load knowledge file: {ex.Message}");
        }
    }
}
```

## ?? 常识文件格式

在 `ExpandActions/Data/BehaviorKnowledge.txt` 中：

```
// 战斗行为指令
[行为-战斗|0.9]遭遇敌人时保持冷静，优先寻找掩体，与队友配合作战
[行为-战斗-近战|0.85]近战时注意敌人数量，避免被包围
[行为-战斗-远程|0.85]远程攻击时保持安全距离，优先攻击威胁最大的目标

// 建造行为指令
[行为-建造|0.8]建造前检查材料充足，优先完成紧急建筑
[行为-建造-防御|0.9]建造防御设施时考虑射击视野和掩体位置
[行为-建造-住宅|0.7]建造住宅时兼顾美观和功能性

# 注释：支持 // 和 # 开头的注释行

// 医疗行为指令
[行为-医疗|0.9]治疗伤员时优先处理重伤患者，确保医疗室清洁
[行为-医疗-手术|0.95]进行手术前准备充分，确保成功率
```

**格式规范**：
- `[标签|重要性]内容`
- 重要性范围：`0.0` ~ `1.0`
- 支持注释：`//` 或 `#` 开头的行会被忽略
- 空行会被忽略

## ?? 卸载和更新

### 移除所有来自 ExpandActions 的常识

```csharp
using RimTalk.Memory;

// 在 Mod 卸载或禁用时调用
public static void OnModUnload()
{
    int removedCount = CommonKnowledgeLibrary.RemoveKnowledgeBySource("ExpandActions");
    Log.Message($"[ExpandActions] Removed {removedCount} knowledge entries");
}
```

### 更新常识（覆盖模式）

```csharp
using RimTalk.Memory;

public static void UpdateKnowledge()
{
    // 先移除旧的
    CommonKnowledgeLibrary.RemoveKnowledgeBySource("ExpandActions");
    
    // 再导入新的
    string newKnowledgeText = @"
[行为-战斗|0.95]遭遇敌人时保持冷静，优先寻找掩体，与队友配合作战（已更新）
[行为-建造|0.85]建造前检查材料充足，优先完成紧急建筑（已更新）
";
    
    CommonKnowledgeLibrary.ImportFromExternalMod(
        newKnowledgeText, 
        sourceModName: "ExpandActions", 
        overwriteExisting: false
    );
}
```

## ?? 高级用法

### 动态生成行为指令

```csharp
using RimTalk.Memory;
using System.Text;

public static class DynamicBehaviorKnowledgeGenerator
{
    // 根据殖民者技能动态生成常识
    public static void GenerateSkillBasedKnowledge()
    {
        var knowledgeBuilder = new StringBuilder();
        
        foreach (var colonist in Find.CurrentMap.mapPawns.FreeColonists)
        {
            var bestSkill = colonist.skills.skills
                .OrderByDescending(s => s.Level)
                .FirstOrDefault();
            
            if (bestSkill != null && bestSkill.Level >= 10)
            {
                string instruction = $"{colonist.LabelShort}擅长{bestSkill.def.label}（等级{bestSkill.Level}），" +
                                   $"在相关任务中表现出色，应优先分配该类工作";
                
                knowledgeBuilder.AppendLine($"[技能-{bestSkill.def.label}|0.8]{instruction}");
            }
        }
        
        if (knowledgeBuilder.Length > 0)
        {
            CommonKnowledgeLibrary.ImportFromExternalMod(
                knowledgeBuilder.ToString(), 
                sourceModName: "ExpandActions", 
                overwriteExisting: true  // 覆盖旧的技能常识
            );
        }
    }
    
    // 定期更新（可在 MapComponent 的 Tick 中调用）
    public static void PeriodicUpdate(int intervalDays = 7)
    {
        int currentDay = GenDate.DaysPassed;
        
        if (currentDay % intervalDays == 0)
        {
            // 每7天更新一次
            GenerateSkillBasedKnowledge();
        }
    }
}
```

### 基于事件动态添加常识

```csharp
using RimTalk.Memory;

// Harmony Patch：捕获重要事件并生成常识
[HarmonyPatch(typeof(Pawn_JobTracker), "EndCurrentJob")]
public static class JobCompletionKnowledgePatch
{
    static void Postfix(Pawn_JobTracker __instance, JobCondition condition)
    {
        if (condition != JobCondition.Succeeded)
            return;
        
        Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        Job lastJob = Traverse.Create(__instance).Field("curJob").GetValue<Job>();
        
        if (lastJob == null || pawn == null || !pawn.IsColonist)
            return;
        
        // 检测重要任务完成
        if (IsImportantJob(lastJob))
        {
            string instruction = GenerateInstructionFromJob(pawn, lastJob);
            
            CommonKnowledgeLibrary.AddKnowledgeEntry(
                tag: $"经验-{lastJob.def.label}",
                content: instruction,
                importance: 0.75f,
                sourceModName: "ExpandActions"
            );
        }
    }
    
    private static bool IsImportantJob(Job job)
    {
        // 定义重要任务类型
        return job.def == JobDefOf.FinishFrame || 
               job.def == JobDefOf.TendPatient ||
               job.def == JobDefOf.Research;
    }
    
    private static string GenerateInstructionFromJob(Pawn pawn, Job job)
    {
        return $"{pawn.LabelShort}成功完成了{job.def.label}任务，" +
               $"展现出良好的{GetRelevantSkill(job)}技能";
    }
    
    private static string GetRelevantSkill(Job job)
    {
        if (job.def == JobDefOf.FinishFrame) return "建造";
        if (job.def == JobDefOf.TendPatient) return "医疗";
        if (job.def == JobDefOf.Research) return "研究";
        return "工作";
    }
}
```

## ?? 测试和验证

### 检查常识是否成功导入

1. **开发模式日志**：
   - 打开 RimWorld 开发模式（按 `~` 键）
   - 查看日志中的 `[CommonKnowledge API]` 消息

2. **游戏内检查**：
   - 打开常识库UI：底部工具栏 → "记忆" → "常识"按钮
   - 搜索 "来自:ExpandActions" 标签
   - 查看自动导入的常识列表

3. **代码验证**：
```csharp
// 检查导入的常识数量
var library = MemoryManager.GetCommonKnowledge();
int expandActionsCount = library.Entries
    .Count(e => e.tags != null && e.tags.Contains("来自:ExpandActions"));

Log.Message($"[ExpandActions] Found {expandActionsCount} knowledge entries in library");
```

## ?? 注意事项

1. **依赖性**：确保 `RimTalk-ExpandMemory` 已加载
2. **加载顺序**：在 `About.xml` 中设置依赖
```xml
<ModDependencies>
    <li>
        <packageId>sanguodxj.rimtalk.expandmemory</packageId>
        <displayName>RimTalk-ExpandMemory</displayName>
        <steamWorkshopUrl>steam://url/CommunityFilePage/YOUR_MOD_ID</steamWorkshopUrl>
    </li>
</ModDependencies>
```

3. **性能考虑**：不要频繁调用 API（建议在初始化或定期更新时调用）
4. **冲突处理**：使用 `overwriteExisting: false` 避免覆盖用户手动添加的常识
5. **清理**：Mod 卸载时调用 `RemoveKnowledgeBySource` 清理数据

## ?? 完整示例

完整的 `ExpandActions` 集成代码：

```csharp
using RimTalk.Memory;
using Verse;
using System;

namespace ExpandActions
{
    [StaticConstructorOnStartup]
    public static class KnowledgeIntegration
    {
        private const string MOD_NAME = "ExpandActions";
        
        static KnowledgeIntegration()
        {
            try
            {
                // 检查 ExpandMemory 是否已加载
                if (!ModsConfig.IsActive("sanguodxj.rimtalk.expandmemory"))
                {
                    Log.Warning($"[{MOD_NAME}] RimTalk-ExpandMemory not found, knowledge integration disabled");
                    return;
                }
                
                // 导入行为指令常识
                string knowledgeText = GetBehaviorKnowledge();
                
                int importedCount = CommonKnowledgeLibrary.ImportFromExternalMod(
                    knowledgeText, 
                    sourceModName: MOD_NAME, 
                    overwriteExisting: false
                );
                
                Log.Message($"[{MOD_NAME}] ? Successfully integrated {importedCount} behavior knowledge entries into RimTalk-ExpandMemory");
            }
            catch (Exception ex)
            {
                Log.Error($"[{MOD_NAME}] ? Failed to integrate knowledge: {ex.Message}");
            }
        }
        
        private static string GetBehaviorKnowledge()
        {
            return @"
[行为-战斗|0.9]遭遇敌人时保持冷静，优先寻找掩体，与队友配合作战
[行为-建造|0.8]建造前检查材料充足，优先完成紧急建筑
[行为-医疗|0.9]治疗伤员时优先处理重伤患者，确保医疗室清洁
[行为-种植|0.7]种植作物时关注生长周期，及时收获防止腐烂
[行为-研究|0.8]进行研究时保持专注，优先解锁关键科技
[行为-社交|0.7]与他人交流时保持友好，避免引发冲突
[行为-休息|0.6]感到疲惫时及时休息，保持良好的精神状态
[行为-娱乐|0.6]适当娱乐有助于缓解压力，但不要过度沉迷
[行为-狩猎|0.8]狩猎时注意安全距离，避免被猎物反击
[行为-烹饪|0.7]烹饪食物时保持卫生，避免食物中毒
[行为-搬运|0.6]搬运物资时规划路线，提高效率
[行为-清洁|0.6]保持环境整洁有助于健康，定期清理垃圾
[行为-采矿|0.7]采矿时注意结构稳定，防止坍塌
[行为-驯兽|0.7]驯服动物时保持耐心，使用温和的方法
[行为-守卫|0.8]站岗时保持警惕，发现异常立即报警
";
        }
    }
}
```

## ?? 总结

通过使用 `RimTalk-ExpandMemory` 提供的公共 API，`ExpandActions` 可以轻松自动添加行为指令常识，最终效果与用户手动导入完全一致。常识会被正确标记来源，可以在游戏内UI中查看和管理。
