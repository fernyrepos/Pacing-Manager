using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;

namespace ProgressionPacing
{
    [HarmonyPatch(typeof(QuestScriptDef), nameof(QuestScriptDef.CanRun), typeof(Slate), typeof(IIncidentTarget))]
    public static class QuestScriptDef_CanRun_EraGate_Patch
    {
        public static void Postfix(QuestScriptDef __instance, ref bool __result)
        {
            if (!__result) return;
            if (!ProgressionPacingModSettings.IsQuestGeneratorAllowed(__instance.defName))
            {
                __result = false;
            }
        }
    }
}
