using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using RimTalk.MemoryPatch;

namespace RimTalk.Memory
{
    /// <summary>
    /// 事件记录常识生成器
    /// 实时检测Message和PlayLog事件，根据关键词自动生成常识
    /// </summary>
    public static class EventRecordKnowledgeGenerator
    {
        // 已处理的事件ID（避免重复）
        private static HashSet<int> processedLogIDs = new HashSet<int>();
        private static HashSet<string> processedMessageHashes = new HashSet<string>();
        
        // 重要事件关键词配置
        private static readonly Dictionary<string, float> ImportantKeywords = new Dictionary<string, float>
        {
            // 死亡相关（重要性1.0）
            { "死亡", 1.0f }, { "死了", 1.0f }, { "被杀", 1.0f }, { "击杀", 1.0f }, { "阵亡", 1.0f },
            { "died", 1.0f }, { "killed", 1.0f }, { "death", 1.0f },
            
            // 战斗相关（重要性0.9）
            { "袭击", 0.9f }, { "进攻", 0.9f }, { "入侵", 0.9f }, { "raid", 0.9f }, { "attack", 0.9f },
            { "击退", 0.85f }, { "战胜", 0.85f }, { "defeated", 0.85f },
            
            // 关系相关（重要性0.85）
            { "结婚", 0.85f }, { "订婚", 0.85f }, { "married", 0.85f }, { "engaged", 0.85f },
            { "分手", 0.75f }, { "离婚", 0.75f }, { "breakup", 0.75f },
            
            // 成员变动（重要性0.8）
            { "加入", 0.8f }, { "逃跑", 0.8f }, { "离开", 0.8f }, { "joined", 0.8f }, { "fled", 0.8f },
            { "招募", 0.75f }, { "recruited", 0.75f },
            
            // 灾难相关（重要性0.85）
            { "爆炸", 0.85f }, { "火灾", 0.85f }, { "崩溃", 0.85f }, { "explosion", 0.85f }, { "fire", 0.85f },
            { "地震", 0.85f }, { "日食", 0.8f }, { "毒雨", 0.8f },
            
            // 贸易相关（重要性0.65）
            { "贸易", 0.65f }, { "商队", 0.65f }, { "trade", 0.65f }, { "caravan", 0.65f },
        };
        
        // ? 新增：低优先级关键词（不自动生成，除非重要）
        private static readonly HashSet<string> LowPriorityKeywords = new HashSet<string>
        {
            "建造", "完成", "研究", "built", "completed", "finished"
        };
        
