using System;
using System.Linq;
using Verse;
using RimWorld;

namespace RimTalk.Memory
{
    /// <summary>
    /// 自动生成Pawn状态常识（新人/老人标识）
    /// </summary>
    public static class PawnStatusKnowledgeGenerator
    {
        /// <summary>
        /// 更新所有殖民者的状态常识
        /// </summary>
        public static void UpdateAllColonistStatus()
        {
            var library = MemoryManager.GetCommonKnowledge();
            if (library == null) return;

            int currentTick = Find.TickManager.TicksGame;

            foreach (var map in Find.Maps)
            {
                foreach (var pawn in map.mapPawns.FreeColonists)
                {
                    UpdatePawnStatusKnowledge(pawn, library, currentTick);
                }
            }
        }

        /// <summary>
        /// 为单个Pawn更新状态常识
        /// </summary>
        public static void UpdatePawnStatusKnowledge(Pawn pawn, CommonKnowledgeLibrary library, int currentTick)
        {
            if (pawn == null || library == null) return;

            // 计算在殖民地的时间
            int joinTick = pawn.records.GetAsInt(RecordDefOf.TimeAsColonistOrColonyAnimal);
            int daysInColony = joinTick / GenDate.TicksPerDay;

            // 查找是否已有该Pawn的状态常识
            string statusTag = $"{pawn.LabelShort},状态";
            var existingEntry = library.Entries.FirstOrDefault(e => e.tag == statusTag);

            string content = GenerateStatusContent(pawn, daysInColony);
            float importance = GetStatusImportance(daysInColony);

            if (existingEntry != null)
            {
                // 更新现有常识
                if (existingEntry.content != content)
                {
                    existingEntry.content = content;
                    existingEntry.importance = importance;
                    
                    if (Prefs.DevMode)
                    {
                        Log.Message($"[PawnStatus] Updated status for {pawn.LabelShort}: {content}");
                    }
                }
            }
            else
            {
                // 创建新常识
                var newEntry = new CommonKnowledgeEntry(statusTag, content)
                {
                    importance = importance,
                    isEnabled = true
                };
                
                library.AddEntry(newEntry);
                
                if (Prefs.DevMode)
                {
                    Log.Message($"[PawnStatus] Created status for {pawn.LabelShort}: {content}");
                }
            }
        }

        /// <summary>
        /// 生成状态描述文本
        /// 增强版：自动包含种族信息，可部分取代成员名单
        /// </summary>
        private static string GenerateStatusContent(Pawn pawn, int daysInColony)
        {
            string name = pawn.LabelShort;
            string race = pawn.def?.label ?? "未知种族";
            
            if (daysInColony < 1)
            {
                // 刚加入（不到1天）
                return $"{name}({race})是今天刚加入殖民地的新成员，对这里还很陌生，不了解殖民地的历史";
            }
            else if (daysInColony < 3)
            {
                // 1-3天
                return $"{name}({race})是{daysInColony}天前加入的新成员，刚开始熟悉殖民地，不了解之前的事情";
            }
            else if (daysInColony < 7)
            {
                // 3-7天
                return $"{name}({race})加入殖民地已经{daysInColony}天，还在适应期，对殖民地的过往不太了解";
            }
            else if (daysInColony < 15)
            {
                // 1-2周
                return $"{name}({race})已经在殖民地生活了{daysInColony}天，开始融入集体，但对更早的历史还不清楚";
            }
            else if (daysInColony < 30)
            {
                // 2-4周
                return $"{name}({race})已经是殖民地的一员（{daysInColony}天），熟悉最近的事情，但对更早的历史了解有限";
            }
            else if (daysInColony < 60)
            {
                // 1-2个月
                return $"{name}({race})已经在殖民地生活了{daysInColony}天，是正式成员，了解最近一个月的事件";
            }
            else
            {
                // 2个月以上 - 老成员
                return $"{name}({race})是殖民地的老成员（{daysInColony}天），见证了殖民地的发展历程";
            }
        }

        /// <summary>
        /// 根据在殖民地的时间计算重要性
        /// </summary>
        private static float GetStatusImportance(int daysInColony)
        {
            if (daysInColony < 7)
            {
                // 新成员：高重要性，需要强调
                return 0.95f;
            }
            else if (daysInColony < 30)
            {
                // 中期成员：中等重要性
                return 0.80f;
            }
            else
            {
                // 老成员：较低重要性（不需要特别强调）
                return 0.60f;
            }
        }

        /// <summary>
        /// 清理过期的状态常识（当Pawn离开或死亡）
        /// </summary>
        public static void CleanupPawnStatusKnowledge(Pawn pawn)
        {
            if (pawn == null) return;

            var library = MemoryManager.GetCommonKnowledge();
            if (library == null) return;

            string statusTag = $"{pawn.LabelShort},状态";
            var entry = library.Entries.FirstOrDefault(e => e.tag == statusTag);
            
            if (entry != null)
            {
                library.RemoveEntry(entry);
                
                if (Prefs.DevMode)
                {
                    Log.Message($"[PawnStatus] Removed status for {pawn.LabelShort}");
                }
            }
        }
    }
}
