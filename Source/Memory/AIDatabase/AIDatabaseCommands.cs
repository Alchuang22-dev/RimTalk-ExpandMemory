using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Verse;

namespace RimTalk.Memory.AIDatabase
{
    /// <summary>
    /// AI数据库命令处理器
    /// v3.3.1
    /// 
    /// 识别并处理AI对话中的数据库查询命令
    /// 
    /// 命令格式:
    /// [DB:查询内容]  - 查询数据库
    /// [RECALL:事件]  - 回忆事件
    /// [SEARCH:关键词] - 搜索记忆
    /// 
    /// 示例:
    /// AI说: "让我想想... [DB:我和Alice的对话] 啊，我记得..."
    /// → 系统自动查询数据库并替换为结果
    /// </summary>
    public static class AIDatabaseCommands
    {
        // 命令模式
        private static readonly Regex DbQueryPattern = new Regex(@"\[DB:([^\]]+)\]", RegexOptions.IgnoreCase);
        private static readonly Regex RecallPattern = new Regex(@"\[RECALL:([^\]]+)\]", RegexOptions.IgnoreCase);
        private static readonly Regex SearchPattern = new Regex(@"\[SEARCH:([^\]]+)\]", RegexOptions.IgnoreCase);
        
        /// <summary>
        /// 处理AI响应中的数据库命令
        /// ? v3.3.2: 优化为隐藏式处理，减少token消耗
        /// </summary>
        /// <param name="response">AI原始响应</param>
        /// <param name="speaker">说话者</param>
        /// <returns>处理后的响应（用户可见）和内部上下文（仅供AI下次使用）</returns>
        public static ProcessedResponse ProcessDatabaseCommands(string response, Pawn speaker)
        {
            if (string.IsNullOrEmpty(response) || speaker == null)
            {
                return new ProcessedResponse 
                { 
                    UserVisibleText = response, 
                    InternalContext = "" 
                };
            }
            
            try
            {
                string processedText = response;
                var internalContext = new System.Text.StringBuilder();
                int commandsProcessed = 0;
                
                // 处理 [DB:xxx] 命令
                processedText = ProcessDbQueries(processedText, speaker, internalContext, ref commandsProcessed);
                
                // 处理 [RECALL:xxx] 命令
                processedText = ProcessRecallCommands(processedText, speaker, internalContext, ref commandsProcessed);
                
                // 处理 [SEARCH:xxx] 命令
                processedText = ProcessSearchCommands(processedText, speaker, internalContext, ref commandsProcessed);
                
                if (commandsProcessed > 0 && Prefs.DevMode)
                {
                    Log.Message($"[AI Database Commands] Processed {commandsProcessed} commands");
                }
                
                return new ProcessedResponse
                {
                    UserVisibleText = processedText,
                    InternalContext = internalContext.Length > 0 ? internalContext.ToString() : ""
                };
            }
            catch (Exception ex)
            {
                Log.Error($"[AI Database Commands] Processing error: {ex.Message}");
                return new ProcessedResponse 
                { 
                    UserVisibleText = response, 
                    InternalContext = "" 
                };
            }
        }
        
        /// <summary>
        /// 处理 [DB:查询] 命令
        /// ? 优化：隐藏命令，结果存入内部上下文
        /// </summary>
        private static string ProcessDbQueries(string text, Pawn speaker, System.Text.StringBuilder internalContext, ref int commandCount)
        {
            var matches = DbQueryPattern.Matches(text);
            
            if (matches.Count == 0)
                return text;
            
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string result = text;
            
            foreach (Match match in matches)
            {
                string query = match.Groups[1].Value;
                string queryResult = AIDatabaseInterface.QueryDatabase(query, speaker, maxResults: 3);
                
                // ? 隐藏命令，用简短提示替代
                string userVisible = $"??"; // 仅显示思考图标
                
                // ? 完整结果存入内部上下文
                if (!string.IsNullOrEmpty(queryResult))
                {
                    internalContext.AppendLine($"[查询：{query}]");
                    internalContext.AppendLine(queryResult);
                    internalContext.AppendLine();
                }
                else
                {
                    internalContext.AppendLine($"[查询：{query}] 无结果");
                }
                
                result = result.Replace(match.Value, userVisible);
                commandCount++;
            }
            
            sw.Stop();
            if (sw.ElapsedMilliseconds > 100)
            {
                Monitoring.PerformanceMonitor.RecordPerformance("AIDatabase_DBCommand", sw.ElapsedMilliseconds);
            }
            
            return result;
        }
        
        /// <summary>
        /// 处理 [RECALL:xxx] 命令
        /// </summary>
        private static string ProcessRecallCommands(string text, Pawn speaker, System.Text.StringBuilder internalContext, ref int commandCount)
        {
            var matches = RecallPattern.Matches(text);
            
            if (matches.Count == 0)
                return text;
            
            string result = text;
            
            foreach (Match match in matches)
            {
                string topic = match.Groups[1].Value;
                string recallResult = AIDatabaseInterface.QueryDatabase($"回忆{topic}", speaker, maxResults: 2);
                
                // ? 隐藏命令
                string userVisible = $"??";
                
                if (!string.IsNullOrEmpty(recallResult))
                {
                    internalContext.AppendLine($"[回忆：{topic}]");
                    internalContext.AppendLine(recallResult);
                    internalContext.AppendLine();
                }
                
                result = result.Replace(match.Value, userVisible);
                commandCount++;
            }
            
            return result;
        }
        
        /// <summary>
        /// 处理 [SEARCH:xxx] 命令
        /// </summary>
        private static string ProcessSearchCommands(string text, Pawn speaker, System.Text.StringBuilder internalContext, ref int commandCount)
        {
            var matches = SearchPattern.Matches(text);
            
            if (matches.Count == 0)
                return text;
            
            string result = text;
            
            foreach (Match match in matches)
            {
                string keywords = match.Groups[1].Value;
                string searchResult = AIDatabaseInterface.QueryDatabase(keywords, speaker, maxResults: 3);
                
                // ? 隐藏命令
                string userVisible = $"??";
                
                if (!string.IsNullOrEmpty(searchResult))
                {
                    internalContext.AppendLine($"[搜索：{keywords}]");
                    internalContext.AppendLine(searchResult);
                    internalContext.AppendLine();
                }
                
                result = result.Replace(match.Value, userVisible);
                commandCount++;
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// 处理后的响应结果
    /// </summary>
    public class ProcessedResponse
    {
        /// <summary>
        /// 用户可见的文本（命令已隐藏）
        /// </summary>
        public string UserVisibleText;
        
        /// <summary>
        /// 内部上下文（查询结果，仅供AI下次对话使用）
        /// </summary>
        public string InternalContext;
    }
}
