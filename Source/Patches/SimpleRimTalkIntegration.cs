using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using RimWorld;
using RimTalk.MemoryPatch;

namespace RimTalk.Memory.Patches
{
    /// <summary>
    /// Simple integration that exposes memory data through a public static API
    /// RimTalk can call these methods directly
    /// </summary>
    [StaticConstructorOnStartup]
    public static class SimpleRimTalkIntegration
    {
        static SimpleRimTalkIntegration()
        {
            Log.Message("[RimTalk Memory] Simple integration initialized");
            Log.Message("[RimTalk Memory] Call RimTalkMemoryAPI.GetMemoryPrompt(pawn, basePrompt) to use memories");
            
            // 初始化 AI 总结服务
            RimTalkAISummarizer.Initialize();
        }
    }

    /// <summary>
    /// Public API for RimTalk to access memory system
    /// </summary>
    public static class RimTalkMemoryAPI
    {
        /// <summary>
        /// Get conversation prompt enhanced with pawn's memories
        /// </summary>
        public static string GetMemoryPrompt(Pawn pawn, string basePrompt)
        {
            if (pawn == null) return basePrompt;

            var memoryComp = pawn.TryGetComp<PawnMemoryComp>();
            if (memoryComp == null)
            {
                Log.Warning($"[RimTalk Memory API] {pawn.LabelShort} has no memory component");
                return basePrompt;
            }

            string memoryContext = memoryComp.GetMemoryContext();
            
            if (string.IsNullOrEmpty(memoryContext))
            {
                return basePrompt;
            }

            Log.Message($"[RimTalk Memory API] Adding memory context for {pawn.LabelShort}");
            return memoryContext + "\n\n" + basePrompt;
        }

        /// <summary>
        /// Get recent memories for a pawn
        /// </summary>
        public static System.Collections.Generic.List<MemoryEntry> GetRecentMemories(Pawn pawn, int count = 5)
        {
            var memoryComp = pawn?.TryGetComp<PawnMemoryComp>();
            return memoryComp?.GetRelevantMemories(count) ?? new System.Collections.Generic.List<MemoryEntry>();
        }

        /// <summary>
        /// Record a conversation between two pawns
        /// </summary>
        public static void RecordConversation(Pawn speaker, Pawn listener, string content)
        {
            // 直接调用底层方法，由底层统一输出日志
            MemoryAIIntegration.RecordConversation(speaker, listener, content);
            // 不在这里输出日志，避免重复
        }

        /// <summary>
        /// Check if a pawn has the memory component
        /// </summary>
        public static bool HasMemoryComponent(Pawn pawn)
        {
            return pawn?.TryGetComp<PawnMemoryComp>() != null;
        }

        /// <summary>
        /// Get memory summary for debugging
        /// </summary>
        public static string GetMemorySummary(Pawn pawn)
        {
            var memoryComp = pawn?.TryGetComp<PawnMemoryComp>();
            if (memoryComp == null) return "No memory component";

            int shortTerm = memoryComp.ShortTermMemories.Count;
            int longTerm = memoryComp.LongTermMemories.Count;
            
            return $"{pawn.LabelShort}: {shortTerm} short-term, {longTerm} long-term memories";
        }
    }

    /// <summary>
    /// AI-powered memory summarizer using RimTalk's API
    /// 使用 RimTalk 的 API 进行 AI 驱动的记忆总结
    /// </summary>
    public static class RimTalkAISummarizer
    {
        private static Type talkServiceType = null;
        private static MethodInfo generateTalkMethod = null;
        private static bool isAvailable = false;

        public static bool IsAvailable => isAvailable;

        public static void Initialize()
        {
            try
            {
                Log.Message("[RimTalk AI Summarizer] Initializing...");
                
                // 列出所有可用的程序集（调试用）
                if (Prefs.DevMode)
                {
                    Log.Message("[RimTalk AI Summarizer] Available assemblies:");
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (asm.GetName().Name.Contains("RimTalk"))
                        {
                            Log.Message($"  - {asm.GetName().Name} (v{asm.GetName().Version})");
                        }
                    }
                }
                
                // 查找 RimTalk 主 Mod 的程序集
                var rimTalkAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "RimTalk");

                if (rimTalkAssembly == null)
                {
                    Log.Warning("[RimTalk AI Summarizer] ❌ RimTalk main mod not found");
                    Log.Warning("[RimTalk AI Summarizer] AI summarization will be DISABLED");
                    Log.Warning("[RimTalk AI Summarizer] Fallback: Simple summaries will be used");
                    return;
                }
                
