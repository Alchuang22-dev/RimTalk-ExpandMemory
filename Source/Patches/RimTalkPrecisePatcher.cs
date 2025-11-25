using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using RimTalk.MemoryPatch;

namespace RimTalk.Memory.Patches
{
    /// <summary>
    /// 针对 RimTalk 的精确补丁 - 基于实际代码结构
    /// </summary>
    [StaticConstructorOnStartup]
    public static class RimTalkPrecisePatcher
    {
        private const string VERSION = "v7.FINAL"; // <-- 新增版本标记

        static RimTalkPrecisePatcher()
        {
            try
            {
                var harmony = new Harmony("rimtalk.memory.precise");
                
                // 查找 RimTalk 程序集
                Assembly rimTalkAssembly = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "RimTalk")
                    {
                        rimTalkAssembly = assembly;
                        break;
                    }
                }
                
                if (rimTalkAssembly == null) return;
                
                // 应用补丁
                bool patchedBuildContext = PatchBuildContext(harmony, rimTalkAssembly);
                bool patchedDecoratePrompt = PatchDecoratePrompt(harmony, rimTalkAssembly);
                bool patchedGenerateTalk = PatchGenerateTalk(harmony, rimTalkAssembly);
                
                int successCount = (patchedBuildContext ? 1 : 0) + 
                                  (patchedDecoratePrompt ? 1 : 0) + 
                                  (patchedGenerateTalk ? 1 : 0);
                
                if (successCount == 0)
                {
                    Log.Error($"[RimTalk Patcher] Failed to patch any methods");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[RimTalk Patcher] Exception: {ex}");
            }
        }
        
