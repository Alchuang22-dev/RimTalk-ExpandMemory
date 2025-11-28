using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimTalk.Memory.RAG;
using RimTalk.Memory.VectorDB;
using RimTalk.Memory.AI;

namespace RimTalk.Memory.AIDatabase
{
    /// <summary>
    /// AI数据库接口 - 为AI提供数据库查询能力
    /// v3.3.1
    /// 
    /// 功能：
    /// - 自然语言查询转换
    /// - 向量数据库检索
    /// - 结果格式化为AI可读格式
    /// - 查询优化和缓存
    /// 
    /// 使用场景：
    /// - AI对话时主动查询记忆
    /// - AI回忆特定事件
    /// - AI分析人物关系
    /// </summary>
    public static class AIDatabaseInterface
    {
        // 查询缓存
        private static Dictionary<string, CachedQuery> queryCache = new Dictionary<string, CachedQuery>();
        private const int MAX_CACHE_SIZE = 50;
        private const int CACHE_TTL_TICKS = 2500; // 100秒
        
        /// <summary>
        /// AI查询数据库（同步）
        /// ? v3.3.2: 增加超时时间和智能降级
        /// </summary>
        /// <param name="query">自然语言查询，如"我和Alice的对话记录"</param>
        /// <param name="speaker">查询者</param>
        /// <param name="maxResults">最大结果数</param>
        /// <returns>格式化的结果文本</returns>
        public static string QueryDatabase(string query, Pawn speaker, int maxResults = 5)
        {
            if (string.IsNullOrEmpty(query) || speaker == null)
                return "";
            
            try
            {
                // 检查缓存
                string cacheKey = $"{speaker.thingIDNumber}_{query.GetHashCode()}";
                if (TryGetCached(cacheKey, out string cached))
                {
                    // ? 减少缓存命中日志
                    if (Prefs.DevMode && UnityEngine.Random.value < 0.1f)
                    {
                        Log.Message($"[AI Database] Cache hit");
                    }
                    
                    return cached;
                }
                
                // ? 快速失败：如果缓存未命中且有其他查询在进行，跳过
                // （简单的并发控制，避免多个查询同时阻塞）
                if (!TryGetCached(cacheKey, out cached))
                {
                    // 如果已经有很多查询在等待，直接返回空避免卡顿
                    lock (queryCache)
                    {
                        if (queryCache.Count >= MAX_CACHE_SIZE)
                        {
                            if (Prefs.DevMode)
                                Log.Warning("[AI Database] Cache full, skipping query to avoid lag");
                            return "";
                        }
                    }
                }
                
                // 提取查询参数
                var queryParams = ParseQuery(query, speaker);
                
                // ? 根据是否启用语义嵌入，调整超时时间
                int timeout = RimTalk.MemoryPatch.RimTalkMemoryPatchMod.Settings.enableSemanticEmbedding 
                    ? 500  // 启用语义嵌入：500ms
                    : 100; // 仅关键词：100ms
                
                // 执行检索
                var ragConfig = new RAGConfig
                {
                    MaxResults = maxResults,
                    UseSemanticScoring = RimTalk.MemoryPatch.RimTalkMemoryPatchMod.Settings.enableSemanticEmbedding
                };
                
                var result = RAGManager.Retrieve(
                    query,
                    speaker,
                    queryParams.TargetPawn,
                    ragConfig,
                    timeoutMs: timeout // ? 动态超时
                );
                
                // 格式化结果
                string formatted = FormatResultsForAI(result, queryParams);
                
                // 缓存结果
                CacheQuery(cacheKey, formatted);
                
                // ? 只在有结果时输出日志
                if (Prefs.DevMode && result.TotalMatches > 0)
                {
                    Log.Message($"[AI Database] Query: '{query.Substring(0, Math.Min(20, query.Length))}...' -> {result.TotalMatches} matches");
                }
                
                return formatted;
            }
            catch (Exception ex)
            {
                // 静默失败，不影响游戏
                if (Prefs.DevMode)
                {
                    Log.Warning($"[AI Database] Query failed (non-critical): {ex.Message}");
                }
                return "";
            }
        }
        
        /// <summary>
        /// AI查询数据库（异步）
        /// </summary>
        public static async Task<string> QueryDatabaseAsync(string query, Pawn speaker, int maxResults = 5)
        {
            if (string.IsNullOrEmpty(query) || speaker == null)
                return "";
            
            try
            {
                var queryParams = ParseQuery(query, speaker);
                
                var ragConfig = new RAGConfig
                {
                    MaxResults = maxResults,
                    UseSemanticScoring = RimTalk.MemoryPatch.RimTalkMemoryPatchMod.Settings.enableSemanticEmbedding
                };
                
                var result = await RAGManager.RetrieveAsync(
                    query,
                    speaker,
                    queryParams.TargetPawn,
                    ragConfig
                );
                
                return FormatResultsForAI(result, queryParams);
            }
            catch (Exception ex)
            {
                Log.Error($"[AI Database] Async query failed: {ex.Message}");
                return "";
            }
        }
        