                Log.Message($"[RimTalk AI Summarizer] ✓ Found RimTalk assembly: {rimTalkAssembly.FullName}");

                // 查找 TalkService 类型
                talkServiceType = rimTalkAssembly.GetType("RimTalk.Service.TalkService");
                if (talkServiceType == null)
                {
                    Log.Warning("[RimTalk AI Summarizer] ❌ TalkService type not found");
                    
                    // 列出所有类型（调试用）
                    if (Prefs.DevMode)
                    {
                        Log.Message("[RimTalk AI Summarizer] Available types in RimTalk:");
                        foreach (var type in rimTalkAssembly.GetTypes().Take(20))
                        {
                            Log.Message($"  - {type.FullName}");
                        }
                    }
                    
                    return;
                }
                
                Log.Message($"[RimTalk AI Summarizer] ✓ Found TalkService type");

                // 查找 GenerateTalk 方法
                generateTalkMethod = talkServiceType.GetMethod("GenerateTalk", 
                    BindingFlags.Public | BindingFlags.Static);

                if (generateTalkMethod == null)
                {
                    Log.Warning("[RimTalk AI Summarizer] ❌ GenerateTalk method not found");
                    
                    // 列出所有方法（调试用）
                    if (Prefs.DevMode)
                    {
                        Log.Message("[RimTalk AI Summarizer] Available methods in TalkService:");
                        foreach (var method in talkServiceType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                        {
                            var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                            Log.Message($"  - {method.ReturnType.Name} {method.Name}({parameters})");
                        }
                    }
                    
                    return;
                }
                
                // 显示方法签名
                var paramInfo = string.Join(", ", generateTalkMethod.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Log.Message($"[RimTalk AI Summarizer] ✓ Found GenerateTalk: {generateTalkMethod.ReturnType.Name} GenerateTalk({paramInfo})");

                isAvailable = true;
                Log.Message("[RimTalk AI Summarizer] ✅ AI summarization ENABLED!");
            }
            catch (Exception ex)
            {
                Log.Error($"[RimTalk AI Summarizer] ❌ Initialization failed:");
                Log.Error($"  Exception: {ex.GetType().Name}");
                Log.Error($"  Message: {ex.Message}");
                Log.Error($"  StackTrace: {ex.StackTrace}");
                isAvailable = false;
            }
        }