        /// <summary>
        /// 补丁 PromptService.BuildContext
        /// </summary>
        private static bool PatchBuildContext(Harmony harmony, Assembly assembly)
        {
            try
            {
                // RimTalk.Service.PromptService
                var promptServiceType = assembly.GetType("RimTalk.Service.PromptService");
                if (promptServiceType == null) return false;
                
                // BuildContext 方法
                var buildContextMethod = promptServiceType.GetMethod("BuildContext", 
                    BindingFlags.Public | BindingFlags.Static);
                
                if (buildContextMethod == null) return false;
                
                // 应用 Postfix
                var postfixMethod = typeof(RimTalkPrecisePatcher).GetMethod(
                    nameof(BuildContext_Postfix), 
                    BindingFlags.Static | BindingFlags.NonPublic);
                
                harmony.Patch(buildContextMethod, postfix: new HarmonyMethod(postfixMethod));
                
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimTalk Patcher] Failed to patch BuildContext: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 补丁 PromptService.DecoratePrompt
        /// </summary>
        private static bool PatchDecoratePrompt(Harmony harmony, Assembly assembly)
        {
            try
            {
                var promptServiceType = assembly.GetType("RimTalk.Service.PromptService");
                if (promptServiceType == null) return false;
                
                // DecoratePrompt 方法
                var decoratePromptMethod = promptServiceType.GetMethod("DecoratePrompt", 
                    BindingFlags.Public | BindingFlags.Static);
                
                if (decoratePromptMethod == null)
                {
                    Log.Warning("[RimTalk Precise Patcher] DecoratePrompt method not found");
                    return false;
                }
                
                // 应用 Postfix
                var postfixMethod = typeof(RimTalkPrecisePatcher).GetMethod(
                    nameof(DecoratePrompt_Postfix), 
                    BindingFlags.Static | BindingFlags.NonPublic);
                
                harmony.Patch(decoratePromptMethod, postfix: new HarmonyMethod(postfixMethod));
                
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimTalk Patcher] Failed to patch DecoratePrompt: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 补丁 TalkService.GenerateTalk (备用方案)
        /// </summary>
        private static bool PatchGenerateTalk(Harmony harmony, Assembly assembly)
        {
            try
            {
                var talkServiceType = assembly.GetType("RimTalk.Service.TalkService");
                if (talkServiceType == null) return false;
                
                // GenerateTalk 方法
                var generateTalkMethod = talkServiceType.GetMethod("GenerateTalk", 
                    BindingFlags.Public | BindingFlags.Static);
                
                if (generateTalkMethod == null) return false;
                
                // 应用 Prefix (在方法执行前)
                var prefixMethod = typeof(RimTalkPrecisePatcher).GetMethod(
                    nameof(GenerateTalk_Prefix), 
                    BindingFlags.Static | BindingFlags.NonPublic);
                
                harmony.Patch(generateTalkMethod, prefix: new HarmonyMethod(prefixMethod));
                
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimTalk Patcher] Failed to patch GenerateTalk: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Postfix for BuildContext - 在构建上下文后添加记忆
        /// </summary>
        private static void BuildContext_Postfix(ref string __result, List<Pawn> pawns)
        {
            try
            {
                if (pawns == null || pawns.Count == 0 || string.IsNullOrEmpty(__result))
                    return;
                
                Pawn mainPawn = pawns[0];
                var memoryComp = mainPawn?.TryGetComp<FourLayerMemoryComp>();
                
                if (memoryComp == null)
                    return;
                
                // 缓存上下文到API（用于预览器）
                RimTalkMemoryAPI.CacheContext(mainPawn, __result);
                
                string memoryContext = "";
                
                // 使用动态注入或静态注入
                if (RimTalkMemoryPatchMod.Settings.useDynamicInjection)
                {
                    memoryContext = DynamicMemoryInjection.InjectMemories(
                        mainPawn, 
                        __result, 
                        RimTalkMemoryPatchMod.Settings.maxInjectedMemories
                    );
                    
                    // 注入常识库
                    var memoryManager = Find.World?.GetComponent<MemoryManager>();
                    if (memoryManager != null)
                    {
                        string knowledgeContext = memoryManager.CommonKnowledge.InjectKnowledgeWithDetails(
                            __result,
                            RimTalkMemoryPatchMod.Settings.maxInjectedKnowledge,
                            out _,
                            mainPawn,
                            pawns.Count > 1 ? pawns[1] : null
                        );
                        
                        if (!string.IsNullOrEmpty(knowledgeContext))
                        {
                            memoryContext = memoryContext + "\n\n" + knowledgeContext;
                        }
                    }
                }
                else
                {
                    var pawnMemoryComp = mainPawn.TryGetComp<PawnMemoryComp>();
                    if (pawnMemoryComp != null)
                    {
                        memoryContext = pawnMemoryComp.GetMemoryContext();
                    }
                }
                
                if (!string.IsNullOrEmpty(memoryContext))
                {
                    __result = __result + "\n\n" + memoryContext;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimTalk Patcher] Error in BuildContext_Postfix: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Postfix for DecoratePrompt - 在装饰提示词后添加记忆
        /// </summary>
        private static void DecoratePrompt_Postfix(object talkRequest, List<Pawn> pawns)
        {
            try
            {
                if (talkRequest == null || pawns == null || pawns.Count == 0)
                    return;
                
                // 通过反射获取 TalkRequest.Prompt 属性
                var talkRequestType = talkRequest.GetType();
                var promptProperty = talkRequestType.GetProperty("Prompt");
                
                if (promptProperty == null)
                    return;
                
                string currentPrompt = promptProperty.GetValue(talkRequest) as string;
                if (string.IsNullOrEmpty(currentPrompt))
                    return;
                
                // 获取主要 Pawn 的记忆
                Pawn mainPawn = pawns[0];
                var memoryComp = mainPawn?.TryGetComp<FourLayerMemoryComp>();
                
                if (memoryComp == null)
                    return;
                
                // 缓存上下文到API（用于预览器）
                RimTalkMemoryAPI.CacheContext(mainPawn, currentPrompt);
                
                string memoryContext = "";
                
                // 使用动态注入或静态注入
                if (RimTalkMemoryPatchMod.Settings.useDynamicInjection)
                {
                    // 动态注入
                    memoryContext = DynamicMemoryInjection.InjectMemories(
                        mainPawn, 
                        currentPrompt, 
                        RimTalkMemoryPatchMod.Settings.maxInjectedMemories
                    );
                    
                    // 注入常识库 - 传递当前Pawn以提取角色关键词
                    var memoryManager = Find.World?.GetComponent<MemoryManager>();
                    if (memoryManager != null)
                    {
                        // 尝试获取目标Pawn（如果有对话对象）
                        Pawn targetPawn = null;
                        if (pawns != null && pawns.Count > 1)
                        {
                            targetPawn = pawns[1];
                        }
                        
                        string knowledgeContext = memoryManager.CommonKnowledge.InjectKnowledgeWithDetails(
                            currentPrompt,
                            RimTalkMemoryPatchMod.Settings.maxInjectedKnowledge,
                            out _,
                            mainPawn,     // 当前Pawn
                            targetPawn    // 目标Pawn（可能为null）
                        );
                        
                        if (!string.IsNullOrEmpty(knowledgeContext))
                        {
                            memoryContext = memoryContext + "\n\n" + knowledgeContext;
                        }
                    }
                }
                else
                {
                    // 静态注入
                    var pawnMemoryComp = mainPawn.TryGetComp<PawnMemoryComp>();
                    if (pawnMemoryComp != null)
                    {
                        memoryContext = pawnMemoryComp.GetMemoryContext();
                    }
                }
                
                if (!string.IsNullOrEmpty(memoryContext))
                {
                    // 更新提示词
                    string enhancedPrompt = currentPrompt + "\n\n" + memoryContext;
                    promptProperty.SetValue(talkRequest, enhancedPrompt);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimTalk Patcher] Error in DecoratePrompt_Postfix: {ex.Message}");
            }
        }

        /// <summary>
        /// Prefix for GenerateTalk - 在生成对话前准备记忆上下文
        /// </summary>
        private static void GenerateTalk_Prefix(object talkRequest)
        {
            try
            {
                if (talkRequest == null)
                    return;
                
                // 通过反射获取 Initiator
                var talkRequestType = talkRequest.GetType();
                var initiatorProperty = talkRequestType.GetProperty("Initiator");
                
                if (initiatorProperty == null)
                    return;
                
                Pawn initiator = initiatorProperty.GetValue(talkRequest) as Pawn;
                if (initiator == null)
                    return;
                
                var memoryComp = initiator.TryGetComp<PawnMemoryComp>();
                if (memoryComp != null)
                {
                    // 备用方案，暂时不输出日志
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimTalk Precise Patcher] Error in GenerateTalk_Prefix: {ex.Message}");
            }
        }
    }
}
