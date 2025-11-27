using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimTalk.Memory
{
    /// <summary>
    /// 智能注入管理器 v3.0
    /// 整合高级评分系统，提供统一的注入接口
    /// </summary>
    public static class SmartInjectionManager
    {
        /// <summary>
        /// 智能注入记忆和常识（统一入口）
        /// </summary>
        /// <param name="speaker">说话者</param>
        /// <param name="listener">听众</param>
        /// <param name="context">对话上下文</param>
        /// <param name="maxMemories">最大记忆数</param>
        /// <param name="maxKnowledge">最大常识数</param>
        /// <returns>格式化的注入文本（为空则返回空字符串）</returns>
        public static string InjectSmartContext(
            Pawn speaker,
            Pawn listener,
            string context,
            int maxMemories = 8,
            int maxKnowledge = 5)
        {
            // 1. 获取记忆内容
            string memories = InjectSmartMemories(speaker, context, maxMemories, listener);
            
            // 2. 获取常识内容
            string knowledge = InjectSmartKnowledge(context, maxKnowledge, speaker, listener);

            // 3. 零结果检查：如果都为空，直接返回空字符串（不注入任何内容）
            if (string.IsNullOrEmpty(memories) && string.IsNullOrEmpty(knowledge))
            {
                Log.Message("[Smart Injection] No relevant memories or knowledge found - skipping injection to save tokens");
                return string.Empty;
            }

            // 4. 构建注入内容（仅包含有效部分）
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(memories))
            {
                sb.AppendLine("## Recent Memories");
                sb.AppendLine(memories);
                if (!string.IsNullOrEmpty(knowledge))
                {
                    sb.AppendLine(); // 只在有两部分时添加分隔
                }
            }

            if (!string.IsNullOrEmpty(knowledge))
            {
                sb.AppendLine("## Knowledge Base");
                sb.AppendLine(knowledge);
            }

            string result = sb.ToString();
            
            // 5. 记录注入统计（用于调试）
            int memoryCount = string.IsNullOrEmpty(memories) ? 0 : memories.Split('\n').Length - 1;
            int knowledgeCount = string.IsNullOrEmpty(knowledge) ? 0 : knowledge.Split('\n').Length - 1;
            Log.Message($"[Smart Injection] Injected {memoryCount} memories, {knowledgeCount} knowledge");

            return result;
        }

        /// <summary>
        /// 智能注入记忆
        /// </summary>
        public static string InjectSmartMemories(
            Pawn pawn,
            string context,
            int maxCount = 8,
            Pawn listener = null)
        {
            // 1. 验证输入
            if (pawn == null)
            {
                Log.Warning("[Smart Injection] Speaker is null, skipping memory injection");
                return string.Empty;
            }

            var memoryComp = pawn.TryGetComp<FourLayerMemoryComp>();
            if (memoryComp == null)
            {
                Log.Warning($"[Smart Injection] {pawn.LabelShort} has no memory component");
                return string.Empty;
            }

            // 2. 收集所有记忆
            var allMemories = new List<MemoryEntry>();
            allMemories.AddRange(memoryComp.SituationalMemories);
            allMemories.AddRange(memoryComp.EventLogMemories);

            // 根据上下文判断是否需要归档记忆
            if (ShouldIncludeArchive(context))
            {
                allMemories.AddRange(memoryComp.ArchiveMemories.Take(10));
            }

            // 3. 零记忆检查
            if (allMemories.Count == 0)
            {
                Log.Message($"[Smart Injection] No memories available for {pawn.LabelShort}");
                return string.Empty;
            }

            // 4. 使用高级评分系统
            var scored = AdvancedScoringSystem.ScoreMemories(
                allMemories,
                context,
                pawn,
                listener);

            // 5. 获取阈值并过滤
            float threshold = RimTalk.MemoryPatch.RimTalkMemoryPatchMod.Settings?.memoryScoreThreshold ?? 0.15f;

            var selected = scored
                .Where(s => s.Score >= threshold)
                .Take(maxCount)
                .ToList();

            // 6. 零结果检查：低于阈值的全部过滤
            if (selected.Count == 0)
            {
                Log.Message($"[Smart Injection] All memories scored below threshold ({threshold:F2}) - no injection");
                return string.Empty; // ? 返回空字符串，不注入任何内容
            }

            // 7. 格式化输出
            bool compress = RimTalk.MemoryPatch.RimTalkMemoryPatchMod.Settings?.enableMemoryCompression ?? false;
            
            if (compress)
            {
                var memories = selected.Select(s => s.Item).ToList();
                return MemoryCompressor.CompressMemories(memories, 500);
            }

            return FormatMemories(selected);
        }

        /// <summary>
        /// 智能注入常识
        /// </summary>
        public static string InjectSmartKnowledge(
            string context,
            int maxCount = 5,
            Pawn speaker = null,
            Pawn listener = null)
        {
            // 1. 获取常识库
            var library = MemoryManager.GetCommonKnowledge();
            if (library == null || library.Entries.Count == 0)
            {
                Log.Message("[Smart Injection] No knowledge library available");
                return string.Empty;
            }

            // 2. 过滤可用常识
            var available = library.Entries
                .Where(e => e.isEnabled)
                .Where(e => e.targetPawnId == -1 || // 全局
                           (speaker != null && e.targetPawnId == speaker.thingIDNumber))
                .ToList();

            // 3. 零常识检查
            if (available.Count == 0)
            {
                Log.Message("[Smart Injection] No enabled knowledge entries");
                return string.Empty;
            }

            // 4. 使用高级评分系统
            var scored = AdvancedScoringSystem.ScoreKnowledge(
                available,
                context,
                speaker,
                listener);

            // 5. 获取阈值并过滤
            float threshold = RimTalk.MemoryPatch.RimTalkMemoryPatchMod.Settings?.knowledgeScoreThreshold ?? 0.1f;

            var selected = scored
                .Where(s => s.Score >= threshold)
                .Take(maxCount)
                .ToList();

            // 6. 零结果检查：低于阈值的全部过滤
            if (selected.Count == 0)
            {
                Log.Message($"[Smart Injection] All knowledge scored below threshold ({threshold:F2}) - no injection");
                return string.Empty; // ? 返回空字符串，不注入任何内容
            }

            // 7. 格式化输出
            bool compress = RimTalk.MemoryPatch.RimTalkMemoryPatchMod.Settings?.enableKnowledgeCompression ?? false;
            
            if (compress)
            {
                var entries = selected.Select(s => s.Item).ToList();
                return KnowledgeCompressor.CompressKnowledge(entries, 300);
            }

            return FormatKnowledge(selected);
        }

        #region 格式化输出

        /// <summary>
        /// 格式化记忆（生产模式：简洁输出）
        /// </summary>
        private static string FormatMemories(List<ScoredItem<MemoryEntry>> scored)
        {
            var sb = new StringBuilder();
            int index = 1;

            foreach (var item in scored)
            {
                var memory = item.Item;
                string typeTag = GetMemoryTypeTag(memory.type);
                string timeStr = memory.TimeAgoString;

                sb.AppendLine($"{index}. [{typeTag}] {memory.content} ({timeStr})");
                
                // ? 移除调试输出（减少Token消耗）
                // 如需调试，请使用 GetPreviewData() 方法
                
                index++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// 格式化常识（生产模式：简洁输出）
        /// </summary>
        private static string FormatKnowledge(List<ScoredItem<CommonKnowledgeEntry>> scored)
        {
            var sb = new StringBuilder();
            int index = 1;

            foreach (var item in scored)
            {
                var entry = item.Item;
                sb.AppendLine($"{index}. [{entry.tag}] {entry.content}");
                
                // ? 移除调试输出（减少Token消耗）
                
                index++;
            }

            return sb.ToString();
        }

        private static string GetMemoryTypeTag(MemoryType type)
        {
            switch (type)
            {
                case MemoryType.Conversation:
                    return "Conv";
                case MemoryType.Action:
                    return "Act";
                case MemoryType.Event:
                    return "Event";
                case MemoryType.Emotion:
                    return "Emot";
                case MemoryType.Relationship:
                    return "Rel";
                default:
                    return "Mem";
            }
        }

        #endregion

        #region 辅助方法

        private static bool ShouldIncludeArchive(string context)
        {
            if (string.IsNullOrEmpty(context))
                return false;

            string[] archiveKeywords = { "过去", "以前", "曾经", "记得", "回忆", "历史", "当时", "那时候" };
            
            foreach (var keyword in archiveKeywords)
            {
                if (context.Contains(keyword))
                    return true;
            }

            return false;
        }

        #endregion

        #region 预览和调试

        /// <summary>
        /// 获取详细评分信息（用于预览窗口）
        /// </summary>
        public static InjectionPreviewData GetPreviewData(
            Pawn speaker,
            Pawn listener,
            string context)
        {
            var data = new InjectionPreviewData
            {
                Speaker = speaker?.LabelShort ?? "Unknown",
                Listener = listener?.LabelShort ?? "None",
                Context = context
            };

            // 记忆评分
            if (speaker != null)
            {
                var memoryComp = speaker.TryGetComp<FourLayerMemoryComp>();
                if (memoryComp != null)
                {
                    var allMemories = new List<MemoryEntry>();
                    allMemories.AddRange(memoryComp.SituationalMemories);
                    allMemories.AddRange(memoryComp.EventLogMemories);

                    data.MemoryScores = AdvancedScoringSystem.ScoreMemories(
                        allMemories,
                        context,
                        speaker,
                        listener);
                }
            }

            // 常识评分
            var library = MemoryManager.GetCommonKnowledge();
            if (library != null)
            {
                var available = library.Entries
                    .Where(e => e.isEnabled)
                    .Where(e => e.targetPawnId == -1 || 
                               (speaker != null && e.targetPawnId == speaker.thingIDNumber))
                    .ToList();

                data.KnowledgeScores = AdvancedScoringSystem.ScoreKnowledge(
                    available,
                    context,
                    speaker,
                    listener);
            }

            return data;
        }

        /// <summary>
        /// 预览数据结构
        /// </summary>
        public class InjectionPreviewData
        {
            public string Speaker;
            public string Listener;
            public string Context;
            public List<ScoredItem<MemoryEntry>> MemoryScores;
            public List<ScoredItem<CommonKnowledgeEntry>> KnowledgeScores;
        }

        #endregion
    }
}