        /// <summary>
        /// 使用 RimTalk AI 总结记忆列表
        /// </summary>
        public static string SummarizeMemories(Pawn pawn, System.Collections.Generic.List<MemoryEntry> memories, MemoryType type)
        {
            if (!isAvailable || memories == null || memories.Count == 0)
                return null;

            try
            {
                // 构建总结提示词
                string prompt = BuildSummarizationPrompt(pawn, memories, type);

                // 调用 RimTalk API
                var result = generateTalkMethod.Invoke(null, new object[] { pawn, prompt, null });
                
                if (result != null)
                {
                    string summary = result.ToString();
                    Log.Message($"[RimTalk AI Summarizer] Generated summary for {pawn.LabelShort}: {summary.Substring(0, System.Math.Min(50, summary.Length))}...");
                    return summary;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimTalk AI Summarizer] Failed to generate summary: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 使用自定义提示词进行 AI 总结
        /// </summary>
        public static string SummarizeMemoriesWithPrompt(Pawn pawn, string customPrompt)
        {
            if (!isAvailable)
            {
                if (Prefs.DevMode)
                    Log.Message($"[RimTalk AI Summarizer] API not available, skipping AI summary");
                return null;
            }
            
            if (string.IsNullOrEmpty(customPrompt))
            {
                Log.Warning($"[RimTalk AI Summarizer] Empty prompt provided");
                return null;
            }

            try
            {
                Log.Message($"[RimTalk AI Summarizer] Calling API for {pawn.LabelShort} (prompt length: {customPrompt.Length})");
                
                // 调用 RimTalk API
                var result = generateTalkMethod.Invoke(null, new object[] { pawn, customPrompt, null });
                
                if (result == null)
                {
                    Log.Warning($"[RimTalk AI Summarizer] API returned null for {pawn.LabelShort}");
                    return null;
                }
                
                string summary = result.ToString();
                
                if (string.IsNullOrEmpty(summary))
                {
                    Log.Warning($"[RimTalk AI Summarizer] API returned empty summary for {pawn.LabelShort}");
                    return null;
                }
                
                Log.Message($"[RimTalk AI Summarizer] ✅ Generated summary for {pawn.LabelShort}: {summary.Substring(0, System.Math.Min(60, summary.Length))}...");
                return summary;
            }
            catch (TargetInvocationException ex)
            {
                // 方法调用时的异常
                Log.Error($"[RimTalk AI Summarizer] ❌ API invocation failed for {pawn.LabelShort}:");
                Log.Error($"  Inner Exception: {ex.InnerException?.GetType().Name}");
                Log.Error($"  Message: {ex.InnerException?.Message}");
                if (Prefs.DevMode)
                    Log.Error($"  StackTrace: {ex.InnerException?.StackTrace}");
            }
            catch (ArgumentException ex)
            {
                // 参数错误
                Log.Error($"[RimTalk AI Summarizer] ❌ Invalid arguments for {pawn.LabelShort}:");
                Log.Error($"  Message: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 其他异常
                Log.Error($"[RimTalk AI Summarizer] ❌ Unexpected error for {pawn.LabelShort}:");
                Log.Error($"  Exception: {ex.GetType().Name}");
                Log.Error($"  Message: {ex.Message}");
                if (Prefs.DevMode)
                    Log.Error($"  StackTrace: {ex.StackTrace}");
            }

            return null;
        }

        /// <summary>
        /// 构建 AI 总结的提示词
        /// </summary>
        private static string BuildSummarizationPrompt(Pawn pawn, System.Collections.Generic.List<MemoryEntry> memories, MemoryType type)
        {
            var prompt = new System.Text.StringBuilder();
            
            prompt.AppendLine($"请为 {pawn.LabelShort} 总结以下{memories.Count}条记忆。");
            prompt.AppendLine($"记忆类型：{type}");
            prompt.AppendLine();
            prompt.AppendLine("核心要求：");
            prompt.AppendLine("1. **只保留关键信息**：WHO（谁）、WHERE（哪里）、WHAT（做了什么/说了什么）");
            prompt.AppendLine("2. **去除所有修饰语**：不要形容词、副词、情感描述");
            prompt.AppendLine("3. **极简表达**：用最少的词表达完整信息");
            prompt.AppendLine("4. **统计频率**：相似事件合并并注明次数（如 ×3）");
            prompt.AppendLine("5. **长度限制**：总结不超过60字");
            prompt.AppendLine();
            prompt.AppendLine("格式示例：");
            prompt.AppendLine("- 对话类：\"与 Mary 交谈工作安排×3、讨论食物储备×2\"");
            prompt.AppendLine("- 行动类：\"烹饪简单餐食×5、清理房间×2、种植土豆×3\"");
            prompt.AppendLine("- 互动类：\"与 John 闲聊×4、与 Sarah 深谈×1\"");
            prompt.AppendLine();
            prompt.AppendLine("原始记忆：");
            
            int index = 1;
            foreach (var memory in memories.Take(20))
            {
                prompt.AppendLine($"{index}. {memory.content}");
                index++;
            }
            
            prompt.AppendLine();
            prompt.AppendLine("请直接输出总结（不要解释）：");

            return prompt.ToString();
        }
    }

    /// <summary>
    /// InteractionWorker patch - REMOVED
    /// 
    /// 互动记忆功能已完全移除，原因：
    /// 1. 互动记忆只有类型标签（如"闲聊"），无具体对话内容
    /// 2. RimTalk对话记忆已完整记录所有对话内容
    /// 3. 互动记忆与对话记忆冗余，无实际价值
    /// 4. 实现复杂，容易产生重复记录等bug
    /// 5. 不符合用户期望（用户需要的是对话内容，不是互动类型标签）
    /// 
    /// 现在只保留：
    /// - 对话记忆（Conversation）：RimTalk生成的完整对话内容
    /// - 行动记忆（Action）：工作、战斗等行为记录
    /// </summary>

    /// <summary>
    /// Helper to get private/public properties via reflection
    /// </summary>
    public static class ReflectionHelper
    {
        public static T GetProp<T>(this object obj, string propertyName) where T : class
        {
            try
            {
                var traverse = Traverse.Create(obj);
                return traverse.Field(propertyName).GetValue<T>() ?? 
                       traverse.Property(propertyName).GetValue<T>();
            }
            catch
            {
                return null;
            }
        }
    }
}