        /// <summary>
        /// 解析查询参数
        /// </summary>
        private static QueryParams ParseQuery(string query, Pawn speaker)
        {
            var result = new QueryParams
            {
                OriginalQuery = query,
                Speaker = speaker
            };
            
            // 提取目标人物（简单的关键词匹配）
            if (Find.CurrentMap != null)
            {
                foreach (var pawn in Find.CurrentMap.mapPawns.FreeColonists)
                {
                    if (query.Contains(pawn.LabelShort) || query.Contains(pawn.Name?.ToStringShort))
                    {
                        result.TargetPawn = pawn;
                        result.TargetPawnName = pawn.LabelShort;
                        break;
                    }
                }
            }
            
            // 提取时间范围（简单实现）
            if (query.Contains("今天") || query.Contains("最近"))
            {
                result.TimeRange = 1; // 1天
            }
            else if (query.Contains("昨天"))
            {
                result.TimeRange = 2; // 2天
            }
            else if (query.Contains("最近一周") || query.Contains("这周"))
            {
                result.TimeRange = 7; // 7天
            }
            
            // 提取记忆类型
            if (query.Contains("对话") || query.Contains("聊天") || query.Contains("说"))
            {
                result.MemoryType = MemoryType.Conversation;
            }
            else if (query.Contains("战斗") || query.Contains("打") || query.Contains("攻击"))
            {
                result.MemoryType = MemoryType.Action;
            }
            else if (query.Contains("事件") || query.Contains("发生"))
            {
                result.MemoryType = MemoryType.Event;
            }
            
            return result;
        }
        
        /// <summary>
        /// 格式化结果为AI可读格式
        /// </summary>
        private static string FormatResultsForAI(RAGResult result, QueryParams queryParams)
        {
            if (result == null || result.TotalMatches == 0)
            {
                return $"[数据库查询结果: 未找到相关记录]";
            }
            
            var sb = new StringBuilder();
            sb.AppendLine($"[数据库查询结果: 找到 {result.TotalMatches} 条相关记录]");
            sb.AppendLine();
            
            int index = 1;
            foreach (var match in result.RerankedMatches.Take(5)) // 最多返回5条
            {
                if (match.Memory != null)
                {
                    var memory = match.Memory;
                    
                    // 格式：序号. [时间] [类型] 内容 (相关度XX%)
                    sb.Append($"{index}. ");
                    
                    // 时间
                    int ticksPerDay = 60000; // RimWorld中1天=60000 ticks
                    if (memory.Age < ticksPerDay)
                    {
                        sb.Append("[今天] ");
                    }
                    else if (memory.Age < ticksPerDay * 2)
                    {
                        sb.Append("[昨天] ");
                    }
                    else
                    {
                        int days = memory.Age / ticksPerDay;
                        sb.Append($"[{days}天前] ");
                    }
                    
                    // 类型
                    sb.Append($"[{memory.TypeName}] ");
                    
                    // 内容
                    string content = memory.content;
                    if (content.Length > 100)
                    {
                        content = content.Substring(0, 100) + "...";
                    }
                    sb.Append(content);
                    
                    // 相关度
                    sb.Append($" (相关度:{match.FinalScore:P0})");
                    
                    sb.AppendLine();
                    index++;
                }
                else if (match.Knowledge != null)
                {
                    var knowledge = match.Knowledge;
                    
                    sb.Append($"{index}. ");
                    sb.Append("[常识] ");
                    
                    string content = knowledge.content;
                    if (content.Length > 100)
                    {
                        content = content.Substring(0, 100) + "...";
                    }
                    sb.Append(content);
                    
                    sb.Append($" (相关度:{match.FinalScore:P0})");
                    sb.AppendLine();
                    index++;
                }
            }
            
            sb.AppendLine();
            sb.AppendLine("[提示: 以上信息来自记忆数据库，可作为回忆参考]");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 缓存管理
        /// </summary>
        private static bool TryGetCached(string key, out string result)
        {
            result = null;
            
            lock (queryCache)
            {
                if (queryCache.TryGetValue(key, out CachedQuery cached))
                {
                    int age = Find.TickManager.TicksGame - cached.CachedAt;
                    
                    if (age < CACHE_TTL_TICKS)
                    {
                        result = cached.Result;
                        return true;
                    }
                    else
                    {
                        queryCache.Remove(key);
                    }
                }
            }
            
            return false;
        }
        
        private static void CacheQuery(string key, string result)
        {
            lock (queryCache)
            {
                if (queryCache.Count >= MAX_CACHE_SIZE)
                {
                    var toRemove = queryCache.Keys.Take(MAX_CACHE_SIZE / 2).ToList();
                    foreach (var k in toRemove)
                    {
                        queryCache.Remove(k);
                    }
                }
                
                queryCache[key] = new CachedQuery
                {
                    Result = result,
                    CachedAt = Find.TickManager.TicksGame
                };
            }
        }
        
        /// <summary>
        /// 清空缓存
        /// </summary>
        public static void ClearCache()
        {
            lock (queryCache)
            {
                queryCache.Clear();
            }
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public static string GetStats()
        {
            lock (queryCache)
            {
                return $"AI Database: {queryCache.Count} cached queries";
            }
        }
    }
    
    #region 数据结构
    
    /// <summary>
    /// 查询参数
    /// </summary>
    internal class QueryParams
    {
        public string OriginalQuery;
        public Pawn Speaker;
        public Pawn TargetPawn;
        public string TargetPawnName;
        public int TimeRange; // 天数
        public MemoryType? MemoryType;
    }
    
    /// <summary>
    /// 缓存的查询
    /// </summary>
    internal class CachedQuery
    {
        public string Result;
        public int CachedAt;
    }
    
    #endregion
}
