using System;
using HarmonyLib;
using Verse;
using RimWorld;

namespace RimTalk.Memory.Patches
{
    /// <summary>
    /// 拦截游戏Message，实时生成事件记录常识
    /// 只补丁最常用的三参数版本
    /// </summary>
    [HarmonyPatch(typeof(Messages))]
    [HarmonyPatch(nameof(Messages.Message))]
    [HarmonyPatch(new Type[] { typeof(string), typeof(MessageTypeDef), typeof(bool) })]
    public static class MessagesPatch
    {
        [HarmonyPostfix]
        public static void Postfix(string text, MessageTypeDef def, bool historical)
        {
            try
            {
                if (Prefs.DevMode)
                {
                    Log.Message($"[EventRecord Patch] Message intercepted: {text?.Substring(0, Math.Min(30, text?.Length ?? 0))}... type={def?.defName}");
                }
                
                // 只处理重要消息类型
                if (def == MessageTypeDefOf.PositiveEvent ||
                    def == MessageTypeDefOf.NegativeEvent ||
                    def == MessageTypeDefOf.NeutralEvent ||
                    def == MessageTypeDefOf.ThreatBig ||
                    def == MessageTypeDefOf.ThreatSmall)
                {
                    EventRecordKnowledgeGenerator.OnMessageReceived(text, def);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[EventRecord Patch] Error in Messages patch: {ex.Message}");
            }
        }
    }
}
