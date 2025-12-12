using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace RimTalk.Memory
{
    /// <summary>
    /// 动态记忆注入系统 - 现实且严格的场景感知权重
    /// ⭐ v3.3.12: 重构为"无上帝模式"设计
    /// 设计哲学：
    /// - 没有完美回忆：即使紧急情况，角色也会遗忘旧细节
    /// - 场景聚焦：根据场景调整关注点，但严格遵守评分结果
    /// - 时间衰减永不为0：所有记忆都会随时间淡化
    /// </summary>
    public static class DynamicMemoryInjection
    {
        /// <summary>
        /// 场景类型（简化版）
        /// </summary>
        private enum SceneType
        {
            Casual,      // 日常/通用
            Emergency,   // 紧急/战斗
            Emotional,   // 情感/社交
            Work         // 工作/任务
        }
        
        /// <summary>
        /// 场景权重配置
        /// </summary>
        private class SceneWeights
        {
            public float TimeDecay;          // 时间衰减（永不为0）
            public float Importance;         // 重要性权重
            public float KeywordMatch;       // 关键词匹配权重
            public float RelationshipBonus;  // 关系加成权重
        }
        
        /// <summary>
        /// 静态权重配置（用户覆盖和固定记忆）
        /// </summary>
        public static class Weights
        {
            public static float LayerBonus = 0.2f;       // 层级加成
            public static float PinnedBonus = 0.5f;      // 固定记忆加成（绝对优先级）
            public static float UserEditedBonus = 0.3f;  // 用户编辑加成（绝对优先级）
        }

        /// <summary>
        /// 动态注入记忆到对话提示词
        /// </summary>
        public static string InjectMemories(Pawn pawn, string context, int maxMemories = 10)
        {
            return InjectMemoriesWithDetails(pawn.TryGetComp<FourLayerMemoryComp>(), context, maxMemories, out _);
        }

        /// <summary>
        /// 动态注入记忆（带详细评分信息）- 用于预览
        /// ⭐ v3.3.12: 使用现实场景感知权重
        /// </summary>
        public static string InjectMemoriesWithDetails(
            FourLayerMemoryComp memoryComp, 
            string context, 
            int maxMemories,
            out List<MemoryScore> scores)
        {
            scores = new List<MemoryScore>();

            if (memoryComp == null)
                return string.Empty;

            // ⭐ v3.3.12: 步骤1 - 场景识别
            SceneType sceneType = IdentifyScene(context);
            
            // ⭐ v3.3.12: 步骤2 - 获取场景专属权重
            SceneWeights sceneWeights = GetWeightsForScene(sceneType);
            
            // 开发模式日志
            if (Prefs.DevMode)
            {
                Log.Message($"[Memory Injection] Scene: {sceneType}");
                Log.Message($"[Memory Injection] Weights: TimeDecay={sceneWeights.TimeDecay:F2}, " +
                           $"Importance={sceneWeights.Importance:F2}, " +
                           $"KeywordMatch={sceneWeights.KeywordMatch:F2}, " +
                           $"Relationship={sceneWeights.RelationshipBonus:F2}");
            }

            // 提取上下文关键词
            List<string> contextKeywords = ExtractKeywords(context);

            // 收集记忆：跳过超短期记忆(ABM)，只收集SCM、ELS、CLPA
            var allMemories = new List<MemoryEntry>();
            allMemories.AddRange(memoryComp.SituationalMemories);
            allMemories.AddRange(memoryComp.EventLogMemories);
            
            // 根据场景决定是否包含归档记忆
            // 情感场景更倾向于包含长期记忆（讲故事）
            if (sceneType == SceneType.Emotional || ShouldIncludeArchive(context))
            {
                allMemories.AddRange(memoryComp.ArchiveMemories.Take(20));
            }

            if (allMemories.Count == 0)
                return string.Empty;

            // 获取阈值设置
            float threshold = RimTalk.MemoryPatch.RimTalkMemoryPatchMod.Settings?.memoryScoreThreshold ?? 0.15f;

            // ⭐ v3.3.12: 步骤3 - 使用场景权重计算评分
            var scoredMemories = allMemories
                .Select(m => new ScoredMemory
                {
                    Memory = m,
                    Score = CalculateMemoryScore(m, contextKeywords, sceneWeights, memoryComp)
                })
                .Where(sm => sm.Score >= threshold)
                .OrderByDescending(sm => sm.Score)
                .Take(maxMemories)
                .ToList();

            // 如果没有记忆达到阈值，返回null
            if (scoredMemories.Count == 0)
            {
                if (Prefs.DevMode)
                {
                    Log.Message($"[Memory Injection] No memories met threshold ({threshold:F2}), returning null");
                }
                return null;
            }

            // 生成详细评分信息
            foreach (var scored in scoredMemories)
            {
                float timeScore = CalculateTimeDecayScore(scored.Memory, sceneWeights.TimeDecay);
                float importanceScore = scored.Memory.importance * sceneWeights.Importance;
                float keywordScore = CalculateKeywordMatchScore(scored.Memory, contextKeywords) * sceneWeights.KeywordMatch;
                
                float bonusScore = GetLayerBonus(scored.Memory.layer) * Weights.LayerBonus;
                
                // 关系加成
                var pawn = memoryComp.parent as Pawn;
                float relationshipScore = CalculateRelationshipBonus(scored.Memory, pawn) * sceneWeights.RelationshipBonus;
                bonusScore += relationshipScore;
                
                // ⭐ 用户覆盖：绝对优先级（不受场景影响）
                if (scored.Memory.isPinned) 
                    bonusScore += Weights.PinnedBonus;
                if (scored.Memory.isUserEdited) 
                    bonusScore += Weights.UserEditedBonus;

                scores.Add(new MemoryScore
                {
                    Memory = scored.Memory,
                    TotalScore = scored.Score,
                    TimeScore = timeScore,
                    ImportanceScore = importanceScore,
                    KeywordScore = keywordScore,
                    BonusScore = bonusScore
                });
            }

            // 构建注入文本
            return FormatMemoriesForInjection(scoredMemories);
        }
        
        /// <summary>
        /// ⭐ v3.3.12: 场景识别（基于关键词检测）
        /// 现实设计：简单关键词匹配，不过度复杂化
        /// </summary>
        private static SceneType IdentifyScene(string context)
        {
            if (string.IsNullOrEmpty(context))
                return SceneType.Casual;
            
            string lowerContext = context.ToLower();
            
            // 紧急场景（战斗/危险）
            string[] emergencyKeywords = { "raid", "draft", "attack", "danger", "blood", "kill", "enemy", 
                                          "袭击", "攻击", "危险", "战斗", "敌人", "受伤", "流血", "死亡" };
            if (emergencyKeywords.Any(kw => lowerContext.Contains(kw)))
                return SceneType.Emergency;
            
            // 情感场景（社交/情绪）
            string[] emotionalKeywords = { "love", "hate", "marry", "divorce", "mood", "friend", 
                                          "喜欢", "爱", "讨厌", "恨", "结婚", "离婚", "心情", "朋友", "聊天", "对话" };
            if (emotionalKeywords.Any(kw => lowerContext.Contains(kw)))
                return SceneType.Emotional;
            
            // 工作场景（建造/制造/种植）
            string[] workKeywords = { "work", "build", "craft", "sow", "construct", "repair", 
                                     "工作", "建造", "制作", "种植", "修理", "建筑" };
            if (workKeywords.Any(kw => lowerContext.Contains(kw)))
                return SceneType.Work;
            
            // 默认：日常
            return SceneType.Casual;
        }
        
        /// <summary>
        /// ⭐ v3.3.12: 获取场景专属权重（现实且严格）
        /// 核心原则：
        /// - TimeDecay 永不为0（所有记忆都会衰减）
        /// - 场景只改变"关注焦点"，不创造上帝模式
        /// </summary>
        private static SceneWeights GetWeightsForScene(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.Emergency:
                    // 紧急场景（战斗）
                    // 设计：高度聚焦当前威胁，但仍会遗忘旧战术
                    return new SceneWeights
                    {
                        TimeDecay = 0.3f,        // 标准衰减（旧记忆仍会遗忘）
                        Importance = 0.6f,       // 高重要性（生死攸关）
                        KeywordMatch = 0.8f,     // 极高关键词（紧盯"敌人"、"武器"）
                        RelationshipBonus = 0.1f // 低关系（战斗时不关心友谊）
                    };
                
                case SceneType.Emotional:
                    // 情感场景（社交/对话）
                    // 设计：略微放松时间限制，但仍会淡忘往事
                    return new SceneWeights
                    {
                        TimeDecay = 0.15f,       // 放松衰减（讲故事模式）
                        Importance = 0.3f,       // 中等重要性（小事也能聊）
                        KeywordMatch = 0.4f,     // 中等关键词（话题宽泛）
                        RelationshipBonus = 0.5f // 高关系加成（与听众的共同记忆）
                    };
                
                case SceneType.Work:
                    // 工作场景（建造/制造）
                    // 设计：强调近期任务，快速遗忘旧工作
                    return new SceneWeights
                    {
                        TimeDecay = 0.5f,        // 严格衰减（专注最近任务）
                        Importance = 0.4f,       // 中高重要性
                        KeywordMatch = 0.6f,     // 高关键词（任务相关）
                        RelationshipBonus = 0.2f // 低关系（工作不关心情感）
                    };
                
                case SceneType.Casual:
                default:
                    // 日常场景（默认平衡）
                    return new SceneWeights
                    {
                        TimeDecay = 0.3f,
                        Importance = 0.3f,
                        KeywordMatch = 0.4f,
                        RelationshipBonus = 0.25f
                    };
            }
        }
        
        /// <summary>
        /// ⭐ v3.3.12: 计算记忆评分（使用场景权重）
        /// 严格遵守评分结果，无特殊照顾
        /// </summary>
        private static float CalculateMemoryScore(
            MemoryEntry memory, 
            List<string> contextKeywords, 
            SceneWeights sceneWeights,
            FourLayerMemoryComp memoryComp)
        {
            float score = 0f;

            // 1. 时间衰减分数（根据场景调整，但永不为0）
            float timeScore = CalculateTimeDecayScore(memory, sceneWeights.TimeDecay);
            score += timeScore * sceneWeights.TimeDecay;

            // 2. 重要性分数
            score += memory.importance * sceneWeights.Importance;

            // 3. 关键词匹配分数
            float keywordScore = CalculateKeywordMatchScore(memory, contextKeywords);
            score += keywordScore * sceneWeights.KeywordMatch;

            // 4. 层级加成
            float layerBonus = GetLayerBonus(memory.layer);
            score += layerBonus * Weights.LayerBonus;
            
            // 5. 关系加成
            var pawn = memoryComp.parent as Pawn;
            float relationshipBonus = CalculateRelationshipBonus(memory, pawn);
            score += relationshipBonus * sceneWeights.RelationshipBonus;

            // 6. ⭐ 用户覆盖（绝对优先级，不受场景影响）
            if (memory.isPinned)
                score += Weights.PinnedBonus;
            
            if (memory.isUserEdited)
                score += Weights.UserEditedBonus;

            // 7. 活跃度加成
            score += memory.activity * 0.1f;

            return score;
        }

        /// <summary>
        /// 计算时间衰减分数（指数衰减，永不为0）
        /// </summary>
        private static float CalculateTimeDecayScore(MemoryEntry memory, float decayRate)
        {
            int currentTick = Find.TickManager.TicksGame;
            int age = currentTick - memory.timestamp;

            // 使用指数衰减：e^(-age * decayRate)
            // 半衰期约为：1 / decayRate 天
            float normalizedAge = age / 60000f; // 转换为游戏天数
            return UnityEngine.Mathf.Exp(-normalizedAge * decayRate);
        }

        /// <summary>
        /// 计算关键词匹配分数
        /// </summary>
        private static float CalculateKeywordMatchScore(MemoryEntry memory, List<string> contextKeywords)
        {
            if (contextKeywords == null || contextKeywords.Count == 0)
                return 0f;

            if (memory.keywords == null || memory.keywords.Count == 0)
                return 0f;

            // 使用 Jaccard 相似度
            var intersection = memory.keywords.Intersect(contextKeywords).Count();
            var union = memory.keywords.Union(contextKeywords).Count();

            if (union == 0)
                return 0f;

            float jaccardSimilarity = (float)intersection / union;

            // 同时考虑内容直接匹配
            float contentMatch = 0f;
            foreach (var keyword in contextKeywords)
            {
                if (memory.content.Contains(keyword))
                    contentMatch += 0.2f;
            }

            return UnityEngine.Mathf.Min(jaccardSimilarity + contentMatch, 1f);
        }
        
        /// <summary>
        /// 计算关系加成（记忆涉及的人物与当前Pawn的关系）
        /// </summary>
        private static float CalculateRelationshipBonus(MemoryEntry memory, Pawn currentPawn)
        {
            if (string.IsNullOrEmpty(memory.relatedPawnId) || currentPawn == null)
                return 0f;
            
            // 查找关联的Pawn
            Pawn relatedPawn = null;
            foreach (var map in Find.Maps)
            {
                relatedPawn = map.mapPawns.AllPawns.FirstOrDefault(p => 
                    p.ThingID == memory.relatedPawnId || 
                    p.LabelShort == memory.relatedPawnName
                );
                
                if (relatedPawn != null)
                    break;
            }
            
            if (relatedPawn == null)
                return 0f;
            
            // 计算关系亲密度
            if (currentPawn.relations == null || relatedPawn.relations == null)
                return 0f;
            
            // 检查是否有直接关系
            var directRelations = currentPawn.relations.DirectRelations
                .Where(r => r.otherPawn == relatedPawn)
                .ToList();
            
            if (directRelations.Any())
            {
                // 配偶/恋人：高加成
                if (directRelations.Any(r => r.def == PawnRelationDefOf.Spouse || r.def == PawnRelationDefOf.Lover))
                    return 1.0f;
                
                // 家人：中等加成
                if (directRelations.Any(r => r.def == PawnRelationDefOf.Parent || 
                                            r.def == PawnRelationDefOf.Child ||
                                            r.def == PawnRelationDefOf.Sibling))
                    return 0.6f;
                
                // 其他关系：小加成
                return 0.3f;
            }
            
            // 检查好感度
            int opinion = currentPawn.relations.OpinionOf(relatedPawn);
            if (opinion > 50)
                return 0.5f;
            else if (opinion > 20)
                return 0.3f;
            else if (opinion < -20)
                return 0.2f; // 负面关系也值得记忆
            
            return 0f;
        }

        /// <summary>
        /// 获取层级加成
        /// </summary>
        private static float GetLayerBonus(MemoryLayer layer)
        {
            switch (layer)
            {
                case MemoryLayer.Active:
                    return 1.0f;
                case MemoryLayer.Situational:
                    return 0.7f;
                case MemoryLayer.EventLog:
                    return 0.4f;
                case MemoryLayer.Archive:
                    return 0.2f;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// 判断是否应该包含归档记忆
        /// </summary>
        private static bool ShouldIncludeArchive(string context)
        {
            if (string.IsNullOrEmpty(context))
                return false;

            // 检测是否提到过去、历史等关键词
            string[] archiveKeywords = { "过去", "以前", "曾经", "记得", "回忆", "历史", "当时", "那时候" };
            
            foreach (var keyword in archiveKeywords)
            {
                if (context.Contains(keyword))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 提取上下文关键词（确定性双重策略）
        /// </summary>
        private static List<string> ExtractKeywords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            // 截断过长文本
            const int MAX_TEXT_LENGTH = 500;
            if (text.Length > MAX_TEXT_LENGTH)
            {
                text = text.Substring(0, MAX_TEXT_LENGTH);
            }

            // 使用超级关键词引擎获取候选词
            var weightedKeywords = SuperKeywordEngine.ExtractKeywords(text, 100);
            
            if (weightedKeywords.Count == 0)
            {
                return new List<string>();
            }
            
            // 核心词：按长度降序 + 字母顺序升序，取前10个
            var sortedByLength = weightedKeywords
                .OrderByDescending(kw => kw.Word.Length)
                .ThenBy(kw => kw.Word, StringComparer.Ordinal)
                .ToList();
            
            var coreKeywords = sortedByLength.Take(10).ToList();
            
            // 模糊词：从剩余池按字母顺序选10个
            var remainingPool = sortedByLength.Skip(10).ToList();
            var fuzzyKeywords = new List<WeightedKeyword>();
            
            if (remainingPool.Count > 0)
            {
                fuzzyKeywords = remainingPool
                    .OrderBy(kw => kw.Word, StringComparer.Ordinal)
                    .Take(10)
                    .ToList();
            }
            
            // 合并核心词 + 模糊词（最多20个）
            var finalKeywords = new List<string>();
            finalKeywords.AddRange(coreKeywords.Select(kw => kw.Word));
            finalKeywords.AddRange(fuzzyKeywords.Select(kw => kw.Word));
            
            return finalKeywords;
        }

        /// <summary>
        /// 格式化记忆用于注入到System Rule
        /// </summary>
        private static string FormatMemoriesForInjection(List<ScoredMemory> scoredMemories)
        {
            if (scoredMemories == null || scoredMemories.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            // 按层级分组显示
            var byLayer = scoredMemories.GroupBy(sm => sm.Memory.layer);

            int index = 1;
            foreach (var group in byLayer.OrderBy(g => g.Key))
            {
                foreach (var scored in group)
                {
                    var memory = scored.Memory;
                    
                    // 简洁格式，适合system rule
                    string typeTag = GetMemoryTypeTag(memory.type);
                    string timeStr = memory.TimeAgoString;
                    
                    sb.AppendLine($"{index}. [{typeTag}] {memory.content} ({timeStr})");
                    index++;
                }
            }

            return sb.ToString();
        }
        
        /// <summary>
        /// 获取记忆类型标签
        /// </summary>
        private static string GetMemoryTypeTag(MemoryType type)
        {
            switch (type)
            {
                case MemoryType.Conversation:
                    return "Conversation";
                case MemoryType.Action:
                    return "Action";
                case MemoryType.Observation:
                    return "Observation";
                case MemoryType.Event:
                    return "Event";
                case MemoryType.Emotion:
                    return "Emotion";
                case MemoryType.Relationship:
                    return "Relationship";
                default:
                    return "Memory";
            }
        }

        /// <summary>
        /// 评分后的记忆
        /// </summary>
        private class ScoredMemory
        {
            public MemoryEntry Memory;
            public float Score;
        }

        /// <summary>
        /// 记忆评分详情
        /// </summary>
        public class MemoryScore
        {
            public MemoryEntry Memory;
            public float TotalScore;
            public float TimeScore;
            public float ImportanceScore;
            public float KeywordScore;
            public float BonusScore;
        }
    }
}