        /// <summary>
        /// 处理Message（每次显示Message时调用）
        /// </summary>
        public static void OnMessageReceived(string message, MessageTypeDef messageType)
        {
            // ? 添加入口日志
            Log.Message($"[EventRecord] OnMessageReceived called: enabled={RimTalkMemoryPatchMod.Settings?.enableEventRecordKnowledge}, message={message?.Substring(0, Math.Min(30, message?.Length ?? 0))}...");
            
            if (!RimTalkMemoryPatchMod.Settings.enableEventRecordKnowledge)
            {
                Log.Message("[EventRecord] Feature is disabled in settings");
                return;
            }
            
            if (string.IsNullOrEmpty(message))
            {
                Log.Message("[EventRecord] Message is null or empty");
                return;
            }
            
            try
            {
                // 生成消息哈希值（避免重复处理）
                string messageHash = $"{message}_{Find.TickManager.TicksGame / 60}"; // 按分钟去重
                
                if (processedMessageHashes.Contains(messageHash))
                    return;
                
                processedMessageHashes.Add(messageHash);
                
                // 限制哈希表大小
                if (processedMessageHashes.Count > 1000)
                {
                    var toRemove = processedMessageHashes.Take(500).ToList();
                    foreach (var hash in toRemove)
                    {
                        processedMessageHashes.Remove(hash);
                    }
                }
                
                // 检查是否包含重要关键词
                var matchedKeywords = ImportantKeywords
                    .Where(kv => message.Contains(kv.Key))
                    .OrderByDescending(kv => kv.Value)
                    .ToList();
                
                // ? 如果没有高优先级关键词，检查是否有低优先级关键词
                bool hasLowPriority = false;
                if (matchedKeywords.Count == 0)
                {
                    hasLowPriority = LowPriorityKeywords.Any(k => message.Contains(k));
                    if (!hasLowPriority)
                        return; // 既没有高优先级也没有低优先级关键词
                }
                
                // ? 对于低优先级关键词，只在特定条件下生成
                if (hasLowPriority && matchedKeywords.Count == 0)
                {
                    // 低优先级事件默认不生成
                    return;
                }
                
                // 过滤无意义消息
                if (IsBoringMessage(message))
                    return;
                
                // ? 添加调试日志
                Log.Message($"[EventRecord] Message matched: {message.Substring(0, Math.Min(50, message.Length))}... (keywords: {string.Join(", ", matchedKeywords.Take(2).Select(kv => kv.Key))})");
                
                // 生成常识
                float importance = matchedKeywords[0].Value; // 使用最高重要性
                string tag = "事件,历史";
                
                // 添加时间前缀
                string timePrefix = GetTimePrefix();
                string content = $"{timePrefix}{message}";
                
                // 检查是否已存在
                var library = MemoryManager.GetCommonKnowledge();
                if (library != null)
                {
                    bool exists = library.Entries.Any(e => 
                        e.content.Contains(message.Substring(0, Math.Min(20, message.Length)))
                    );
                    
                    if (!exists)
                    {
                        var entry = new CommonKnowledgeEntry(tag, content)
                        {
                            importance = importance,
                            isEnabled = true,
                            isUserEdited = false
                        };
                        
                        library.AddEntry(entry);
                        
                        Log.Message($"[EventRecord] ? Created knowledge: {content.Substring(0, Math.Min(50, content.Length))}... (importance: {importance:F2})");
                    }
                    else
                    {
                        Log.Message($"[EventRecord] ?? Skipped duplicate: {message.Substring(0, Math.Min(30, message.Length))}...");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[EventRecord] Error processing message: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理PlayLog事件（定期扫描，每小时调用一次）
        /// </summary>
        public static void ScanRecentPlayLog()
        {
            if (!RimTalkMemoryPatchMod.Settings.enableEventRecordKnowledge)
                return;
            
            try
            {
                var gameHistory = Find.PlayLog;
                if (gameHistory == null)
                    return;
                
                var library = MemoryManager.GetCommonKnowledge();
                if (library == null)
                    return;
                
                int processedCount = 0;
                int currentTick = Find.TickManager.TicksGame;
                
                // 只处理最近1小时的事件
                int oneHourAgo = currentTick - 2500;
                
                var recentEntries = gameHistory.AllEntries
                    .Where(e => e != null && e.Age > oneHourAgo)
                    .OrderByDescending(e => e.Age)
                    .Take(50);
                
                foreach (var logEntry in recentEntries)
                {
                    try
                    {
                        // 使用LogEntry的ID去重
                        int logID = logEntry.GetHashCode();
                        
                        if (processedLogIDs.Contains(logID))
                            continue;
                        
                        processedLogIDs.Add(logID);
                        
                        // 限制集合大小
                        if (processedLogIDs.Count > 2000)
                        {
                            var toRemove = processedLogIDs.Take(1000).ToList();
                            foreach (var id in toRemove)
                            {
                                processedLogIDs.Remove(id);
                            }
                        }
                        
                        string eventText = ExtractEventInfo(logEntry);
                        
                        if (!string.IsNullOrEmpty(eventText))
                        {
                            // 检查是否已存在
                            bool exists = library.Entries.Any(e => 
                                e.content.Contains(eventText.Substring(0, Math.Min(15, eventText.Length)))
                            );
                            
                            if (!exists)
                            {
                                // 计算重要性
                                float importance = CalculateImportance(eventText);
                                
                                var entry = new CommonKnowledgeEntry("事件,历史", eventText)
                                {
                                    importance = importance,
                                    isEnabled = true,
                                    isUserEdited = false
                                };
                                
                                library.AddEntry(entry);
                                processedCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"[EventRecord] Error processing log entry: {ex.Message}");
                    }
                }
                
                if (processedCount > 0 && Prefs.DevMode)
                {
                    Log.Message($"[EventRecord] Processed {processedCount} new PlayLog events");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[EventRecord] Error scanning PlayLog: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 从LogEntry提取事件信息
        /// </summary>
        private static string ExtractEventInfo(LogEntry logEntry)
        {
            if (logEntry == null)
                return null;
            
            try
            {
                // 检查是否为交互日志（特殊处理，避免POV错误）
                if (logEntry.GetType().Name == "PlayLogEntry_Interaction")
                {
                    // 跳过交互日志，这些已经由RimTalk对话记忆处理
                    return null;
                }
                
                string text = logEntry.ToGameStringFromPOV(null, false);
                
                if (string.IsNullOrEmpty(text))
                    return null;
                
                // 过滤长度
                if (text.Length < 10 || text.Length > 200)
                    return null;
                
                // 过滤无意义内容
                if (IsBoringMessage(text))
                    return null;
                
                // 检查是否包含重要关键词
                bool hasImportantKeyword = ImportantKeywords.Keys.Any(k => text.Contains(k));
                
                if (!hasImportantKeyword)
                    return null;
                
                // 添加时间前缀
                int ticksAgo = Find.TickManager.TicksGame - logEntry.Age;
                int daysAgo = ticksAgo / GenDate.TicksPerDay;
                
                string timePrefix = "";
                if (daysAgo < 1)
                {
                    timePrefix = "今天";
                }
                else if (daysAgo < 3)
                {
                    timePrefix = $"{daysAgo}天前";
                }
                else if (daysAgo < 7)
                {
                    timePrefix = $"约{daysAgo}天前";
                }
                else
                {
                    return null; // 超过7天的事件不记录
                }
                
                return $"{timePrefix}{text}";
            }
            catch (Exception ex)
            {
                // 静默处理错误，避免日志污染
                if (Prefs.DevMode)
                {
                    Log.Warning($"[EventRecord] Error in ExtractEventInfo: {ex.Message}");
                }
                return null;
            }
        }
        
        /// <summary>
        /// 过滤无意义消息
        /// </summary>
        private static bool IsBoringMessage(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;
            
            var boringKeywords = new[] 
            { 
                "走路", "吃饭", "睡觉", "娱乐", "闲逛", "工作", "休息",
                "walking", "eating", "sleeping", "recreation", "wandering"
            };
            
            return boringKeywords.Any(k => text.Contains(k));
        }
        
        /// <summary>
        /// 计算事件重要性
        /// </summary>
        private static float CalculateImportance(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0.5f;
            
            // 查找匹配的关键词
            var matched = ImportantKeywords
                .Where(kv => text.Contains(kv.Key))
                .OrderByDescending(kv => kv.Value)
                .FirstOrDefault();
            
            if (matched.Key != null)
            {
                return matched.Value;
            }
            
            return 0.6f; // 默认重要性
        }
        
        /// <summary>
        /// 获取时间前缀（相对当前时间）
        /// </summary>
        private static string GetTimePrefix()
        {
            return "刚才"; // Message通常是实时的
        }
        
        /// <summary>
        /// 清理处理记录（定期维护）
        /// </summary>
        public static void CleanupProcessedRecords()
        {
            // 限制集合大小
            if (processedLogIDs.Count > 2000)
            {
                var toRemove = processedLogIDs.Take(1000).ToList();
                foreach (var id in toRemove)
                {
                    processedLogIDs.Remove(id);
                }
            }
            
            if (processedMessageHashes.Count > 1000)
            {
                var toRemove = processedMessageHashes.Take(500).ToList();
                foreach (var hash in toRemove)
                {
                    processedMessageHashes.Remove(hash);
                }
            }
        }
    }
}
